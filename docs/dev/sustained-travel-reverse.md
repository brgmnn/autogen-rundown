# Sustained Travel & Reverse Movement - Developer Guide

Covers runtime component injection for type 100 scans and the reverse movement mechanic. This is the most technically fragile part of the travel scan system.

## The Sustained Travel Concept

A sustained scan runs for a fixed duration (120s for type 100) and requires all players to stand in the scan area. The base game supports sustained scans, but their prefabs don't include `CP_BasicMovable` — they're stationary by design.

Sustained travel combines sustained behavior with walking movement. Since no prefab exists for this combination, we inject `CP_BasicMovable` at runtime onto the bioscan core's GameObject.

## Runtime Component Injection

Two Harmony patches work in sequence during `ChainedPuzzleInstance.Setup`:

### Step 1: Flag Detection (`Patch_ChainedPuzzleInstance_Setup`)

```
ChainedPuzzleInstance.Setup(data)
├── Prefix: scan data.ChainedPuzzle list for any PuzzleType in SustainedTravelTypes
│            if found → PendingSustainedTravel = true
│
├── [base game runs, creates CP_Bioscan_Core instances]
│   └── CP_Bioscan_Core.Setup fires for each component
│       └── Patch_CP_Bioscan_Core_Setup sees the flag
│
└── Postfix: PendingSustainedTravel = false
```

The flag pattern exists because `CP_Bioscan_Core.Setup` is a different method on a different class than `ChainedPuzzleInstance.Setup`. The prefix/postfix bracket the base game's loop over puzzle components, and the flag communicates between them.

### Ordering Guarantee

The base game calls `CP_Bioscan_Core.Setup` synchronously inside `ChainedPuzzleInstance.Setup`. The prefix sets the flag before any core is created, the postfix clears it after all cores are created. There is no async gap.

### Step 2: Component Injection (`Patch_CP_Bioscan_Core_Setup`)

When `PendingSustainedTravel` is true and the core has no existing `m_movingComp`:

1. `AddComponent<CP_BasicMovable>()` — injects the movement component
2. Write IL2CPP fields on the new component:
   - `m_amountOfPositions = 2` (placeholder — overwritten by `Patch_SetupMovement`)
   - `m_typeOfMovement = MovementType.Circular` (value 2)
   - `m_movementSpeed = 2.0f` (`SustainedTravelSpeed`)
   - `m_onlyMoveWhenScanning = true` (movement pauses when scan requirements unmet)
3. `__instance.m_movingComp = movable` — assign to the core's movement field
4. `SustainedTravelInstances.Add(__instance.Pointer)` — register for reverse tracking

The placeholder `m_amountOfPositions = 2` is required so that `IsMoveConfigured` returns true (it checks `m_amountOfPositions > 1`). The actual count is set later by `Patch_SetupMovement`.

## IL2CPP Field Access

### Why Not C# Properties

`CP_BasicMovable`'s movement fields (`m_amountOfPositions`, `m_typeOfMovement`, etc.) are `[SerializeField] private` in the game's C# source. When compiled to IL2CPP, these fields live in native memory. Il2CppInterop generates managed wrapper classes, but it doesn't always generate property setters for serialized private fields — only the native memory contains the actual values.

### How Offset Resolution Works

```csharp
// 1. Get the IL2CPP class pointer for CP_BasicMovable
var il2cppClass = Il2CppClassPointerStore<CP_BasicMovable>.NativeClassPtr;

// 2. Find the field by name
var fieldPtr = IL2CPP.il2cpp_class_get_field_from_name(il2cppClass, "m_amountOfPositions");

// 3. Get its byte offset from the object pointer
int offset = (int)IL2CPP.il2cpp_field_get_offset(fieldPtr);

// 4. Write directly via unsafe pointer arithmetic
*(int*)(movable.Pointer + offset) = value;
```

Offsets are resolved once and cached in static fields. Both `Patch_SustainedTravel` and `Patch_SetupMovement` resolve their own copies (they need different subsets of fields). `Patch_SustainedTravelReverse` resolves `m_lerpAmount`, `m_reset`, and `m_currentState` separately.

### Fragility Warning

**Field offsets can change between game versions.** If the GTFO developers add, remove, or reorder fields in `CP_BasicMovable`, the memory layout shifts. The offset resolution uses field names (`il2cpp_class_get_field_from_name`), so renamed fields will fail loudly (field not found). But reordered fields silently change offsets — the code still finds the field by name, but the offset now points to different data.

If scans start behaving erratically after a game update, check whether `CP_BasicMovable` fields have changed in the decompiled source.

## The Reverse Mechanic

### Design Intent

When a sustained travel scan is active, all players must stay inside the moving scan radius. If someone falls behind or splits off, the scan pauses (base game behavior). The reverse mechanic adds pressure: instead of just pausing, the scan **moves backward**, undoing progress. Players must regroup and catch up to resume forward movement.

### State Machine

```
Forward ──────────────── Paused (base game) ──────────── Reversing
   ▲                          │                              │
   │                          │ all players return            │ all players return
   │                          ▼                              │
   │                     Resume Forward ◄────────────────────┘
   │                          │                  m_reset = true
   └──────────────────────────┘                  (restart coroutine
                                                  from reversed position)
```

### How Paused Detection Works

`Patch_SustainedTravelReverse` hooks `CP_Bioscan_Core.OnSyncStateChange`:

1. Check `status == Scanning || status == Waiting` (scan is active)
2. Read `m_currentState.paused` from `CP_BasicMovable` via IL2CPP offset
   - `m_currentState` is a `pMovableStateSync` struct
   - `paused` is at offset +4 within the struct (after `float lerp` at +0)
3. If paused and `LerpAmount > 0`, set `_reversing[instancePtr] = true`
4. If not paused and was reversing, set `m_reset = true` and clear the flag

### Reverse Movement (Update_Postfix)

Runs every frame on the master when `_reversing[ptr] == true`:

```csharp
// Compute reverse delta
float delta = Clock.Delta * ReverseSpeed / segmentDistance;
lerpAmount -= delta;

// Recompute position
int idx = (int)lerpAmount;
float t = lerpAmount % 1f;
position = Lerp(positions[idx], positions[idx + 1], t);

// Write back
WriteLerpAmount(movable, lerpAmount);      // m_lerpAmount
WriteCurrentStateLerp(movable, lerpAmount); // m_currentState.lerp
```

### Why We Write Both m_lerpAmount and m_currentState.lerp

- `m_lerpAmount` is used by `CP_BasicMovable`'s movement coroutine to track the current position along the path. When forward movement resumes, the coroutine reads this value.
- `m_currentState.lerp` is used by `pMovableStateSync` for network replication. Clients receive this value to update their local position.

Missing either causes:
- Without `m_lerpAmount`: forward movement resumes from the pre-reverse position (visual snap)
- Without `m_currentState.lerp`: clients don't see the reverse movement (host/client desync)

### The m_reset Trick

When the team regroups and reverse stops, we set `m_reset = true`. This flag tells `CP_BasicMovable`'s movement coroutine to restart from the current `m_lerpAmount` position rather than continuing from where it was before the pause. Without it, the scan would snap back to its pre-reverse position when forward movement resumes.

### Master-Only Execution

Both `OnSyncStateChange_Postfix` and `Update_Postfix` check `SNet.IsMaster` first. Only the host runs reverse logic. If all 4 clients ran it, the scan would reverse 4x as fast (each applying `delta` independently). The host's writes to `m_currentState.lerp` are replicated to clients through the base game's `pMovableStateSync` system.

## Pitfalls

### Stale IntPtr Pointers

`SustainedTravelInstances` stores raw IL2CPP `IntPtr` pointers to `CP_Bioscan_Core` objects. These pointers are only valid for the current level. If `TravelScanRegistry.Clear()` doesn't run on level cleanup, the next level's `Update_Postfix` would dereference freed memory — a crash or silent corruption.

`Clear()` is registered via `LevelAPI.OnLevelCleanup` in `TravelScanRegistry.Setup()`. It clears both `SustainedTravelInstances` and the `_reversing` dictionary.

### Game Version Changes

IL2CPP field offsets are resolved by name but stored as byte offsets. If `CP_BasicMovable` gains or loses fields between game versions, all offsets after the changed field shift. The code logs resolved offsets at startup — compare these against expected values when debugging post-update issues.

Fields used across both injection and reverse patches:

| Field | Used By | Type |
|-------|---------|------|
| `m_amountOfPositions` | Injection, SetupMovement | int |
| `m_typeOfMovement` | Injection, SetupMovement | int (enum) |
| `m_movementSpeed` | Injection | float |
| `m_onlyMoveWhenScanning` | Injection | bool |
| `m_lerpAmount` | Reverse | float |
| `m_reset` | Reverse | bool |
| `m_currentState` | Reverse | pMovableStateSync struct |

### Paused-Detection Timing

The reverse patch only activates when `status` is `Scanning` or `Waiting`. If the scan is `Finished` or `TimedOut`, reverse does not apply — the scan is done. If `Disabled` or `SplineReveal`, movement hasn't started yet. This prevents reverse from running during non-gameplay states.

### pMovableStateSync Struct Layout

The struct layout matters for direct memory access:

```
m_currentState (pMovableStateSync):
  +0: float lerp     ← current position along path
  +4: bool  paused   ← movement paused by base game
```

If this struct layout changes in a future game version, both `ReadPaused` and `WriteCurrentStateLerp` will read/write wrong offsets. The field names resolved via `il2cpp_class_get_field_from_name("m_currentState")` point to the struct start — the internal layout is hardcoded as `+0` and `+4`.
