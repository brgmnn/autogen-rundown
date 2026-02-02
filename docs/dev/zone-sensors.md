# Zone Sensors - Developer Guide

This guide documents the Zone Sensor system for placing custom security sensors that detect players and trigger events.

## Overview

Zone sensors are security sensors placed automatically within zones using the game's navigation mesh. Unlike vanilla sensors (placed via level datablocks), zone sensors are:

- Defined in JSON files, separate from level generation
- More configurable (custom colors, text, movement, encrypted text cycling)
- Triggered per-group or individually
- Controllable via events (enable/disable/reset at runtime)
- Density-driven count, auto-calculated from zone area

## Architecture

```
┌─────────────────────────────────────────────────────────────────┐
│                         JSON Files                              │
│  BepInEx/GameData/{revision}/Custom/AutogenRundown/ZoneSensors/ │
└───────────────────────────┬─────────────────────────────────────┘
                            │ loaded by
                            ▼
┌─────────────────────────────────────────────────────────────────┐
│                      LevelZoneSensors                           │
│  Deserializes JSON, groups definitions by MainLevelLayout ID    │
│  File: DataBlocks/Custom/AutogenRundown/LevelZoneSensors.cs     │
└───────────────────────────┬─────────────────────────────────────┘
                            │ stored in
                            ▼
┌─────────────────────────────────────────────────────────────────┐
│                     ZoneSensorManager                           │
│  Singleton that spawns sensors on level build                   │
│  Handles trigger events, toggles, cleanup                       │
│  File: Patches/ZoneSensors/ZoneSensorManager.cs                 │
└───────────────────────────┬─────────────────────────────────────┘
                            │ creates
                            ▼
        ┌───────────────────┴───────────────────┐
        │                                       │
┌───────┴───────┐                     ┌─────────┴─────────┐
│ ZoneSensorGroup │                   │ GameObject (sensor)│
│ Runtime state   │                   │ Visual + Collider │
│ Network sync    │                   │ Optional mover    │
└─────────────────┘                   └───────────────────┘
```

### Key Components

| Component | File | Purpose |
| --------- | ---- | ------- |
| `ZoneSensorDefinition` | `DataBlocks/Custom/ZoneSensors/ZoneSensorDefinition.cs` | Zone-level configuration (extends Definition) |
| `ZoneSensorGroupDefinition` | `DataBlocks/Custom/ZoneSensors/ZoneSensorGroupDefinition.cs` | Group settings (count, radius, color, etc.) |
| `SensorDensity` | `DataBlocks/Custom/ZoneSensors/SensorDensity.cs` | Density enum for runtime count calculation |
| `LevelZoneSensors` | `DataBlocks/Custom/AutogenRundown/LevelZoneSensors.cs` | Top-level JSON structure, file I/O |
| `ZoneSensorManager` | `Patches/ZoneSensors/ZoneSensorManager.cs` | Spawning and event handling singleton |
| `ZoneSensorGroup` | `Patches/ZoneSensors/ZoneSensorGroup.cs` | Runtime state, network sync via StateReplicator |
| `ZoneSensorCollider` | `Patches/ZoneSensors/ZoneSensorCollider.cs` | Player detection MonoBehaviour |
| `ZoneSensorMover` | `Patches/ZoneSensors/ZoneSensorMover.cs` | Movement along NavMesh paths |
| `ZoneSensorGroupState` | `Patches/ZoneSensors/ZoneSensorGroupState.cs` | Network replication state struct |
| `ZoneSensorPositionState` | `Patches/ZoneSensors/ZoneSensorPositionState.cs` | Network state for spawn positions |
| `ZoneSensorWaypointState` | `Patches/ZoneSensors/ZoneSensorWaypointState.cs` | Network state for NavMesh waypoints |
| `ZoneSensorMovementState` | `Patches/ZoneSensors/ZoneSensorMovementState.cs` | Network state for movement progress |
| `ZoneSensorTextAnimator` | `Patches/ZoneSensors/ZoneSensorTextAnimator.cs` | Encrypted text animation |
| `GtfoTextMeshPro` | `Patches/ZoneSensors/GtfoTextMeshPro.cs` | Clean TMPro instantiation utility |
| `Patch_ZoneSensorToggle` | `Patches/ZoneSensors/Patch_ZoneSensorToggle.cs` | Harmony patch for event types 400-404 |

---

## Configuration

### JSON File Location

```
BepInEx/GameData/{revision}/Custom/AutogenRundown/ZoneSensors/
```

Files are named `{MainLevelLayout}_{Name}.json` and loaded automatically at game data initialization.

### Full JSON Schema

```json
{
  "Name": "Level_Name",
  "MainLevelLayout": 123,
  "Definitions": [
    {
      "DimensionIndex": "Reality",
      "LayerType": "MainLayer",
      "LocalIndex": 5,
      "StartEnabled": true,
      "SensorGroups": [
        {
          "AreaIndex": -1,
          "Count": 3,
          "Density": "None",
          "Radius": 2.3,
          "Color": {
            "Red": 0.93,
            "Green": 0.10,
            "Blue": 0.0,
            "Alpha": 0.26
          },
          "Text": null,
          "TextColor": {
            "Red": 0.88,
            "Green": 0.90,
            "Blue": 0.89,
            "Alpha": 0.70
          },
          "EncryptedText": false,
          "EncryptedTextColor": {
            "Red": 0.85,
            "Green": 0.31,
            "Blue": 0.10,
            "Alpha": 0.80
          },
          "HideText": false,
          "TriggerEach": false,
          "Moving": 2,
          "Speed": 1.5,
          "EdgeDistance": 0.3,
          "Height": 0.8
        }
      ],
      "EventsOnTrigger": [
        {
          "Type": 9,
          "Delay": 0.5,
          "EnemyWaveData": {
            "SurvivalWaveSettings": 50,
            "SurvivalWavePopulation": 1
          }
        }
      ]
    }
  ]
}
```

### LevelZoneSensors Properties

| Property | Type | Description |
| -------- | ---- | ----------- |
| `Name` | string | Level name for file naming |
| `MainLevelLayout` | uint | Level layout ID to attach sensors to |
| `Definitions` | List | Zone sensor definitions |

### ZoneSensorDefinition Properties

Inherits from `Definition` base class:

| Property | Type | Default | Description |
| -------- | ---- | ------- | ----------- |
| `DimensionIndex` | string | `"Reality"` | Dimension: `"Reality"`, `"Dimension_1"`, `"Dimension_2"` |
| `LayerType` | string | `"MainLayer"` | Layer: `"MainLayer"`, `"SecondaryLayer"`, `"ThirdLayer"` |
| `LocalIndex` | int | `0` | Zone number within the layer |
| `Id` | int | auto | Unique ID for event targeting. Auto-assigned if not set in JSON |
| `StartEnabled` | bool | `true` | Whether sensors start enabled when spawned. Set to `false` to spawn disabled |
| `SensorGroups` | List | `[]` | Groups of sensors to place |
| `EventsOnTrigger` | List | `[]` | Events executed when any sensor triggers |

### ZoneSensorGroupDefinition Properties

| Property | Type | Default | Description |
| -------- | ---- | ------- | ----------- |
| `AreaIndex` | int | `-1` | Area within zone (`-1` = random areas) |
| `Count` | int | `1` | Number of sensors in this group. Ignored when `Density` is set |
| `Density` | SensorDensity | `None` | Runtime count calculation from zone area. Overrides `Count` when not `None` |
| `Radius` | double | `2.3` | Detection radius of each sensor |
| `Color` | Color | Red (R8D1 style) | Color of the sensor visual ring |
| `Text` | string? | `null` | Text displayed on sensor. `null` = random corrupted text per sensor |
| `TextColor` | Color | Light gray | Color of the sensor text |
| `EncryptedText` | bool | `false` | Enables hex cycling animation on sensor text |
| `EncryptedTextColor` | Color | Orange | Color used during encrypted text phases |
| `HideText` | bool | `false` | Suppresses text display entirely. Takes precedence over `EncryptedText` |
| `TriggerEach` | bool | `false` | If true, each sensor triggers independently |
| `Moving` | int | `1` | Number of patrol positions. `1` = stationary, `2+` = moving between N positions |
| `Speed` | double | `1.5` | Movement speed (units/second) when `Moving > 1` |
| `EdgeDistance` | double | `0.3` | Min distance from NavMesh edges for waypoints |
| `Height` | double | `0.8` | Height multiplier for the sensor visual. Combined with `Radius` for vertical offset |

### Color Format

```json
{
  "Red": 0.0-1.0,
  "Green": 0.0-1.0,
  "Blue": 0.0-1.0,
  "Alpha": 0.0-1.0
}
```

**Preset colors** (defined in `DataBlocks/Color.cs`):

| Name | R | G | B | A | Use |
|------|---|---|---|---|-----|
| `ZoneSensor_RedSensor` | 0.934 | 0.106 | 0.0 | 0.263 | Default sensor ring |
| `ZoneSensor_InfectionGreenSensor` | 0.435 | 1.0 | 0.435 | 0.263 | Infection-themed ring |
| `ZoneSensor_EncryptedText` | 0.85 | 0.31 | 0.10 | 0.8 | Default encrypted text |

---

## Density System

When `Density` is set on a `ZoneSensorGroupDefinition`, the `Count` property is ignored. Instead, the sensor count is computed at runtime from the zone's walkable area.

### SensorDensity Enum

| Value | Name | Rate (per 100 coverage per unit radius) |
|-------|------|-----------------------------------------|
| 0 | `None` | Use explicit `Count` |
| 1 | `Low` | 1.5 |
| 2 | `Medium` | 3.0 |
| 3 | `High` | 4.5 |
| 4 | `VeryHigh` | 6.0 |

### Formula

From `ZoneSensorManager.CalculateSensorCountFromDensity()`:

```
rawCount = totalCoverage / 100 * rate / radius
count = Clamp(rawCount, 1, 128)
```

Where:
- `totalCoverage` = sum of `VoxelCoverage(0.9)` across all areas in the zone (derived from AI graph node count)
- `rate` = density rate from the table above
- `radius` = sensor's `Radius` property (larger sensors cover more area, so fewer are needed)

### Scaling Behavior

- Larger radius sensors produce fewer sensors at the same density (inversely proportional)
- Small zones (low coverage) always get at least 1 sensor
- Maximum 128 sensors per group (hard limit from bitmask and batch system)

---

## Tier-Based Generation

The `AddSecuritySensors()` method in `LevelLayout.SecuritySensors.cs` generates sensor configurations scaled by level tier. All parameters are weighted random selections.

### Density & Radius by Tier

| Tier | Density Options (weight) |
|------|--------------------------|
| A | Low/2.3 (1.0), Medium/1.2 (0.4) |
| B | Low/2.3 (1.0), Medium/1.2 (0.6), Medium/2.3 (0.1) |
| C | Low/2.3 (1.0), Medium/1.2 (1.0), Medium/2.3 (0.3), High/1.2 (0.3) |
| D | Low/2.3 (0.5), Medium/1.2 (1.0), Medium/2.3 (1.0), High/1.2 (0.6), VeryHigh/1.2 (0.1) |
| E | Medium/2.3 (1.0), High/1.2 (1.0), Medium/2.3 (0.7), VeryHigh/1.2 (0.5) |

### Moving Chance by Density & Tier

| Density | A | B | C | D | E |
|---------|---|---|---|---|---|
| Low | 40% | 45% | 52% | 60% | 66% |
| Medium | 33% | 33% | 50% | 50% | 50% |
| High | 5% | 5% | 21% | 33% | 45% |
| VeryHigh | 0% | 0% | 0% | 8% | 17% |

When moving: `Moving` = random 2-3, `Speed` = random 0.6-0.85.

### TriggerEach Chance by Density & Tier

| Density | A | B | C | D | E |
|---------|---|---|---|---|---|
| Low | 33% | 33% | 33% | 33% | 33% |
| Medium | 65% | 65% | 50% | 50% | 50% |
| High | 90% | 90% | 82% | 75% | 60% |
| VeryHigh | 100% | 100% | 100% | 95% | 90% |

### Wave Selection by Tier

| Tier | Wave Options (weight) |
|------|----------------------|
| A | Sensor_6pts (1.0) |
| B | Sensor_6pts (0.4), Sensor_8pts (1.0), Sensor_Shooters_6pts (0.4) |
| C | Sensor_Shooters_6pts (0.3), Sensor_8pts (0.4), Sensor_12pts (1.0) |
| D | Sensor_12pts (0.3), Sensor_Shooters_12pts (0.4), Sensor_16pts (1.0), SingleMother (0.25), SingleTank (0.15) |
| E | Sensor_Shooters_12pts (0.3), Sensor_16pts (1.0), SingleTank (0.4), SinglePouncer (0.3), SingleMother (0.2) |

Additional modifier-based waves: Chargers (0.6), Shadows (0.5), Nightmares (0.5), Hybrids (0.4), PouncerShadow (D: 0.2, E: 0.35). Moving sensors remove SingleMother and SingleTank from the pool.

### Cycling (EventLoop)

When `TriggerEach` is enabled, sensors have a chance to cycle on/off via an EventLoop:

| Tier | Cycle Chance |
|------|-------------|
| A | 70% |
| B | 55% |
| C | 40% |
| D | 25% |
| E | 15% |

Cycling parameters: off time = 3-18s, on time = 8-25s. Uses `DisableZoneSensors` to turn off, then `EnableZoneSensors` (TriggerEach) or `EnableZoneSensorsWithReset` (group mode) to turn back on.

---

## Events

### EventsOnTrigger

Standard `WardenObjectiveEvent` list executed when sensors trigger. Common event types:

```json
{
  "Type": 9,
  "Delay": 0.0,
  "EnemyWaveData": {
    "SurvivalWaveSettings": 50,
    "SurvivalWavePopulation": 1,
    "TriggerAlarm": true,
    "IntelMessage": "Security breach detected"
  }
}
```

### Zone Sensor Event Types (400-404)

Defined in `ZoneSensorEventTypes` and `WardenObjectiveEventType`:

| Type | Enum Name | Description |
|------|-----------|-------------|
| 400 | `ToggleSecuritySensor` | Standard toggle. Uses `Enabled` field. Resets all sensors on enable |
| 401 | `ToggleSecuritySensorPreserveTriggered` | Toggle preserving triggered state (triggered sensors stay hidden) |
| 402 | `ToggleSecuritySensorResetTriggered` | Toggle with full reset (clear triggered state, all sensors reappear) |
| 403 | `DisableSecuritySensor` | Disable sensor group (preserves triggered state) |
| 404 | `EnableSecuritySensor` | Enable sensor group (only untriggered sensors appear) |

### Event Targeting

Events support two targeting modes based on the `Count` field:

| `Count` Value | Mode | Target |
|---------------|------|--------|
| `> 0` | ID targeting | `Count` is the definition `Id` (direct) |
| `0` | Zone targeting | Uses `LocalIndex` and `Layer` to find all sensors in that zone |

### Toggle Event JSON

```json
{
  "Type": 400,
  "Enabled": false,
  "Count": 0,
  "Delay": 0.1
}
```

| Field | Description |
| ----- | ----------- |
| `Type` | 400-404 (see table above) |
| `Enabled` | true = enable, false = disable (used by types 400, 402) |
| `Count` | Sensor definition ID (> 0) or zone targeting mode (0) |
| `Delay` | Delay before toggle executes |

### C# Extension Methods

From `Extensions/WardenObjectiveEventCollections.cs`:

```csharp
// ID-based targeting (Count = sensorId)
events.AddToggleSecuritySensors(enabled: false, sensorIndex: 0, delay: 0.1);
events.ToggleZoneSensors(sensorId: 0, enabled: true, delay: 5.0);
events.EnableZoneSensors(sensorId: 0, delay: 0.0);        // Type 404, triggered stay hidden
events.DisableZoneSensors(sensorId: 0, delay: 0.0);       // Type 403
events.EnableZoneSensorsWithReset(sensorId: 0, delay: 0.0);  // Type 402, all sensors reappear
events.ToggleZoneSensorsWithReset(sensorId: 0, enabled: true, delay: 0.0);  // Type 402

// Zone-based targeting (Count = 0, uses zone coordinates)
events.EnableZoneSensorsInZone(zone, delay: 0.0);          // Type 404
events.DisableZoneSensorsInZone(zone, delay: 0.0);         // Type 403
events.ResetZoneSensorsInZone(zone, delay: 0.0);           // Type 402, enable + reset
```

---

## Network Synchronization

### Replicator Architecture

The zone sensor system uses 4 types of `StateReplicator` for network sync:

| Replicator | ID Scheme | Purpose | Sync Frequency |
|------------|-----------|---------|----------------|
| State | `0x5A534E00 + groupIndex` | Group enabled/disabled + 128-bit sensor masks + 128-bit triggered masks | On change |
| Position | `0x5A535000 + groupIndex*8 + batchIndex` | Spawn positions (16 sensors per batch) | Once at build |
| Waypoint | `0x5A535700 + groupIndex*1024 + sensorIndex*8 + batchIndex` | NavMesh path waypoints per moving sensor (20 per batch) | Once at build |
| Movement | `0x5A536000 + groupIndex*4 + batchIndex` | Movement progress (32 sensors per batch) | Every 0.5s |

### Size Constraints

All state structs must fit within StateReplicator's 256-byte payload limit:

| Struct | Size | Layout |
|--------|------|--------|
| `ZoneSensorGroupState` | 33 bytes | 1 (enabled) + 16 (sensor mask) + 16 (triggered mask) |
| `ZoneSensorPositionState` | ~200 bytes | 7 header + 192 data (16 × 12 bytes) |
| `ZoneSensorWaypointState` | 248 bytes | 8 header + 240 data (20 × 12 bytes) |
| `ZoneSensorMovementState` | ~104 bytes | 8 header + 96 data (32 × 3 bytes) |

### Late Joiner Support

The system ensures late joiners see sensors in the correct state:

1. **Position Recall**: Position replicators auto-recall stored state
2. **Waypoint Recall**: Moving sensor paths are recalled from waypoint replicators
3. **Movement State**: Current movement progress applied after sensor spawn
4. **Inactive Creation**: Late joiner sensors start inactive, then apply received state

Flow:
- `isLateJoinerSpawn=true` -> Sensors created inactive
- `OnPositionStateChanged(isRecall=true)` -> Store positions, wait for all batches
- `SpawnSensorsFromBatches()` -> Create sensors, apply stored waypoints + movement
- `UpdateVisualsUnsynced()` -> Apply group enabled/disabled state

### Rebroadcast System

Host repeatedly re-broadcasts all replicator states after level entry to handle slow clients:

- **Interval**: 5 seconds between attempts
- **Attempts**: 6 total (~30s window)
- **Order**: Group state -> Positions -> Waypoints -> Movement
- All rebroadcasts are idempotent (safe to repeat)

---

## Detection Logic

From `ZoneSensorCollider.cs`:

- Checks every **0.1 seconds** (`CHECK_INTERVAL = 0.1f`)
- Counts alive human players within `Radius` distance
- **Triggers on player count increase** (new player entered detection zone)
- Skips bots (`player.Owner.IsBot`) and dead players (`!player.Alive`)
- Checks `gameObject.activeSelf` and `IsGroupEnabled()` before processing
- **TriggerEach mode**: calls `SensorTriggeredIndividual()` -> disables only that sensor
- **Group mode**: calls `SensorTriggered()` -> disables entire group

### ResetState

`ResetState()` zeroes `lastPlayerCount` and `checkTimer`. Called when a sensor transitions from disabled to enabled (`UpdateVisualsUnsynced`), preventing immediate re-triggers when a sensor reappears on top of a player.

---

## Movement System

From `ZoneSensorMover.cs`:

### Initialization

1. Receives a list of patrol positions (start + `Moving - 1` additional random points)
2. Calculates NavMesh paths between consecutive positions using `NavMesh.CalculatePath()`
3. Adjusts waypoints to maintain `EdgeDistance` from NavMesh edges via `PullAwayFromEdge()`
4. Falls back to direct line if no NavMesh path found

### Runtime Movement

- Moves toward current waypoint at constant `Speed` via `Vector3.MoveTowards()`
- Reaches waypoint when within 0.01 units
- **Ping-pong pattern**: reverses direction at path endpoints
- Skips movement when `gameObject.activeSelf` is false (sensor disabled)

### Deterministic Seeding

Waypoint positions are generated using a per-sensor deterministic seed:
```
sensorSeed = SessionSeedRandom.Seed + groupIndex * 1000 + sensorIndex * 100
```
This ensures all clients generate identical initial positions, though actual NavMesh waypoints are synced from host (NavMesh.CalculatePath can differ across clients).

### EdgeDistance Adjustment

`AdjustWaypointsForEdgeDistance()` processes each corner from `NavMesh.CalculatePath()`:
1. Finds closest NavMesh edge via `NavMesh.FindClosestEdge()`
2. If closer than `EdgeDistance`, pulls position away using edge normal
3. Validates new position is still on NavMesh via `NavMesh.SamplePosition()`

---

## Text Animation

From `ZoneSensorTextAnimator.cs`:

When `EncryptedText: true`, sensor text animates through a **9.5-second cycle**:

| Phase | Duration | Display | Color |
|-------|----------|---------|-------|
| 0 - Reveal | 1.2s | Actual text | Normal (`TextColor`) |
| 1 - Partial | 2.15s | Random hex chars (spaces preserved) | Encrypted (`EncryptedTextColor`) |
| 2 - Full Encrypted | 4.0s | `"XX-XX-XX-XX-XX"` format (random hex) | Encrypted (`EncryptedTextColor`) |
| 3 - Partial | 2.15s | Random hex chars (spaces preserved) | Encrypted (`EncryptedTextColor`) |

- Hex characters cycle every **0.6 seconds** (`HEX_CYCLE_INTERVAL`) during phases 1-3
- Each sensor starts at a **random point** in the cycle to prevent synchronized text
- `HideText` takes precedence over `EncryptedText` -- if both are true, no text is shown

When `Text` is `null`, a random corrupted text is selected deterministically:
```
textIndex = (groupIndex * 31 + sensorIndex * 17) % SensorTexts.Count
```

---

## Edge Cases & Gotchas

### Hard Limits
- **128 sensors per group** maximum (bitmask limit: 4 x uint32)
- **16 sensors per position batch** (struct size constraint)
- **20 waypoints per waypoint batch** (248 bytes fits in 256 payload)
- **32 sensors per movement batch** (96 bytes movement data)
- **8 position batches per group** (`MAX_BATCHES_PER_GROUP`), so 128 sensors max
- **8 waypoint batches per sensor** (`MAX_WAYPOINT_BATCHES_PER_SENSOR`), so 160 waypoints per path
- **4 movement batches per group** (`MAX_MOVEMENT_BATCHES`), so 128 sensors with movement
- Waypoint count per sensor capped at 3 additional positions (2 bits in `WaypointCounts`)

### Placement
- Sensors avoid spawning within 15 units of world origin (`IsNearOrigin` check)
- Overlap avoidance: placement retries up to 5 times if new sensor overlaps an existing one (radii summed)
- `AreaIndex = -1` selects random areas using `SessionSeedRandom`
- `NavMesh.SamplePosition()` snaps start positions to NavMesh with 1.0 unit tolerance

### Network Sync
- Only the master (host) executes events and sets state; clients receive updates
- Toggling is scheduled via `ZoneSensorToggleScheduler` (MonoBehaviour with timer, not coroutines -- IL2CPP compatibility)
- All pending toggles are cleared on level cleanup to prevent stale toggles across level transitions
- Pre-registration creates and immediately unloads dummy replicators at `0x5A53FF00-FF` range to ensure network event handlers exist before late joiners
- NetworkAPI must be ready before creating replicators (checked in `BuildSensors`)

### State Management
- `ResetState()` on collider only fires when a sensor transitions from disabled -> enabled (prevents re-trigger on reappear)
- `previousState` tracking ensures transition detection works across state changes
- `sensorsSpawned` flag prevents duplicate spawning from repeated rebroadcasts
- `StartEnabled = false` creates sensors then immediately deactivates GameObjects via `UpdateVisualsUnsynced`

### Movement
- `NavMesh.CalculatePath()` can produce different corners on different clients -- host broadcasts authoritative waypoints
- Clients store received waypoints and apply them after sensors spawn (order-independent)
- Movement progress is quantized to 1 byte (0-255 = 0.0-1.0) for network efficiency
- Waypoint index is clamped to byte range (0-255)
- Boundary cases in `ApplyMovementState` prevent `fromIndex == waypointIndex` (would cause zero-distance calculations)

### Text
- `null` Text generates a random corrupted string per sensor, deterministic from group/sensor index
- `HideText` destroys the TMPro child but leaves the sensor functional
- Random cycle start position means sensors in the same group may show different text phases

---

## Examples

### Basic Sensor Placement

Three sensors in Zone 5, spawn enemies on trigger:

```json
{
  "Name": "Security_Zone",
  "MainLevelLayout": 100,
  "Definitions": [
    {
      "LocalIndex": 5,
      "SensorGroups": [
        {
          "Count": 3,
          "Radius": 2.5
        }
      ],
      "EventsOnTrigger": [
        {
          "Type": 9,
          "EnemyWaveData": {
            "SurvivalWaveSettings": 50,
            "SurvivalWavePopulation": 1
          }
        }
      ]
    }
  ]
}
```

### Moving Sensors

Patrolling sensors that move between two random points:

```json
{
  "SensorGroups": [
    {
      "Count": 2,
      "Moving": 2,
      "Speed": 2.0,
      "Radius": 3.0,
      "EdgeDistance": 0.5
    }
  ]
}
```

### Density-Based Sensors

Let the system calculate count from zone area:

```json
{
  "SensorGroups": [
    {
      "Density": "Medium",
      "Radius": 1.2,
      "TriggerEach": true
    }
  ]
}
```

### Encrypted Text Sensors

Sensors with cycling hex text animation:

```json
{
  "SensorGroups": [
    {
      "Count": 4,
      "Radius": 2.3,
      "EncryptedText": true,
      "Text": "CLASSIFIED DATA"
    }
  ]
}
```

### Sensors Starting Disabled

Spawn sensors disabled, enable them later via events:

```json
{
  "Definitions": [
    {
      "LocalIndex": 3,
      "StartEnabled": false,
      "SensorGroups": [
        {
          "Count": 5,
          "Radius": 2.0
        }
      ],
      "EventsOnTrigger": [
        {
          "Type": 9,
          "EnemyWaveData": {
            "SurvivalWaveSettings": 50,
            "SurvivalWavePopulation": 1
          }
        }
      ]
    }
  ]
}
```

### TriggerEach Mode

Each sensor fires events independently and only that sensor disappears:

```json
{
  "SensorGroups": [
    {
      "Count": 5,
      "TriggerEach": true,
      "Radius": 2.0
    }
  ],
  "EventsOnTrigger": [
    {
      "Type": 5,
      "SoundId": 123456789
    }
  ]
}
```

### Chained Events with Toggle

Sensors that disable themselves after trigger, then re-enable after a delay:

```json
{
  "Definitions": [
    {
      "LocalIndex": 3,
      "SensorGroups": [
        {
          "Count": 2,
          "Radius": 2.3
        }
      ],
      "EventsOnTrigger": [
        {
          "Type": 403,
          "Count": 0,
          "Delay": 0.1
        },
        {
          "Type": 9,
          "Delay": 0.5,
          "EnemyWaveData": {
            "SurvivalWaveSettings": 50,
            "SurvivalWavePopulation": 1
          }
        },
        {
          "Type": 402,
          "Enabled": true,
          "Count": 0,
          "Delay": 30.0
        }
      ]
    }
  ]
}
```

### Custom Visual Appearance

Blue sensor with custom text:

```json
{
  "SensorGroups": [
    {
      "Count": 1,
      "Radius": 4.0,
      "Color": {
        "Red": 0.0,
        "Green": 0.3,
        "Blue": 0.9,
        "Alpha": 0.3
      },
      "Text": "QUARANTINE ZONE",
      "TextColor": {
        "Red": 0.0,
        "Green": 0.8,
        "Blue": 1.0,
        "Alpha": 0.9
      }
    }
  ]
}
```

### Multiple Groups in Same Zone

Different sensor configurations in the same zone (each group gets its own index):

```json
{
  "LocalIndex": 7,
  "SensorGroups": [
    {
      "Count": 3,
      "Radius": 2.0,
      "AreaIndex": 0
    },
    {
      "Count": 1,
      "Radius": 5.0,
      "AreaIndex": 1,
      "Moving": 2
    }
  ]
}
```

---

## Programmatic Usage

### Saving Zone Sensors from Code

```csharp
var levelSensors = new LevelZoneSensors
{
    Name = level.Name,
    MainLevelLayout = levelLayout.PersistentId
};

var definition = new ZoneSensorDefinition
{
    Bulkhead = Bulkhead.Main,
    ZoneNumber = 5,
    StartEnabled = true,
    SensorGroups = new List<ZoneSensorGroupDefinition>
    {
        new()
        {
            Count = 3,
            Radius = 2.5,
            Density = SensorDensity.None
        }
    },
    EventsOnTrigger = new List<WardenObjectiveEvent>
    {
        new() { Type = WardenObjectiveEventType.SpawnEnemyWave, ... }
    }
};

levelSensors.Definitions.Add(definition);
levelSensors.Save();
```

### Toggle via Extension Methods

```csharp
// ID-based: disable sensors when objective completes
objective.EventsOnGotoWin
    .DisableZoneSensors(sensorId: 0, delay: 0.0);

// ID-based: re-enable after wave ends (triggered sensors stay hidden)
waveEvents.EnableZoneSensors(sensorId: 0, delay: 5.0);

// ID-based: re-enable with full reset (all sensors reappear)
waveEvents.EnableZoneSensorsWithReset(sensorId: 0, delay: 5.0);

// ID-based: toggle sensors on/off
events.ToggleZoneSensors(sensorId: 0, enabled: false, delay: 0.0);
events.ToggleZoneSensors(sensorId: 0, enabled: true, delay: 3.0);

// Zone-based: disable all sensors in a zone
events.DisableZoneSensorsInZone(zone, delay: 0.0);

// Zone-based: enable all sensors in a zone
events.EnableZoneSensorsInZone(zone, delay: 0.0);

// Zone-based: reset all sensors in a zone (all reappear)
events.ResetZoneSensorsInZone(zone, delay: 0.0);
```

### Using AddSecuritySensors (Tier-Based)

```csharp
// Auto-configured based on level tier
layout.AddSecuritySensors(node);

// With explicit wave
layout.AddSecuritySensors(node, wave: GenericWave.Sensor_12pts);

// Force moving sensors
layout.AddSecuritySensors(node, moving: true);
```

---

## Key Files

| File | Purpose |
| ---- | ------- |
| `DataBlocks/Custom/ZoneSensors/ZoneSensorDefinition.cs` | Zone-level sensor definition with `StartEnabled` and `Id` |
| `DataBlocks/Custom/ZoneSensors/ZoneSensorGroupDefinition.cs` | Group settings record (all visual/behavior properties) |
| `DataBlocks/Custom/ZoneSensors/SensorDensity.cs` | Density enum for runtime count calculation |
| `DataBlocks/Custom/AutogenRundown/LevelZoneSensors.cs` | JSON file I/O |
| `DataBlocks/LevelLayout.SecuritySensors.cs` | Tier-based generation (`AddSecuritySensors`) |
| `DataBlocks/Objectives/WardenObjectiveEventType.cs` | Event type enum (400-404) |
| `Patches/ZoneSensors/ZoneSensorManager.cs` | Spawning, density calc, events, rebroadcast |
| `Patches/ZoneSensors/ZoneSensorGroup.cs` | Runtime state, replicators, movement init |
| `Patches/ZoneSensors/ZoneSensorCollider.cs` | Player detection MonoBehaviour |
| `Patches/ZoneSensors/ZoneSensorMover.cs` | Movement along NavMesh paths |
| `Patches/ZoneSensors/ZoneSensorGroupState.cs` | 128-bit bitmask network state |
| `Patches/ZoneSensors/ZoneSensorPositionState.cs` | Position batch network state |
| `Patches/ZoneSensors/ZoneSensorWaypointState.cs` | Waypoint batch network state |
| `Patches/ZoneSensors/ZoneSensorMovementState.cs` | Movement progress network state |
| `Patches/ZoneSensors/Patch_ZoneSensorToggle.cs` | Harmony patch for event types 400-404 |
| `Patches/ZoneSensors/ZoneSensorTextAnimator.cs` | Encrypted text cycling animation |
| `Patches/ZoneSensors/ZoneSensorAssets.cs` | Asset loading (CircleSensor prefab) |
| `Extensions/WardenObjectiveEventCollections.cs` | C# extension methods for sensor events |
