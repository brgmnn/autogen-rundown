# Travel Path Generation - Developer Guide

Deep dive on `TravelPathGenerator` and `Patch_SetupMovement`: how NavMesh walking circuits are built from AI graph nodes.

## Why NavMesh Instead of AI Graph Positions

The base game generates moving scan positions with `LG_NodeTools.TryGetPositionsOnRadiusDistancedFromEachother`, which picks random points within a radius. This produces a cluster of nearby positions — fine for small movement, but useless for zone-spanning walks.

We need a **walkable circuit**: a loop of positions that traces actual walking routes through the zone. NavMesh pathfinding guarantees every waypoint is reachable by walking, with no shortcuts through walls or jumps off ledges.

## Four-Leg Circuit Design

The path is a closed quadrilateral: `source → dest1 → dest2 → dest3 → source`.

```
      dest1 ————— dest2
       /             \
      /               \
source ————————————— dest3
```

Why a closed circuit instead of a random walk or two-point path:

- **Two points** (out-and-back) doubles back on itself — players walk the same corridor twice
- **Random walk** has no guaranteed loop closure — the last leg might be a long straight line
- **Closed circuit** covers four distinct routes through the zone with guaranteed closure back to source

The loop is set to `MovementType.Circular`, so after reaching the last waypoint the scan wraps back to position 0 (the source) and repeats indefinitely.

## Destination Selection

### Phase 1: Euclidean Pre-Filter

Sort all candidate nodes by Euclidean distance from source (descending) and take the top 20 (`CandidatePoolSize`). This is cheap — just `sqrMagnitude` comparisons — and eliminates nearby nodes that would produce tiny loops.

### Phase 2: NavMesh Distance Ranking

For the top 20 candidates, compute actual NavMesh walking distance:

**dest1**: Pick the node with the highest NavMesh distance from source. This maximizes the first leg length.

**dest2**: Pick the node that maximizes `min(navDist to source, navDist to dest1)`. This is a triangle-spread heuristic — it avoids picking a node that's far from source but right next to dest1 (which would collapse two legs into one). Nodes within 1.0 unit of dest1 are skipped entirely.

### Why Both Distance Metrics

Euclidean distance is cheap but misleading in complex zones — two nodes can be 50m apart in a straight line but 200m apart by walking (separated by walls). NavMesh distance is what matters for walkability, but computing it for every node is expensive (`NavMesh.CalculatePath` + summing corners). The Euclidean pre-filter narrows the field so we only run ~40 NavMesh distance calculations (20 for dest1, up to 20 for dest2) instead of hundreds.

## Node Filtering

`GatherCandidates` filters AI graph nodes from the source area's `AIG_NodeCluster`:

1. Uses `m_reachableNodes` (falls back to `m_nodes` if empty)
2. Requires `node.ClusterID == clusterId` (same cluster as source)
3. Requires `node.Links.Count >= 4` (non-edge nodes)

The link count filter avoids dead ends and narrow corridors. Nodes with ≥4 connections are typically at intersections or in open areas — better scan destinations where players have room to maneuver.

If fewer than 2 candidates pass, `GenerateLoop` returns an empty list and the system falls back to base game behavior (`Patch_SetupMovement` returns `true`).

## NavMesh Pathfinding

`AppendNavMeshLeg` traces each leg of the triangle:

```csharp
NavMesh.CalculatePath(from, to, -1, navPath)  // -1 = all NavMesh areas
```

- Requires `PathComplete` status and ≥2 corners
- Subsequent legs skip their first corner (it equals the previous leg's last corner)
- Falls back to a direct line if pathfinding fails (shouldn't happen for in-cluster nodes)

The four legs are appended into a single `rawPath` list. The final corner of the last leg equals `sourcePos`, which is not duplicated in the resampled output (handled by the caller inserting sourcePos as position 0).

## Resampling

Raw NavMesh corners are unevenly spaced — tight turns produce clusters of corners, straight corridors produce two corners far apart. The base game's `CP_BasicMovable` interpolates between consecutive waypoints at constant speed, so uneven spacing causes the scan to crawl through turns and sprint through corridors.

`ResamplePath` fixes this by walking along the raw path at fixed `StepDistance` (2m) intervals:

```
Raw:    ●————————————————●——●——●————————————●
                                             (unevenly spaced)

Resampled: ●——●——●——●——●——●——●——●——●——●——●
                                             (3m intervals)
```

### Algorithm

1. Start at `corners[0]`, add it (surface-snapped and edge-pulled) as the first point
2. Walk along each segment. When accumulated distance reaches `stepDistance`, emit a point
3. Carry leftover distance (`remaining`) into the next segment
4. **Do not** add the final corner — it's `sourcePos`, which the caller inserts as position 0

The walk cursor stays on the raw corners; only the emitted points are snapped. Snapping the cursor itself would let arc length drift.

This keeps the loop tight: the last resampled point is within `stepDistance` of the source, and `Circular` movement wraps back to position 0.

## Surface Snapping

Resampled points sit on straight chords *between* NavMesh corners, and Unity's funnel algorithm only emits a corner where the path turns **horizontally**. A straight run up a staircase therefore emits no corner at the crest — you get one corner low on the stairs and the next well out onto the upper floor, with every point in between buried in the geometry.

`SnapToSurface` projects each emitted point back onto the walkable surface before edge-pulling:

1. Sample at `point + Vector3.up * SurfaceSampleLift` (0.15m) — the same bias the game uses in `CP_Holopath_Spline.TryGetPosOnNavMesh`
2. `NavMesh.SamplePosition` with `SurfaceSampleRadius` (1.5m)
3. Reject the result if it moved the point more than `MaxSurfaceSnapRise` (1.5m) vertically — that means a *different floor* was found, not the surface under this point. Stacked geometry (two staircases close together) makes this a real risk, and `CP_PlayerScanner` tests scan membership with a full 3D sphere, so a waypoint on the wrong floor is a bubble players cannot stand in.

Snap runs **before** `AdjustForEdges` so edge distance is measured from a point that is actually on the mesh.

## Sag Subdivision

Snapping the waypoints is not enough on its own. `CP_BasicMovable.DoMoveScanner` moves the scan with a plain `Vector3.Lerp` between consecutive `ScanPositions`, so the scan travels along the *chord*, not the surface. At a convex break — the crest of a staircase — a chord between two correctly-placed waypoints still passes below the floor.

`SubdivideSaggingSegments` walks each pair of consecutive waypoints and recursively splits any whose chord sags:

```
before:  A●________________●B     chord cuts the crest
              ▁▂▃▄▅▆▇
            stairs   upper floor

after:   A●─●─●─●─●──────●B     crest forced as a waypoint
              ▁▂▃▄▅▆▇
```

1. Sample the chord midpoint on the surface
2. If `surfaceY - chordY > MaxChordSag` (0.25m), insert the surface point and recurse on both halves
3. Stop at `MaxSubdivisionDepth` (4), or when the segment is under `2 × MinSegmentLength`

Only **sag** is tested. A chord passing *above* the surface — the concave break at the foot of a staircase — just floats the scan slightly and is harmless, so testing one direction keeps the waypoint count down. Flat corridors gain no points at all.

Because movement is `Circular`, the wrap-around `last → sourcePos` segment is a real segment and is subdivided too; its interior points are kept and `sourcePos` itself is dropped, preserving `GenerateLoop`'s contract that the caller re-inserts it at index 0.

`MinSegmentLength` (0.35m) is not cosmetic: `DoMoveScanner` divides by segment length, and `Patch_SustainedTravelReverse.ReverseMovement` bails out below 0.001m, so a degenerate segment either teleports the scan or stalls reverse movement permanently.

**Gotcha:** `RemoveBunchedPoints` runs inside `ResamplePath` at `stepDistance * 0.5` (1m), *before* subdivision. Do not re-run it afterwards — it would delete exactly the crest waypoints subdivision just inserted.

Denser waypoints do not change scan speed in either direction: `DoMoveScanner` and `ReverseMovement` both divide by the actual segment length, so speed stays in world units per second.

## Edge Pulling

Scans have a radius. If a waypoint is right at a NavMesh edge, players standing in the scan would be pushed against walls or off ledges. `PullAwayFromEdge` ensures every waypoint has at least `EdgeDistance` (2m) of clearance:

1. `NavMesh.FindClosestEdge(position)` → get distance and surface normal to nearest edge
2. If `hit.distance < minDistance`, compute `newPos = position + hit.normal * pullAmount`
3. Validate with `NavMesh.SamplePosition(newPos, 0.5f)` — if the pulled position is off-mesh, keep the original

## Graceful Degradation

| Condition                                    | Behavior                                         |
| -------------------------------------------- | ------------------------------------------------ |
| `sourceArea.m_courseNode` not valid          | Returns empty list → base game fallback          |
| < 2 candidate nodes (small/degenerate zone)  | Returns empty list → base game fallback          |
| `NavMesh.CalculatePath` fails for a leg      | Direct line fallback for that leg                |
| `NavMesh.CalculatePath` returns partial path | Treated as failure, uses direct line             |
| Edge pull lands off-mesh                     | Keeps original position                          |
| Surface snap finds no NavMesh nearby         | Keeps the chord position                         |
| Surface snap would cross to another floor    | Keeps the chord position                         |
| Chord still sags at `MaxSubdivisionDepth`    | Stops subdividing, accepts the residual sag      |
| < 2 resampled positions                      | `Patch_SetupMovement` returns `true` → base game |

The system never crashes on bad geometry — it falls back to base game behavior at every level.

## Integration with Patch_SetupMovement

`Patch_SetupMovement` is a Harmony prefix on `ChainedPuzzleInstance.SetupMovement`. It fires for **all** movable scan types (not just type 100):

1. Checks `CP_BasicMovable.IsMoveConfigured` — skips non-movable scans
2. Calls `TravelPathGenerator.GenerateLoop(sourceArea, transform.position)`
3. Inserts current position as position 0
4. Converts to `Il2CppSystem.Collections.Generic.List<Vector3>`
5. Sets `movable.ScanPositions` via the `iChainedPuzzleMovable` interface
6. Writes `m_amountOfPositions` and `m_typeOfMovement = Circular` via IL2CPP field offsets
7. Returns `false` to skip the base game's radial position generation

The IL2CPP field writes are necessary because `m_amountOfPositions` and `m_typeOfMovement` are `[SerializeField] private` fields — IL2CPP doesn't expose property setters for them, so we resolve their memory offsets at runtime and write directly.

Writing `m_amountOfPositions` from the *final* `pathPositions.Count` matters more than it looks: `CP_BasicMovable.TryGetNextIndex` indexes off `AmountOfPositions`, not `ScanPositions.Count`. A mismatch truncates the path or throws index-out-of-range.

## Debug Overlay (DEBUG builds only)

`TravelPathDebugDraw` renders every generated path in-game so waypoint placement is visible rather than inferred. It uses the game's own `DebugDraw3D` manager, which is registered in `Global.m_ManagersAlwaysLoaded` and set up by `Global.SetupManagers` in retail builds — its prefabs, materials, layers and level-cleanup are already correct.

- **Cyan sphere** per waypoint; **magenta** at index 0 (the loop start)
- **Cone** per segment, tip at the later waypoint so it reads as a direction-of-travel arrow
- **Green** where the chord stays above the walkable surface, **red** where it still sags past `MaxChordSag` — red segments are exactly where the scan will clip through the floor

Paths are recorded by `Patch_SetupMovement` into `TravelScanRegistry.GeneratedPaths` during level build, then drawn from `LevelAPI.OnBuildDone`. Drawing is deliberately deferred rather than done inline in `SetupMovement`, which runs mid-build — `DebugDraw3D.OnLevelCleanup` clears every active shape, so drawing after the build sidesteps ordering concerns entirely.

`DebugDraw3D`'s pools are `GameObjectPoolType.LoopAndReuse` at a fixed size (200 spheres, 200 cones). In that mode `GameObjectPool.GetPooledObject` **ignores `CanCreateNew`** and just wraps `m_loopIndex`, silently handing back a shape that is still in use. `MaxDrawnWaypoints` (180) keeps us under that ceiling; longer paths are drawn with a stride and the stride is logged.

The whole file is inside `#if DEBUG`. Note that `AutogenRundown.csproj` defines `DEBUG` whenever `$(Debug) != 'false'`, which is unset by default — so **every** local build gets the overlay, including `-c Release`. To build without it: `dotnet build /p:Debug=false` (which also disables the PostBuild auto-deploy). CI already passes `--property:Debug=false`.
