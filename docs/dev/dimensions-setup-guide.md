# Dimensions Setup Guide

How to create and configure alternate dimensions in autogen-rundown levels.

## Overview

A level can use up to 20 alternate dimensions (`Dimension1`-`Dimension20`) plus the `Arena` dimension (used for pouncer fights). Reality (`DimensionIndex.Reality`) is the main level and is always present. Each dimension has its own environment: fog, lighting, enemies, resources, and atmosphere.

There are two types of dimensions:

- **Static dimensions** — a single pre-built geomorph. The entire dimension is one prefab (e.g. a desert arena, mining shaft). No zone generation.
- **Generated dimensions** — procedurally generated zones using a level layout, just like Reality. The dimension geomorph serves as the elevator/starting area equivalent.

## Static vs Generated Dimensions

### Static Dimensions

Set `LevelLayoutData = 0` and pick the prefab in `DimensionGeomorph`. The dimension is entirely that one geomorph with no generated zones.

```csharp
var dimension = new Dimension
{
    Data = new Dimensions.DimensionData
    {
        LevelLayoutData = 0,  // No level layout — static
        DimensionGeomorph = "Assets/AssetPrefabs/Complex/Dimensions/Desert/Dimension_Desert_Boss_Arena.prefab",
        IsStaticDimension = true,
        // ... other settings
    }
};
```

Use `Static*` fields to configure the environment: `StaticLightSettings`, `StaticEnemySpawningInZone`, `StaticTerminalPlacements`, resource multipliers (`StaticHealthMulti`, `StaticWeaponAmmoMulti`, etc.).

### Generated Dimensions

Set `DimensionGeomorph` to the elevator/starting zone equivalent, and `LevelLayoutData` to the persistent ID of a level layout that defines the zone structure.

```csharp
var dimension = new Dimension
{
    Data = new Dimensions.DimensionData
    {
        LevelLayoutData = myLayout.PersistentId,  // References a LevelLayout
        DimensionGeomorph = "Assets/AssetPrefabs/Complex/Dimensions/Desert/Dimension_Desert_R6A2.prefab",
        // ... other settings
    }
};
```

The dimension geomorph acts as the spawn/elevator area, and zones generate outward from it using the referenced layout — same as how Reality uses the elevator drop zone plus a level layout.

## Setting Up a Dimension

### Step 1: Create the DimensionData

Either create from scratch or use a preset with `with` to override specific fields:

```csharp
// From preset
var dimension = new Dimension
{
    Data = Dimensions.DimensionData.AlphaOne with
    {
        ForbidWaveSpawning = true,
        StaticTerminalPlacements = new List<TerminalPlacement> { ... }
    }
};

// From scratch
var dimension = new Dimension
{
    Data = new Dimensions.DimensionData
    {
        LevelLayoutData = 0,
        DimensionGeomorph = "Assets/AssetPrefabs/Complex/Dimensions/PouncerArena/Dimension_Pouncer_Arena_01.prefab",
        IsStaticDimension = true,
        ForbidWaveSpawning = true,
        ForbidCarryItemWarps = true,
        LeaveDeployablesOnWarp = true,
        // ...
    }
};
```

### Step 2: Persist the Dimension

Call `FindOrPersist()` to deduplicate and register the dimension datablock. If an identical dimension already exists, it returns the existing one instead of creating a duplicate.

```csharp
dimension.FindOrPersist();
```

### Step 3: Register on the Level

Add the dimension to the level's `DimensionDatas` list with a `DimensionIndex` slot:

```csharp
level.DimensionDatas.Add(new Levels.DimensionData
{
    Dimension = DimensionIndex.Dimension1,
    Data = dimension
});
```

Each `DimensionIndex` slot can only be used once per level. The `Arena` slot is already taken by the Pouncer Arena (registered by default on all levels).

### Step 4: Create Portal and MWP Zones

Players need a way to reach the dimension. This requires two zones in Reality:

**Portal zone** — contains the physical portal object:

```csharp
var (portal, portalZone) = AddZone_Forward(startNode);
portalZone.GenPortalGeomorph();
```

**MWP zone** — contains the Matter Wave Projector item that activates the portal:

```csharp
var (mwp, mwpZone) = BuildChallenge_Small(portal);
mwpZone.GenMatterWaveProjectorGeomorph(level.Complex);
mwpZone.BigPickupDistributionInZone = BigPickupDistribution.MatterWaveProjector.PersistentId;
```

Portal geomorphs are only available for **Mining** and **Tech** complexes. Service complex does not have portal geomorphs.

### Step 5: Add Warp/Portal Events

Events on the portal zone fire when the portal is used:

```csharp
portalZone.EventsOnPortalWarp.Add(
    new WardenObjectiveEvent
    {
        Type = WardenObjectiveEventType.SpawnEnemyWave,
        Delay = 10.0,
        SoundId = Sound.Enemies_DistantLowRoar,
        EnemyWaveData = new GenericWave
        {
            Population = WavePopulation.OnlyNightmareGiants,
            Settings = WaveSettings.Error_Hard
        },
        Dimension = DimensionIndex.Dimension1  // Spawn in the dimension
    });
```

## Warp Event Types

Two event types move players between dimensions:

### DimensionWarpTeam (Permanent)

Permanently warps the entire team to a dimension. One-way — players stay in the destination.

```csharp
new WardenObjectiveEvent
{
    Type = WardenObjectiveEventType.DimensionWarpTeam,
    Dimension = DimensionIndex.Dimension1,
    Layer = (int)Bulkhead.Main,
    Delay = 0.0,
    ClearDimension = false  // Set true to kill all enemies on arrival
}
```

### DimensionFlashTeam (Temporary)

Temporarily warps the team to a dimension for `Duration` seconds, then auto-returns them.

```csharp
new WardenObjectiveEvent
{
    Type = WardenObjectiveEventType.DimensionFlashTeam,
    Dimension = DimensionIndex.Dimension1,
    Layer = (int)Bulkhead.Main,
    Duration = 30.0,  // Seconds before auto-return
    Delay = 2.0,      // Seconds before warp executes
    ClearDimension = true  // Kill enemies on arrival
}
```

These events can be placed on:

- `zone.EventsOnPortalWarp` — fires when portal is used
- `zone.EventsOnOpenDoor` — fires when the zone's security door opens
- Objective event lists (`EventsOnActivate`, `EventsOnGotoWin`, etc.)

## Key DimensionData Fields

### Behavior Controls

| Field                        | Default | Description                                                                                    |
| ---------------------------- | ------- | ---------------------------------------------------------------------------------------------- |
| `ForbidWaveSpawning`         | false   | Prevents alarm-triggered enemy waves from spawning in the dimension                            |
| `ForbidCarryItemWarps`       | false   | Items (pickups, cells, etc.) stay behind when players warp — they don't travel with the player |
| `LeaveDeployablesOnWarp`     | false   | Sentries, mines, and C-foam stay in the origin dimension when players warp out                 |
| `ForbidTerminalsInDimension` | false   | No terminals spawn in the dimension                                                            |

### Static Dimension Environment

| Field                                    | Default | Description                              |
| ---------------------------------------- | ------- | ---------------------------------------- |
| `StaticLightSettings`                    | 0       | Light preset ID                          |
| `StaticEnemySpawningInZone`              | []      | Pre-spawned enemy groups (JArray)        |
| `StaticTerminalPlacements`               | []      | Terminal objects placed in the dimension |
| `StaticHealthMulti`                      | 0.0     | Health pack multiplier                   |
| `StaticToolAmmoMulti`                    | 0.0     | Tool refill multiplier                   |
| `StaticWeaponAmmoMulti`                  | 0.0     | Ammo refill multiplier                   |
| `StaticDisinfectionMulti`                | 0.0     | Disinfection station multiplier          |
| `StaticAllowResourceContainerAllocation` | false   | Allow resource boxes                     |
| `StaticAllowSmallPickupsAllocation`      | false   | Allow small item spawns                  |
| `StaticForceBigPickupsAllocation`        | false   | Force big pickup placement               |

### Atmosphere (Outdoor Dimensions)

Set `IsOutside = true` to enable outdoor rendering (sky, sun, clouds). Key atmosphere fields:

| Field                               | Default    | Description             |
| ----------------------------------- | ---------- | ----------------------- |
| `LightAzimuth` / `LightElevation`   | 0.0 / 45.0 | Sun direction           |
| `LightIntensity`                    | 1.0        | Sun brightness          |
| `AmbientIntensity`                  | 1.0        | Ambient light level     |
| `AtmosphereData`                    | 0          | Atmosphere preset ID    |
| `Exposure`                          | 1.0        | Camera exposure         |
| `CloudsData`                        | 0          | Cloud preset ID         |
| `CloudsCoverage`                    | 1.0        | Cloud density           |
| `Sandstorm`                         | false      | Enable sandstorm effect |
| `SandstormEdgeA` / `SandstormEdgeB` | 0.4 / 0.6  | Sandstorm fog bounds    |

### Other

| Field                        | Default     | Description                                                                                                                               |
| ---------------------------- | ----------- | ----------------------------------------------------------------------------------------------------------------------------------------- |
| `DimensionResourceSetID`     | 51          | Resource set for the dimension                                                                                                            |
| `DimensionFogData`           | 81          | Fog settings persistent ID                                                                                                                |
| `VerticalExtentsUp` / `Down` | 50.0 / 50.0 | Vertical space reserved. Dimensions are stacked vertically above Reality — these values determine how much Y-space the dimension occupies |
| `SoundEventOnWarpTo`         | Sound.Warp  | Sound played when warping in                                                                                                              |
| `UseLocalDimensionSeeds`     | true        | Use independent random seeds for the dimension                                                                                            |
| `ObjectiveType`              | 2           | Objective type reference                                                                                                                  |
| `LinkedToLayer`              | 0           | Layer this dimension is linked to (0=Main, 1=Extreme, 2=Overload)                                                                         |

## Available Presets

Pre-configured `DimensionData` statics in `Dimensions.DimensionData`:

| Preset             | Geomorph                        | Description                       | Notes                                                                           |
| ------------------ | ------------------------------- | --------------------------------- | ------------------------------------------------------------------------------- |
| `Unknown_One`      | `Dimension_Desert_Boss_flash`   | Small map, distant Kraken visible | R6C1. Sandstorm enabled                                                         |
| `Unknown_Two`      | `Dimension_Desert_Static_02`    | Desert flash                      | R6A1. Not suitable for >10s stays. Heavy sandstorm                              |
| `Unknown_Three`    | `Dimension_Desert_R7B3`         | R7B3 variant                      | Sandstorm, no terminals                                                         |
| `AlphaOne`         | `Dimension_Desert_Boss_Arena`   | Kraken boss fight arena           | R6D1. Has resource multipliers (health 4x, weapon ammo 14x)                     |
| `AlphaTwo`         | `Dimension_Desert_Static_01`    | Large desert, ~2 min run to base  | R6B1. Sandstorm enabled                                                         |
| `AlphaThree_Top`   | `Dimension_Desert_R6A2`         | Small map with giant fan vent     | R6AX. Has `LevelLayoutData` — generated dimension. Resource multipliers         |
| `AlphaThree_Shaft` | `Dimension_Desert_Mining_Shaft` | Inside the fan hole               | R6CX. Walking enemy spawns don't work well here                                 |
| `AlphaSix`         | `Dimension_Desert_Dune_camp_03` | Desert camp with outpost terminal | R7C2. ~65s run to outpost. Set `MLSLevelKit = 1` on level for correct materials |

All presets are outdoor (`IsOutside = true`) with atmosphere/cloud/sandstorm settings tuned.

Use `with` to customize a preset:

```csharp
Dimensions.DimensionData.AlphaOne with
{
    ForbidWaveSpawning = true,
    StaticTerminalPlacements = new List<TerminalPlacement> { ... }
}
```

## Available Dimension Geomorphs

### Dimension Prefabs

| Prefab                                                      | Type                                      |
| ----------------------------------------------------------- | ----------------------------------------- |
| `Dimensions/Desert/Dimension_Desert_Static_01.prefab`       | Large desert (AlphaTwo)                   |
| `Dimensions/Desert/Dimension_Desert_Static_02.prefab`       | Desert flash (Unknown_Two)                |
| `Dimensions/Desert/Dimension_Desert_Boss_flash.prefab`      | Boss flash / distant Kraken (Unknown_One) |
| `Dimensions/Desert/Dimension_Desert_Boss_Arena.prefab`      | Boss fight arena (AlphaOne)               |
| `Dimensions/Desert/Dimension_Desert_R7B3.prefab`            | R7B3 variant (Unknown_Three)              |
| `Dimensions/Desert/Dimension_Desert_R6A2.prefab`            | Fan vent area (AlphaThree_Top)            |
| `Dimensions/Desert/Dimension_Desert_Mining_Shaft.prefab`    | Mine shaft (AlphaThree_Shaft)             |
| `Dimensions/Desert/Dimension_Desert_Dune_camp_03.prefab`    | Dune camp (AlphaSix)                      |
| `Dimensions/PouncerArena/Dimension_Pouncer_Arena_01.prefab` | Pouncer boss arena                        |

### Portal Geomorphs (Reality zones)

| Complex | Prefab                                                  | Notes                       |
| ------- | ------------------------------------------------------- | --------------------------- |
| Mining  | `Mining/Geomorphs/geo_64x64_mining_portal_HA_01.prefab` | Has a possible path forward |
| Tech    | `Tech/Geomorphs/geo_64x64_portal_HA_01.prefab`          | Dead end tile               |
| Service | None                                                    | Not supported               |

### MWP Geomorphs (Reality zones)

| Complex | Prefab(s)                                                                                                                     |
| ------- | ----------------------------------------------------------------------------------------------------------------------------- |
| Mining  | `Mining/Geomorphs/Digsite/geo_64x64_mining_dig_site_hub_SF_02.prefab` or `SamdownGeos/T2D2 MWP room/MWP_Room.prefab` (random) |
| Tech    | `Tech/Geomorphs/geo_64x64_Lab_dead_end_room_02.prefab`                                                                        |
| Service | `Service/Geomorphs/Maintenance/geo_64x64_service_floodways_hub_SF_01.prefab`                                                  |

## Complete Working Example

From `BuildLayout_GatherTerminal_AlphaSix` — the reference pattern for dimension usage in objectives:

```csharp
// 1. Create portal zone in Reality
var (portal, portalZone) = AddZone_Forward(start);
portalZone.GenPortalGeomorph();

// 2. Create MWP zone (side branch off portal zone)
var (mwp, mwpZone) = BuildChallenge_Small(portal);
mwpZone.GenMatterWaveProjectorGeomorph(level.Complex);
mwpZone.BigPickupDistributionInZone = BigPickupDistribution.MatterWaveProjector.PersistentId;
mwpZone.EnemySpawningInZone.Add(EnemySpawningData.Pouncer);

// 3. Add events that fire when portal is used
portalZone.EventsOnPortalWarp.Add(
    new WardenObjectiveEvent
    {
        Type = WardenObjectiveEventType.SpawnEnemyWave,
        Delay = 10.0,
        SoundId = Sound.Enemies_DistantLowRoar,
        EnemyWaveData = new GenericWave
        {
            Population = WavePopulation.OnlyNightmareGiants,
            Settings = WaveSettings.Error_Hard
        },
        Dimension = DimensionIndex.Dimension1
    });

// 4. Create dimension (static, using AlphaOne preset with terminal placements)
var dimension = new Dimension
{
    Data = Dimensions.DimensionData.AlphaOne with
    {
        StaticTerminalPlacements = new List<TerminalPlacement>
        {
            new()
            {
                LogFiles = new List<LogFile>
                {
                    DLockDecipherer.R1B1_Z40
                }
            }
        }
    }
};
dimension.FindOrPersist();

// 5. Register dimension on level
level.DimensionDatas.Add(new Levels.DimensionData
{
    Dimension = DimensionIndex.Dimension1,
    Data = dimension
});
```

## Notes

- The **Pouncer Arena** is registered on all levels by default (`DimensionIndex.Arena`). You don't need to add it manually.
- Portal geomorphs only exist for **Mining** and **Tech** complexes. Service complex will log an error.
- MWP geomorphs exist for all three complexes (Mining, Tech, Service).
- When using `AlphaSix`, set `level["MLSLevelKit"] = 1` for correct material rendering.
- `AlphaThree_Shaft` doesn't work well with walking enemy spawns due to how the room geometry splits.
- `Unknown_Two` is not suitable for stays longer than ~10 seconds — it's a brief "flash" dimension.
- Dimension slots are limited: `Dimension1`-`Dimension20`. Using more than a few per level is uncommon.
- `FindOrPersist()` deduplicates dimensions — if two objectives create identical dimension data, only one datablock is persisted. Call it instead of `Persist()` directly.
- `ClearDimension = true` on a warp event kills all enemies in the dimension on arrival. Useful for resetting arenas between visits.
