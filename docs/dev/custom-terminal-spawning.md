# Custom Terminal Spawning

This document describes how to spawn terminals at custom locations in GTFO, bypassing the normal marker-based placement system.

## Background

The default terminal placement system works as follows:

1. **Distribution phase** (`TerminalDistribution` batch): `LG_Distribute_TerminalsPerZone` adds terminals to the distribution queue based on `TerminalPlacementData`
2. **Function markers phase** (`FunctionMarkers` batch): `LG_PopulateFunctionMarkersInZoneJob` spawns terminals at marker spawner locations
3. **Marker spawners**: Predefined spawn points baked into geomorph prefabs, grouped by `ExpeditionFunction` type

### Problems with Moving Existing Terminals

The previous approach (in `Patch_LG_ComputerTerminal_Setup.cs`) moved terminals after they were spawned:

1. **Orphaned scenery**: The original spawn location has scenery/geometry set up for the terminal
2. **Overlapping objects**: The new location might have other markers spawned there
3. **Culling issues**: Terminal may not render correctly if moved outside its original area

## Solution: Spawn Before Markers

### Factory Batch Order (Relevant)

```
Geomorphs (8)        - Geomorph prefabs instantiated
Areas (9)            - Areas set up, marker spawners registered
...
DistributionSetup (14)
TerminalDistribution (15)
Distribution (16)    - Good place to spawn custom terminals
FunctionMarkers (17) - Normal markers spawn here
...
FunctionMarkerFallback (40)
FinalLogicLinking (59)
```

### Key Classes and Methods

#### Terminal Prefab
```
Assets/AssetPrefabs/Complex/Generic/FunctionMarkers/Terminal_Floor.prefab
```

#### Loading Assets
```csharp
var prefab = AssetShardManager.GetLoadedAsset<GameObject>(TERMINAL_PREFAB);
// or
var prefab = AssetAPI.GetLoadedAsset<GameObject>(TERMINAL_PREFAB);
```

#### Spawning
```csharp
var terminal = GOUtil.SpawnChildAndGetComp<LG_ComputerTerminal>(prefab, worldPos, worldRot);
```

#### Terminal Setup Requirements
```csharp
// 1. Set SpawnNode (for game integration, optional but recommended)
terminal.SpawnNode = area.m_courseNode;

// 2. Call Setup
terminal.Setup(startStateData, terminalPlacementData);

// 3. Register with zone
zone.TerminalsSpawnedInZone.Add(terminal);

// Note: LG_ComputerTerminalManager.RegisterTerminal() is called automatically by Setup()
```

### Marker Spawner System

Each `LG_Area` has `m_markerSpawnerGroups` - an array indexed by `ExpeditionFunction`:

```csharp
// Get all spawners for a function type
List<LG_MarkerSpawner> spawners = area.GetAllMarkerSpawners(ExpeditionFunction.Terminal);

// Remove a specific spawner (from all groups)
area.RemoveMarkerSpawner(spawner);

// Remove from specific function group
area.RemoveMarkerSpawner(spawner, ExpeditionFunction.Terminal);

// Get spawner count
int count = area.GetMarkerSpawnerCount(ExpeditionFunction.Terminal);
```

### Clearing Markers Near Custom Position

To prevent other objects from spawning at/near your terminal position:

```csharp
private static void ClearMarkersNearPosition(LG_Area area, Vector3 localPosition, float radius = 2.0f)
{
    var worldPos = area.transform.TransformPoint(localPosition);

    var funcCount = EnumUtil.GetValueLength<ExpeditionFunction>();
    for (int func = 0; func < funcCount; func++)
    {
        var spawners = area.GetAllMarkerSpawners((ExpeditionFunction)func);
        foreach (var spawner in spawners.ToList())
        {
            var spawnerWorldPos = spawner.m_parent.transform.TransformPoint(spawner.m_localPosition);
            if (Vector3.Distance(spawnerWorldPos, worldPos) < radius)
            {
                area.RemoveMarkerSpawner(spawner, (ExpeditionFunction)func);
            }
        }
    }
}
```

### Injecting Custom Factory Jobs

```csharp
LG_Factory.InjectJob(
    new MyCustomTerminalJob(...),
    LG_Factory.BatchName.Distribution  // Before FunctionMarkers
);
```

## Implementation Strategy

### Two-Phase Approach

1. **After areas are built** (hook `LG_Floor.OnAreasCreated` or similar):
   - Find the target area for each custom terminal
   - Remove marker spawners within radius of desired position
   - This prevents other objects from spawning there

2. **Before FunctionMarkers** (inject job in `Distribution` batch):
   - Load terminal prefab
   - Instantiate at exact position
   - Set `SpawnNode` to area's course node
   - Call `Setup()`
   - Add to zone's terminal list

### Example Custom Factory Job

```csharp
public class SpawnCustomTerminalJob : LG_FactoryJob
{
    private const string TERMINAL_PREFAB =
        "Assets/AssetPrefabs/Complex/Generic/FunctionMarkers/Terminal_Floor.prefab";

    private readonly LG_Zone _zone;
    private readonly LG_Area _area;
    private readonly Vector3 _worldPosition;
    private readonly Quaternion _worldRotation;

    public SpawnCustomTerminalJob(LG_Zone zone, LG_Area area, Vector3 pos, Quaternion rot)
    {
        _zone = zone;
        _area = area;
        _worldPosition = pos;
        _worldRotation = rot;
    }

    public override bool Build()
    {
        var prefab = AssetShardManager.GetLoadedAsset<GameObject>(TERMINAL_PREFAB);
        if (prefab == null)
            return true; // Job complete (with error)

        var terminal = GOUtil.SpawnChildAndGetComp<LG_ComputerTerminal>(
            prefab, _worldPosition, _worldRotation);

        terminal.SpawnNode = _area.m_courseNode;
        terminal.Setup();
        _zone.TerminalsSpawnedInZone.Add(terminal);

        return true; // Job complete
    }
}
```

## Reference: How Game Spawns Terminals

### Via Marker System (Normal Flow)
From `LG_FunctionMarkerBuilder.SetupFunctionGO()`:
```csharp
LG_ComputerTerminal terminal = GO.GetComponentInChildren<LG_ComputerTerminal>();
if (terminal != null)
{
    terminal.Setup(m_terminalStartStateData, m_terminalPlacementData);
    m_node.m_zone.TerminalsSpawnedInZone.Add(terminal);
    // ... warden objective setup if applicable
}
```

### Via Direct Spawn (Reactor Terminal)
From `LG_WardenObjective_Reactor`:
```csharp
m_terminal = GOUtil.SpawnChildAndGetComp<LG_ComputerTerminal>(m_terminalPrefab, m_terminalAlign);
m_terminal.Setup();
m_terminal.ConnectedReactor = this;
```

### Via TGA_End_Room
```csharp
LG_ComputerTerminal comp = GOUtil.SpawnChildAndGetComp<LG_ComputerTerminal>(
    m_terminalPrefab, m_terminalAlign);
comp.Setup();
LG_ComputerTerminalManager.RegisterTerminal(comp);
```

## Important Notes

1. **SpawnNode**: Setting `terminal.SpawnNode` enables:
   - Proper zone/area association
   - Terminal registration with course node
   - Correct behavior for location-based features

2. **Setup() handles**:
   - State replicator creation (multiplayer sync)
   - Serial number assignment
   - Terminal item interface setup
   - State machine initialization
   - Registration with `LG_ComputerTerminalManager`

3. **Timing**: Must spawn after geomorphs/areas are created but before markers

4. **Position calculation**: Use `area.transform.TransformPoint(localPos)` to convert local geomorph coordinates to world coordinates
