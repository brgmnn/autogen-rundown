# Travel Scan System - Developer Guide

Travel scans replace the base game's stationary moving-scan behavior with walking circuits that force players to move through entire zones. Instead of shuffling between positions in a tight radius, the scan traces a NavMesh loop through the zone.

## The Problem

Base game moving scans use `LG_NodeTools.TryGetPositionsOnRadiusDistancedFromEachother` to scatter waypoints in a radius around the spawn point. Players stand in roughly the same spot. Travel scans create a looping circuit through the zone using NavMesh pathfinding, turning a stationary encounter into a walking one.

## Architecture

```
Level Build Phase
=================

ChainedPuzzleInstance.Setup(data)
│
├── [Patch_ChainedPuzzleInstance_Setup]     ← Prefix: scans puzzle list for type 100
│   │                                         Sets PendingSustainedTravel = true
│   │
│   ├── CP_Bioscan_Core.Setup(...)
│   │   │
│   │   └── [Patch_CP_Bioscan_Core_Setup]  ← Prefix: if PendingSustainedTravel,
│   │       │                                 injects CP_BasicMovable component
│   │       │                                 via AddComponent + IL2CPP field writes
│   │       │
│   │       └── Sets m_movingComp, registers IntPtr in SustainedTravelInstances
│   │
│   └── Postfix: clears PendingSustainedTravel
│
├── ChainedPuzzleInstance.SetupMovement(gameObject, sourceArea)
│   │
│   └── [Patch_SetupMovement]              ← Prefix: replaces position generation
│       │                                     for ALL movable scan types
│       ├── TravelPathGenerator.GenerateLoop()
│       │   ├── GatherCandidates()         ← AI graph nodes with ≥4 links
│       │   ├── PickDestinations()         ← Euclidean pre-filter → NavMesh ranking
│       │   ├── AppendNavMeshLeg() ×3      ← source→dest1→dest2→source
│       │   └── ResamplePath()             ← Fixed 3m steps, edge-pulled
│       │
│       ├── Sets ScanPositions via interface
│       ├── Writes m_amountOfPositions, m_typeOfMovement via IL2CPP
│       └── return false (skip base game)

Gameplay Phase
==============

CP_Bioscan_Core.OnSyncStateChange(status)
│
└── [Patch_SustainedTravelReverse]         ← Postfix: detects paused state
    │                                         (not all players in scan)
    └── Sets _reversing[ptr] = true

CP_Bioscan_Core.Update()
│
└── [Patch_SustainedTravelReverse]         ← Postfix: drives scan backwards
    │                                         at 1.0 m/s when reversing
    └── Writes m_lerpAmount + m_currentState.lerp

Level Cleanup
=============

LevelAPI.OnLevelCleanup → TravelScanRegistry.Clear()
├── SustainedTravelInstances.Clear()       ← Prevents stale IntPtr access
├── PendingSustainedTravel = false
└── Patch_SustainedTravelReverse.Clear()   ← Clears _reversing dictionary
```

## Puzzle Type Taxonomy

### Naturally Movable (prefab includes CP_BasicMovable)

| ID  | Name                                         | Description                   |
| --- | -------------------------------------------- | ----------------------------- |
| 22  | `SecurityScan_Big_RequireAll_Movable`        | Big scan, require all, moving |
| 31  | `SecurityScan_Big_Movable_FadeIn`            | Big movable, fade-in visual   |
| 38  | `SecurityScan_Big_Movable_FadeIn_RequireAll` | Fade-in, require all          |
| 42  | `SecurityScan_Big_Movable`                   | Big movable scan              |
| 43  | `SecurityScan_Movable_Small`                 | Small movable scan            |
| 52  | `SecurityScan_Big_Movable_Slow`              | Big movable, slow speed       |
| 60  | `SecurityScan_Big_Movable_FadeIn_Slow`       | Fade-in, slow speed           |

These types already have `CP_BasicMovable` on their prefab with `IsMoveConfigured = true`. Patch_SetupMovement intercepts all of them to replace radial position generation with NavMesh circuits.

### Runtime-Injected (type 100)

| ID  | Name              | Duration | Speed   |
| --- | ----------------- | -------- | ------- |
| 100 | `SustainedTravel` | 120s     | 2.0 m/s |

Type 100 is special: the base game sustained scan prefab has no `CP_BasicMovable` component. The system injects one at runtime via `AddComponent<CP_BasicMovable>()` and writes its serialized fields through IL2CPP pointer arithmetic. This is the only type that also gets the reverse mechanic.

## Lifecycle

```
Init        Plugin.cs → GameDataAPI.OnGameDataInitialized += TravelScanRegistry.Setup
            TravelScanRegistry.Setup registers LevelAPI.OnLevelCleanup += Clear

Setup       ChainedPuzzleInstance.Setup fires during level build
            → Patch detects type 100, sets PendingSustainedTravel flag
            → CP_Bioscan_Core.Setup fires, patch injects CP_BasicMovable
            → Pointer stored in SustainedTravelInstances

Movement    ChainedPuzzleInstance.SetupMovement fires for all movable types
            → Patch_SetupMovement generates NavMesh loop, sets positions
            → Returns false to skip base game radial generation

Gameplay    Scan activates, CP_BasicMovable drives forward movement
            → Reverse patch monitors state changes and Update() calls
            → If paused (players left scan), drives scan backward

Cleanup     LevelAPI.OnLevelCleanup → TravelScanRegistry.Clear()
            → Clears SustainedTravelInstances and _reversing dictionary
            → Critical: prevents stale IntPtr dereference on next level
```

## Networking Model

Path generation is **deterministic across clients**. All clients call `SetupMovement` during level build with the same AI graph, NavMesh, and spawn positions, producing identical waypoint lists. No path data is sent over the network.

Forward movement is driven by `CP_BasicMovable`'s built-in `pMovableStateSync` replicator, which syncs `lerp` (position along path) and `paused` (whether requirements are met).

Reverse movement runs **master-only** (`SNet.IsMaster`). The master writes `m_lerpAmount` and `m_currentState.lerp` directly, and the existing `pMovableStateSync` replication propagates the reversed position to clients. If all clients ran reverse logic, the scan would reverse 4x as fast.

## File Map

| File                              | Purpose                                                          |
| --------------------------------- | ---------------------------------------------------------------- |
| `TravelScanRegistry.cs`           | Constants, type sets, instance tracking, lifecycle hooks         |
| `Patch_SustainedTravel.cs`        | Runtime CP_BasicMovable injection for type 100                   |
| `Patch_SetupMovement.cs`          | Replaces radial positions with NavMesh loops (all movable types) |
| `Patch_SustainedTravelReverse.cs` | Reverse movement when players leave scan (type 100 only)         |
| `TravelPathGenerator.cs`          | NavMesh pathfinding, destination selection, resampling           |

## Key Constants

| Constant                      | Value   | Location              |
| ----------------------------- | ------- | --------------------- |
| `SustainedTravelSpeed`        | 2.0 m/s | `TravelScanRegistry`  |
| `SustainedTravelReverseSpeed` | 1.0 m/s | `TravelScanRegistry`  |
| `StepDistance`                | 3.0 m   | `TravelScanRegistry`  |
| `EdgeDistance`                | 2.0 m   | `TravelScanRegistry`  |
| `CandidatePoolSize`           | 20      | `TravelPathGenerator` |
