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

**dest2**: Try a **gate** first, and only fall back to distance ranking. `TryFindGateDestination` prefers the zone's own source gate — where the previous zone connects, and where enemies come in — if it is in this area; otherwise any gate in the area, preferring zone-crossing ones farthest from dest1. `OffsetFromGate` then moves the chosen position 3m toward the area centre so the waypoint does not sit in the doorway itself. This is what the log line `dest2 placed near zone entrance gate` means.

When no gate is found, dest2 falls back to the same triangle-spread heuristic as dest3.

**dest3**: Pick the node that maximizes `min(navDist to source, navDist to dest1)`. This is a triangle-spread heuristic — it avoids picking a node that's far from source but right next to dest1 (which would collapse two legs into one). Nodes within 1.0 unit of dest1 are skipped entirely.

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
NavMesh.CalculatePath(from, to, TravelScanRegistry.WalkableAreaMask, navPath)  // 1 = area 0 only
```

- Requires `PathComplete` status and ≥2 corners. `PathPartial` is treated as failure, not as a usable prefix
- Subsequent legs skip their first corner (it equals the previous leg's last corner)
- **There is no straight-line fallback.** A leg that cannot be pathed on walkable ground is dropped and the destination skipped, making the circuit smaller

The missing fallback is deliberate. A direct line between two nodes runs through whatever geometry is in the way, and once the scan is lerping along it there is nothing downstream that can recover — the tracing pass will refuse to follow it and say so, but it cannot invent a route that the pathfinder could not find. A smaller circuit is strictly better than one with a chord through a wall. Only failing to close the loop back to `sourcePos` aborts generation entirely, falling back to base game behaviour.

See [Area Mask](#area-mask) for why the mask is 1 rather than -1 — this is the query where it matters most.

The four legs are appended into a single `rawPath` list. The final corner of the last leg equals `sourcePos`, which is not duplicated in the traced output (handled by the caller inserting sourcePos as position 0).

## The Invariant

Everything below serves one rule:

> Consecutive waypoints — including the `Circular` wrap back to index 0 — must be joined by a straight line that stays on walkable ground.

`CP_BasicMovable.DoMoveScanner` lerps straight between consecutive `ScanPositions` with no smoothing, so any segment that violates this is one the scan visibly travels *through* geometry.

## Tracing the triangles

The navmesh is a triangle mesh. Following it is the whole design, and it is what makes the invariant true by construction rather than merely intended.

Raw NavMesh corners will not do on their own. Unity's funnel algorithm only emits a corner where the path turns **horizontally**, so a straight run up a staircase emits none at the crest: one corner low on the stairs, the next well out onto the upper floor. The chord between them cuts through the slope. Vanilla has exactly this bug — `CP_Holopath_Spline.GeneratePath` feeds `CalculatePath` corners straight into a spline with no per-triangle refinement, which is why the holopath ribbon clips stairs.

`NavSurface.TryTrace` marches the route across the triangles instead:

```
Corners:    ●———————————————————————————————————●
Triangles:  ╱│╲ ╱│╲ ╱│╲ ╱│╲ ╱│╲ ╱│╲ ╱│╲ ╱│╲ ╱│╲
Traced:     ●—●—●—●—●—●—●—●—●—●—●—●—●—●—●—●—●—●—●   a point at every edge crossing
Waypoints:  ●———————●———●—●———————●———————————●—●   thinned, folds kept
```

1. Locate the triangle the route enters
2. Find where the XZ ray leaves that triangle, and emit the crossing lifted onto the triangle's plane
3. Step a centimetre past the edge, re-locate, and repeat
4. Stop when the destination falls inside the current triangle

**Every emitted segment lies inside a single flat triangle.** A flat triangle cannot be passed through, so the line the scan lerps along is a line across a flat surface. That is the property that removed the detour walk, edge pulling, sag subdivision, walkability repair and the refinement loop in one go — every one of those existed to patch up damage that this cannot cause.

### What it looks like in play

Confirmed on `DogGeos_Tech_Junction`, ZONE_606 Area_A, seed `C3 Monthly 2026_08` — the same tile that produced every failure in this investigation. Every waypoint sits on the navmesh, and the path visibly **steps along the mesh triangulation**.

That stepped look is the algorithm working, not an artefact. Each waypoint is a triangle edge crossing, so on regularly triangulated floor the crossings come at regular intervals and the path reads as though it were following a grid — because it is. Every vertex is a real feature of the surface rather than a sample taken at an arbitrary interval, which is exactly why none of them can be off the floor. `Decimate` then thins flat runs and keeps folds, so waypoint density follows the geometry: sparse across open floor, tight over stairs.

For contrast, the last run on this tile before the rewrite reported **six** segments not walkable, including an 8.4m and a 7.3m chord, with four separate walks each saturating the shared 8-detour budget.

Loop closure is the other number that moved: 7.00m → 0.94m → **0.02m** across the three approaches. The trace ends where it was told to, rather than wherever a resampling step happened to stop.

### Baseline

From that run, as a reference point — a regression in extraction cost or trace density should be obvious by comparison rather than needing to be re-derived:

| | |
| --- | --- |
| Walkable triangles / total | 189,929 of 189,931 |
| Vertices | 388,899 |
| Pathfinder corners | 40 |
| Traced points | 463 (~11.9 per corner pair, 39 pairs) |
| Waypoints after thinning | 156 (3.0:1), 157 positions set |
| Loop closing distance | 0.02m |

Three things worth drawing out of those numbers.

**The vertex count is evidence for the [no-adjacency decision](#why-the-trace-re-locates-instead-of-using-adjacency).** 388,899 vertices across 189,929 triangles is ≈2.05 vertices per triangle. For comparison, a large welded triangulated grid tends to ≈0.5 (an *N*×*N* quad grid is 2*N*² triangles over ≈*N*² vertices), and a completely unwelded soup is exactly 3.0.

2.05 is very close to what you get when each navmesh *polygon* contributes its own vertices and shares none with its neighbours: a 4-vertex convex polygon triangulates into 2 triangles from 4 vertices, giving exactly 2.0. So vertices are essentially per-polygon here, not shared across polygons.

That matters because an adjacency table built from shared vertex indices — the obvious way to cross an edge — would then find almost no shared edges at all. It would not merely have holes at the 64m bake-tile seams; it would be close to useless. The argument for re-locating by position and height was made from first principles, and the measurement is stronger than the argument was.

**Only 2 triangles of 189,931 were dropped**, so the area-0 filter is very nearly a no-op on this geometry. It stays because it is cheap and it is what keeps Jump and Ladder surface out of the mesh on a level that has any — but do not mistake it for load-bearing here.

**The mesh is large and it is held for the whole level.** Roughly 4.7MB of vertices plus 2.3MB of indices, plus the spatial index on top: order 15–20MB retained from first use until `OnLevelCleanup`. Acceptable for a level build, but real. If it ever matters, filter triangles to the path's bounding box before indexing (see [Extracting the triangulation](#extracting-the-triangulation)).

Extraction time is **not currently logged** — only counts are. If extraction is ever suspected of being slow, adding a stopwatch to `NavSurface.Build` is the first step; do not infer a figure from these numbers, because none was measured.

One more: 157 positions sits under the debug overlay's `MaxDrawnWaypoints` (180) but not far under. A larger zone will start drawing every *n*th waypoint and will log that it has done so. That is expected — see [Debug Overlay](#debug-overlay-debug-builds-only).

### Locating a point

`NavSurface.TryLocate` takes a point and a **preferred height**, gathers every triangle whose XZ projection contains that point, and returns the one whose surface height is nearest.

This is what replaced `NavMesh.SamplePosition`, and the difference is not incremental. `SamplePosition` can only return the *nearest* surface within a radius, which near a ledge is the floor below; the only way to steer it is to move the probe and hope. Enumerating the candidates makes the floor choice explicit, so a point cannot silently resolve onto the wrong one.

Points with nothing directly underneath — a `CalculatePath` corner sitting exactly on a mesh boundary, or a crossing landing in the sliver where two bake tiles meet — fall back to the nearest triangle within `LocateRadius` in the surrounding cells.

### Why the trace re-locates instead of using adjacency

The obvious way to cross a triangle edge is an adjacency table built from shared vertex indices. The trace deliberately does not have one. It steps `EdgeNudge` (1cm) past the edge and asks which triangle is there, preferring the height it just left.

Two reasons:

- **Bake tiles.** The bake is one `NavMeshData` per Dimension over a 2560m cube in 64m tiles (`LG_BuildUnityGraphJob.cs:49-60`). Unity is under no obligation to weld vertices across a tile seam, and an adjacency table built from indices would have a hole at every one. Re-locating treats a seam as just another crossing.
- **Floors are decided explicitly.** With adjacency the floor is whatever the topology says; here every crossing re-states which height it expects. That is where the ledge check lives.

### The ledge check

A crossing is rejected when the height changes by more than `MaxTraceStep` (0.5m):

```csharp
if (Mathf.Abs(next.y - exitY) > TravelScanRegistry.MaxTraceStep)
    return false;
```

0.5m is `agentClimb` from the bake — the most two adjacent walkable polygons can legitimately differ by at their shared edge, so real staircases still trace. Anything larger is a different floor.

This one line is the fix for segment 21→22 in ZONE_606: `(-19.9, 0.0, 124.1)` → `(-24.0, -6.0, 124.5)`. Both endpoints are on the mesh, the horizontal distance is only 4m, and there is perfectly good floor immediately past the edge — six metres down. Every check that works by asking *"is there surface near here"* answers yes, which is why no amount of tuning `SamplePosition` was ever going to catch it. `SurfaceTrace_Tests.Test_Trace_StopsAtACliffWithFloorDirectlyBelowIt` pins it, and the fixture builds the two floors as separate abutting grids on purpose — one grid with a step in its height function produces a 1m ramp instead of a cliff, which the trace should and does follow.

### Starting the trace

The march starts a nudge *along* the route rather than at the start point:

```csharp
var cursor = new Vector2(start.x, start.z) + direction * TravelScanRegistry.EdgeNudge;
```

Every pathfinder corner sits on a triangle boundary, and a point on a shared edge belongs to the triangles on both sides of it. Picking the one *behind* the route leaves a ray with no forward exit at all, and the trace gives up before taking a step. This showed up immediately in testing as every axis-aligned trace failing with zero points while off-grid traces worked perfectly.

## Thinning the trace

The trace is exact but dense. `Decimate` reduces it to waypoints under two bounds:

| Bound | Constant | Purpose |
| --- | --- | --- |
| Deviation | `MaxTraceDeviation` (0.05m) | A point may be dropped only if the path without it still follows the path with it. This is what keeps every fold — each stair nose, each crest. |
| Spacing | `StepDistance` (2m) | A long flat run is split so the scan does not cross it in one lerp. |

Deviation is measured against the **traced polyline**, not against a re-sampled guess at the surface. That distinction is the reason this tolerance means what it says where the old `MaxChordSag` did not.

Splits are taken at even arclength along the trace and interpolated between traced points. Interpolating there is exact rather than approximate: the segment between two consecutive traced points lies inside one triangle, so the surface across it is flat. Splitting on arclength rather than on the chord is what bounds the spacing — straight-line distance is never more than distance along the polyline.

`MinSegmentLength` (0.05m) survives only as a division guard: `DoMoveScanner` divides by segment length and `Patch_SustainedTravelReverse` bails out below 0.001m. It is applied as a pre-pass so everything after it can append unconditionally and the spacing bound comes out exact. It is deliberately far below the spacing of real geometry — at the bake's 8.3cm voxel size, consecutive folds on a fine staircase can be centimetres apart, and thinning those away is what put chords through the floor in the first place.

## When a stretch cannot be traced

`TryTrace` failing between two corners of a *complete* path means the funnel and the triangles disagree, which should not happen. `TraceSurface` handles it in one step: ask `CalculatePath` for the route between just those two points and trace that.

This is the information the old detour walk asked for and then threw away. Its budget of 8 re-routes was shared across the **whole walk** — 43 corners across 4 legs — and the check short-circuited, so once spent, no further re-routing was attempted at all. Every subsequent blocked step fell into a branch that teleported the cursor to the corner without emitting it, leaving one unlogged chord from the last good waypoint to a point 2m *past* the corner. Four `Surface walk took 8 detour(s)` lines in one log meant four separate walks had each saturated the cap.

The diagnostic that settled this is worth keeping in mind: every failing segment reported `CalculatePath PathComplete` with 3 to 11 corners. The pathfinder always knew the way round.

If the reroute also fails, the corner is emitted and the circuit continues — the segment is wrong, but it is **logged** and the rest of the path is usable. A partial trace is discarded rather than kept, because it ends wherever the surface ran out and puts the gap somewhere less obvious.

## Verifying the output

`LogOffSurfaceSegments` re-checks the finished waypoint list against the surface it was traced from, so a failure appears in the log rather than only in-game:

- every waypoint must locate on the surface at its own height
- every segment, sampled across its length, must not pass below the floor by more than `MaxTraceDeviation`

Sampling across the segment rather than only at its midpoint matters: a segment can straddle a fold with both ends *and* the middle on the floor while the quarter points are underneath it.

## Area Mask

Every NavMesh query uses `WalkableAreaMask` (1 — area 0 only), and the triangulation is filtered to `areas[t] == 0` for the same reason.

An `areaMask` of -1 admits Jump (area 2) and Ladder (area 3) off-mesh links. `LG_Ladder.BuildOffmeshLink` spawns ten per ladder, and a path routed through one yields a vertical pair of corners with nothing walkable in between — the scan drops straight through the floor. The game does the same thing for the same reason: `PlayerBotActionTravel` passes 17 (Walkable | PlayerBot).

Note `NavMesh.AllAreas` does **not** exist in GTFO's IL2CPP metadata — it is a `const int` that IL2CPP folded away and the unstripper did not restore. Hard-code -1 where you want every area.

## Graceful Degradation

| Condition                                    | Behavior                                         |
| -------------------------------------------- | ------------------------------------------------ |
| `sourceArea.m_courseNode` not valid          | Returns empty list → base game fallback          |
| < 2 candidate nodes (small/degenerate zone)  | Returns empty list → base game fallback          |
| `NavMesh.CalculatePath` fails for a leg      | Destination skipped, circuit gets smaller        |
| `NavMesh.CalculatePath` returns partial path | Treated as failure, destination skipped          |
| Closing leg back to source cannot be pathed  | Returns empty list → base game fallback          |
| `CalculateTriangulation` unavailable / empty | Warns, returns the raw pathfinder corners        |
| Trace produces < 2 points                    | Warns, returns the raw pathfinder corners        |
| Path start is not on the surface             | Warns, returns an empty trace                    |
| A stretch cannot be traced, reroute fails    | Corner emitted, warned, drawn red                |
| Segment still below the surface at the end   | Kept, logged as a warning, drawn red             |
| < 2 resampled positions                      | `Patch_SetupMovement` returns `true` → base game |

The system never crashes on bad geometry — it falls back to base game behavior at every level. `NavSurface.Build` is additionally wrapped in a try/catch, because `NavMeshTriangulation` is a non-blittable struct that Il2CppInterop hands over as a reference wrapper around a boxed IL2CPP value type (see [Extracting the triangulation](#extracting-the-triangulation)).

## Extracting the triangulation

`NavMesh.CalculateTriangulation()` is genuinely in GTFO's IL2CPP metadata — the interop DLL carries `NativeMethodInfoPtr_CalculateTriangulation_Public_Static_NavMeshTriangulation_0` rather than an ICall delegate shim, and the game calls it itself every level in `MapDetails.Extract` (`gtfo-decompile/Modules-ASM/MapDetails.cs:131-153`) to build the in-game map mesh.

Two things to know before touching it:

- **It is not a blittable struct.** `NavMeshTriangulation` holds three managed array references, so Il2CppInterop emits it as an `Il2CppSystem.ValueType`-derived reference wrapper with `Il2CppStructArray<T>` members and no instance fields. There is no `fixed`/`Marshal` fast path; go through the properties, and copy straight out with `.ToArray()` so nothing holds an interop object across the rest of the build.
- **`NavMeshQuery` is not an alternative.** `MapLocation`, `Raycast` and `IsValidPolygon` are real (the AI-graph bake uses them at `LG_BuildNodeVolumeJobData.cs:179,252`), but every polygon-geometry accessor — `GetPortalPoints`, `GetEdgesAndNeighbors`, `GetPolygonType` — is a shim. `CalculateTriangulation` is the only geometry source there is.

The surface is extracted once per level, lazily on first use, and cleared from `TravelScanRegistry.Clear()` on `OnLevelCleanup`. Lazily rather than from a build hook because generation runs during `ChainedPuzzleInstance.SetupMovement`, by which point the navmesh is demonstrably live — `CalculatePath` already succeeds there.

## Why NavMesh.Raycast is not enough — and why it is now unused

Worth recording, because it cost several rounds to work out and the same trap is waiting in any other navmesh code.

`NavMesh.Raycast(a, b, out hit, mask)` walks the **2D projection** of the navmesh polygons. Two positions stacked at nearly the same XZ on different floors look like a zero-length walk that never leaves the mesh, and it reports them as **connected**.

The previous design paired it with a slope band (`|Δy| <= horizontal * tan(55°) + 0.5`) as a stand-in for the polygon information it lacks — the same two-test structure the game uses in `LG_BuildNodeVolumeJobData.NodeLinksJob`, which gates every AI graph link on both a ±1.5m Y window and a clear raycast.

Tracing needs neither. Knowing which triangle you are on answers both questions exactly, so `IsWalkable`, `IsSlopeWalkable`, `MaxSagFor` and the whole `ISurfaceProbe` seam are gone. `NavMesh.Raycast` no longer appears in the travel scan code at all.


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
- Segments are coloured by the invariant:

| Colour | Meaning |
| --- | --- |
| **Red** segment | The chord passes below the floor by more than `MaxTraceDeviation`, sampled across its length. The scan clips through geometry here. |
| **Orange** segment | An endpoint is not on the walkable surface at all, so nothing about the segment can be judged. |
| **Green** segment | Good. |
| **Orange** sphere | This waypoint is not on the walkable surface. |
| **Magenta** sphere | Waypoint 0, the loop start. |
| **Cyan** sphere | Normal waypoint. |

Both failure colours now mean a real bug rather than a tolerance being grazed. Tracing puts every waypoint on a triangle and every segment inside one, so anything else means the surface and the path have genuinely diverged. The old yellow state — "walkable, but sagging more than this slope allows" — is gone with the adaptive sag tolerance it reported on.

Orange is worth keeping for the lesson behind it. The old `Snap` returned its input unchanged when it could not find the surface, so an off-mesh waypoint was indistinguishable from a perfectly placed one: the sag test measured zero and painted it **green**. A whole run of waypoints under the floor looked flawless. Anything that cannot be vouched for renders as unknown rather than good.

Segments are sampled at quarter points rather than only at the midpoint, for the same reason `LogOffSurfaceSegments` does: a segment can straddle a fold with both ends and the middle on the floor while the quarter points are underneath it.

Paths are recorded by `Patch_SetupMovement` into `TravelScanRegistry.GeneratedPaths` during level build, then drawn from `LevelAPI.OnBuildDone`. Drawing is deliberately deferred rather than done inline in `SetupMovement`, which runs mid-build — `DebugDraw3D.OnLevelCleanup` clears every active shape, so drawing after the build sidesteps ordering concerns entirely.

`DebugDraw3D`'s pools are `GameObjectPoolType.LoopAndReuse` at a fixed size (200 spheres, 200 cones). In that mode `GameObjectPool.GetPooledObject` **ignores `CanCreateNew`** and just wraps `m_loopIndex`, silently handing back a shape that is still in use. `MaxDrawnWaypoints` (180) keeps us under that ceiling; longer paths are drawn with a stride and the stride is logged.

The whole file is inside `#if DEBUG`. Note that `AutogenRundown.csproj` defines `DEBUG` whenever `$(Debug) != 'false'`, which is unset by default — so **every** local build gets the overlay, including `-c Release`. To build without it: `dotnet build /p:Debug=false` (which also disables the PostBuild auto-deploy). CI already passes `--property:Debug=false`.
