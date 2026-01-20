# Zone Sensors - Developer Guide

This guide documents the Zone Sensor system for placing custom security sensors that detect players and trigger events.

## Overview

Zone sensors are security sensors placed automatically within zones using the game's navigation mesh. Unlike vanilla sensors (placed via level datablocks), zone sensors are:

- Defined in JSON files, separate from level generation
- More configurable (custom colors, text, movement)
- Triggered per-group or individually
- Controllable via events (enable/disable at runtime)

## Architecture Overview

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
| `LevelZoneSensors` | `DataBlocks/Custom/AutogenRundown/LevelZoneSensors.cs` | Top-level JSON structure, file I/O |
| `ZoneSensorManager` | `Patches/ZoneSensors/ZoneSensorManager.cs` | Spawning and event handling singleton |
| `ZoneSensorGroup` | `Patches/ZoneSensors/ZoneSensorGroup.cs` | Runtime state, network sync via StateReplicator |
| `ZoneSensorCollider` | `Patches/ZoneSensors/ZoneSensorCollider.cs` | Player detection MonoBehaviour |
| `ZoneSensorMover` | `Patches/ZoneSensors/ZoneSensorMover.cs` | Movement along NavMesh paths |
| `ZoneSensorGroupState` | `Patches/ZoneSensors/ZoneSensorGroupState.cs` | Network replication state struct |

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
      "SensorGroups": [
        {
          "AreaIndex": -1,
          "Count": 3,
          "Radius": 2.3,
          "Color": {
            "Red": 0.93,
            "Green": 0.10,
            "Blue": 0.0,
            "Alpha": 0.26
          },
          "Text": "S:_EC/uR_ITY S:/Ca_N",
          "TextColor": {
            "Red": 0.88,
            "Green": 0.90,
            "Blue": 0.89,
            "Alpha": 0.70
          },
          "TriggerEach": false,
          "Moving": false,
          "Speed": 1.5,
          "EdgeDistance": 0.1
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
| `DimensionIndex` | string | "Reality" | Dimension: "Reality", "Dimension_1", "Dimension_2" |
| `LayerType` | string | "MainLayer" | Layer: "MainLayer", "SecondaryLayer", "ThirdLayer" |
| `LocalIndex` | int | 0 | Zone number within the layer |
| `SensorGroups` | List | [] | Groups of sensors to place |
| `EventsOnTrigger` | List | [] | Events executed when any sensor triggers |

### ZoneSensorGroupDefinition Properties

| Property | Type | Default | Description |
| -------- | ---- | ------- | ----------- |
| `AreaIndex` | int | -1 | Area within zone (-1 = random areas) |
| `Count` | int | 1 | Number of sensors in this group |
| `Radius` | double | 2.3 | Detection radius of each sensor |
| `Color` | Color | Red (R8D1 style) | Color of the sensor visual ring |
| `Text` | string | "S:_EC/uR_ITY S:/Ca_N" | Text displayed on sensor |
| `TextColor` | Color | Light gray | Color of the sensor text |
| `TriggerEach` | bool | false | If true, each sensor triggers independently |
| `Moving` | bool | false | If true, sensors patrol between two points |
| `Speed` | double | 1.5 | Movement speed (units/second) when Moving |
| `EdgeDistance` | double | 0.1 | Min distance from NavMesh edges for waypoints when Moving |
| `Height` | double | 0.6 | Height of sensor visual, combined with Radius for vertical offset |

### Color Format

```json
{
  "Red": 0.0-1.0,
  "Green": 0.0-1.0,
  "Blue": 0.0-1.0,
  "Alpha": 0.0-1.0
}
```

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

### ToggleSecuritySensor (Type 400)

Enable or disable sensor groups at runtime:

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
| `Type` | 400 (ToggleSecuritySensor) |
| `Enabled` | true = enable group, false = disable |
| `Count` | Sensor group index (0-based) |
| `Delay` | Delay before toggle executes |

**C# Helper Methods:**

```csharp
// Extension methods in WardenObjectiveEventCollections.cs
events.AddToggleSecuritySensors(enabled: false, sensorIndex: 0, delay: 0.1);
events.ToggleZoneSensors(groupIndex: 0, enabled: true, delay: 5.0);
events.EnableZoneSensors(groupIndex: 0, delay: 0.0);
events.DisableZoneSensors(groupIndex: 0, delay: 0.0);
events.EnableZoneSensorsWithReset(groupIndex: 0, delay: 0.0);  // Full reset, all sensors reappear
events.ToggleZoneSensorsWithReset(groupIndex: 0, enabled: true, delay: 0.0);  // Toggle with reset
```

### Network Synchronization

Sensor state is synchronized via `StateReplicator<ZoneSensorGroupState>`:

- **Enabled**: Group-level enable/disable state
- **SensorMask**: Bitmask for individual sensor states (supports up to 32 sensors per group)

Only the master client executes events and sets state; other clients receive state updates.

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
      "Moving": true,
      "Speed": 2.0,
      "Radius": 3.0,
      "EdgeDistance": 0.5
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
          "Type": 400,
          "Enabled": false,
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
          "Type": 400,
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
      "Moving": true
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
    SensorGroups = new List<ZoneSensorGroupDefinition>
    {
        new() { Count = 3, Radius = 2.5 }
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
// In objective events - disable sensors when objective completes
objective.EventsOnGotoWin
    .DisableZoneSensors(groupIndex: 0, delay: 0.0);

// Re-enable after wave ends
waveEvents.EnableZoneSensors(groupIndex: 0, delay: 5.0);

// Re-enable with full reset (all sensors reappear)
waveEvents.EnableZoneSensorsWithReset(groupIndex: 0, delay: 5.0);

// Toggle sensors on/off
events.ToggleZoneSensors(groupIndex: 0, enabled: false, delay: 0.0);
events.ToggleZoneSensors(groupIndex: 0, enabled: true, delay: 3.0);
```

---

## Implementation Details

### Sensor Spawning Flow

1. `ZoneSensorManager.Setup()` loads all JSON files at game data init
2. `LevelAPI.OnBuildDone` triggers `BuildSensors()`
3. For each definition matching the current level layout:
   - Look up zone by dimension/layer/local index
   - Create `ZoneSensorGroup` with StateReplicator
   - Spawn sensors using `GetRandomPositionInside()` on NavMesh
   - Add `ZoneSensorCollider` for detection
   - Add `ZoneSensorMover` if moving enabled

### Detection Logic (ZoneSensorCollider)

- Checks every 0.1 seconds (configurable via `CHECK_INTERVAL`)
- Counts alive human players within radius
- Triggers on player count increase (player entered)
- Skips bots and dead players
- TriggerEach mode: triggers `SensorTriggeredIndividual()`
- Group mode: triggers `SensorTriggered()`

### Movement Logic (ZoneSensorMover)

- Calculates NavMesh path between start and random end point
- Moves along path waypoints at constant speed
- Reverses direction at path ends (ping-pong pattern)
- Falls back to direct line if no NavMesh path found

### Network Sync

- `StateReplicator<ZoneSensorGroupState>` syncs enable/disable state
- SensorMask bitmask tracks individual sensor states
- Only master executes events and broadcasts state changes
- Clients receive state via `OnStateChanged` callback

---

## Key Files

| File | Purpose |
| ---- | ------- |
| `DataBlocks/Custom/ZoneSensors/ZoneSensorDefinition.cs` | Zone-level sensor definition |
| `DataBlocks/Custom/ZoneSensors/ZoneSensorGroupDefinition.cs` | Group settings record |
| `DataBlocks/Custom/AutogenRundown/LevelZoneSensors.cs` | JSON file I/O |
| `Patches/ZoneSensors/ZoneSensorManager.cs` | Spawning and event handling |
| `Patches/ZoneSensors/ZoneSensorGroup.cs` | Runtime state management |
| `Patches/ZoneSensors/ZoneSensorCollider.cs` | Player detection MonoBehaviour |
| `Patches/ZoneSensors/ZoneSensorMover.cs` | Movement MonoBehaviour |
| `Patches/ZoneSensors/ZoneSensorGroupState.cs` | Network state struct |
| `Patches/ZoneSensors/Patch_ZoneSensorToggle.cs` | Harmony patch for type 400 events |
| `Patches/ZoneSensors/ZoneSensorAssets.cs` | Asset loading (CircleSensor prefab) |
