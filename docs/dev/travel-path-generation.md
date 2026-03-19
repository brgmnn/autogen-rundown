# Travel Path Generation - Developer Guide

Deep dive on `TravelPathGenerator` and `Patch_SetupMovement`: how NavMesh walking circuits are built from AI graph nodes.

## Why NavMesh Instead of AI Graph Positions

The base game generates moving scan positions with `LG_NodeTools.TryGetPositionsOnRadiusDistancedFromEachother`, which picks random points within a radius. This produces a cluster of nearby positions — fine for small movement, but useless for zone-spanning walks.

We need a **walkable circuit**: a loop of positions that traces actual walking routes through the zone. NavMesh pathfinding guarantees every waypoint is reachable by walking, with no shortcuts through walls or jumps off ledges.

## Three-Leg Circuit Design

The path is a triangle: `source → dest1 → dest2 → source`.

```
        dest1
       /     \
      /       \
source ——————— dest2
```

Why a triangle instead of a random walk or two-point path:

- **Two points** (out-and-back) doubles back on itself — players walk the same corridor twice
- **Random walk** has no guaranteed loop closure — the last leg might be a long straight line
- **Triangle** covers three distinct routes through the zone with guaranteed closure back to source

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

The three legs are appended into a single `rawPath` list. The final corner of the last leg equals `sourcePos`, which is not duplicated in the resampled output (handled by the caller inserting sourcePos as position 0).

## Resampling

Raw NavMesh corners are unevenly spaced — tight turns produce clusters of corners, straight corridors produce two corners far apart. The base game's `CP_BasicMovable` interpolates between consecutive waypoints at constant speed, so uneven spacing causes the scan to crawl through turns and sprint through corridors.

`ResamplePath` fixes this by walking along the raw path at fixed `StepDistance` (3m) intervals:

```
Raw:    ●————————————————●——●——●————————————●
                                             (unevenly spaced)

Resampled: ●——●——●——●——●——●——●——●——●——●——●
                                             (3m intervals)
```

### Algorithm

1. Start at `corners[0]`, add it (edge-pulled) as the first point
2. Walk along each segment. When accumulated distance reaches `stepDistance`, emit a point
3. Carry leftover distance (`remaining`) into the next segment
4. **Do not** add the final corner — it's `sourcePos`, which the caller inserts as position 0

This keeps the loop tight: the last resampled point is within `stepDistance` of the source, and `Circular` movement wraps back to position 0.

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
