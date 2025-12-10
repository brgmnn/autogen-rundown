# Zone Layout System - Developer Guide

This guide explains how to work with the Zone, ZoneNode, LayoutPlanner, and LevelLayout systems in AutogenRundown. It's intended for AI agents (Claude) and developers creating new features or fixing issues.

## Architecture Overview

The zone system has four interconnected components:

```
┌─────────────────────────────────────────────────────────────────┐
│                         LevelLayout                             │
│  (Partial record - primary API for building zones)              │
│  Files: LevelLayout.*.cs                                        │
└───────────────────────────┬─────────────────────────────────────┘
                            │ uses
                            ▼
┌─────────────────────────────────────────────────────────────────┐
│                       LayoutPlanner                             │
│  (Directed graph managing zone connections)                     │
│  File: DataBlocks/Zones/LayoutPlanner.cs:86-576                 │
└───────────────────────────┬─────────────────────────────────────┘
                            │ manages
                            ▼
        ┌───────────────────┴───────────────────┐
        │                                       │
┌───────┴───────┐                     ┌─────────┴─────────┐
│   ZoneNode    │  ◄─── maps to ───►  │      Zone         │
│  (Identifier) │                     │ (Configuration)   │
│  Lightweight  │                     │  Full datablock   │
└───────────────┘                     └───────────────────┘
```

### ZoneNode (Lightweight Identifier)

**File**: `src/DataBlocks/Zones/LayoutPlanner.cs:6-35`

```csharp
public record struct ZoneNode(
    Bulkhead Bulkhead,          // Main, Extreme, Overload, StartingArea
    int ZoneNumber,             // Index within bulkhead (0, 1, 2, ...)
    string Branch = "primary",  // Branch identifier
    int MaxConnections = 2)     // Max child zones allowed
{
    public Tags Tags { get; set; } = new();
}
```

Key points:

- **Equality** is based solely on `Bulkhead + ZoneNumber` - other properties are metadata
- Use `Branch` to categorize zones (e.g., `"primary"`, `"keycard"`, `"power_cell"`)
- `MaxConnections` controls how many zones can branch off this one. **This cannot be larger than 3, the game has a max of 3 child connections**
- `Tags` control generation behavior (e.g., `"no_enemies"`, `"no_scouts"`, `"no_blood_door"`)

### Zone (Full Configuration)

**File**: `src/DataBlocks/Zone.cs`

Contains all zone data: geometry, enemies, alarms, resources, terminals, fog, etc. Always access via `planner.GetZone(node)`.

### LayoutPlanner (Graph Manager)

**File**: `src/DataBlocks/Zones/LayoutPlanner.cs:86-576`

Maintains a directed graph of zone connections. Key methods:

- `Connect(from, to)` - Create edge between zones
- `AddZone(node, zone)` - Register zone data
- `GetZone(node)` - Retrieve Zone from ZoneNode
- `GetOpenZones(bulkhead, branch)` - Find zones with available connection slots
- `UpdateNode(node)` - Update node properties across the graph

### LevelLayout (Primary API)

Partial record spread across multiple files. This is what you use to build levels.

---

## Core Concepts

### Bulkheads

Levels can have multiple bulkhead sections:

- `Bulkhead.Main` - Primary progression path
- `Bulkhead.Extreme` - Secondary objective area
- `Bulkhead.Overload` - Tertiary objective area
- `Bulkhead.StartingArea` - Elevator zone (part of Main but before main bulkhead door)

The `director.Bulkhead` tells you which section you're currently building.

### Branches

Branch names categorize zones for querying:

- `"primary"` - Main progression path (default)
- `"keycard"`, `"keycard_1"`, `"keycard_2"` - Keycard storage branches
- `"power_cell"` - Generator cell storage
- `"error_turnoff"` - Error alarm deactivation terminal
- `"hsu_sample"` - HSU objective location
- `"reactor_code_0"`, `"reactor_code_1"` - Reactor code fetch areas
- Custom names for your needs

### Tags

Tags modify zone generation behavior:

| Tag             | Effect                    |
| --------------- | ------------------------- |
| `no_enemies`    | Skip enemy spawning roll  |
| `no_scouts`     | Prevent scout enemies     |
| `no_blood_door` | Block blood door rolling  |
| `reactor`       | Reactor zone (no enemies) |
| `boss_megamom`  | MegaMom boss marker       |

Add tags via:

```csharp
node = planner.UpdateNode(node with { Tags = node.Tags.Extend("no_enemies", "no_scouts") });
```

### MaxConnections

Controls zone branching capacity:

- Default is `2` - allows two child zones
- Set up to `3` for hub (X-tile) zones
- Set to `1` for corridor (I-tile) zones
- Set to `0` for dead ends

---

## Common Patterns

### Creating Zones

**File**: `src/DataBlocks/LevelLayout.ZoneBuild.cs`

#### Basic Zone Creation

```csharp
// Add zone with default settings
var (node, zone) = AddZone(sourceNode);

// Add zone with specific branch
var (node, zone) = AddZone(sourceNode, new ZoneNode { Branch = "keycard" });

// Add zone with max connections set
var (node, zone) = AddZone(sourceNode, new ZoneNode { MaxConnections = 3 });
```

#### Directional Zone Creation

```csharp
// Force zone to expand in specific direction relative to bulkhead
var (node, zone) = AddZone_Forward(sourceNode);
var (node, zone) = AddZone_Left(sourceNode);
var (node, zone) = AddZone_Right(sourceNode);
var (node, zone) = AddZone_Backward(sourceNode);
var (node, zone) = AddZone_Side(sourceNode);  // Random left or right
```

### Creating Branches

```csharp
// Create a chain of 3 zones
List<ZoneNode> nodes = AddBranch(baseNode, zoneCount: 3, branch: "primary");
// Returns list of all created nodes

// Get the last zone in the branch
var lastNode = nodes.Last();
var lastZone = planner.GetZone(lastNode)!;

// With callback to configure each zone
var nodes = AddBranch(baseNode, 2, "power_cell", (node, zone) => {
    zone.Coverage = CoverageMinMax.Small;
    zone.AmmoPacks += 3.0;
});

// Directional branches
var nodes = AddBranch_Forward(baseNode, 3, "primary");
var nodes = AddBranch_Left(baseNode, 2, "keycard");
var nodes = AddBranch_Side(baseNode, 1, "power_cell");  // Random direction
```

### Hub-and-Spoke Pattern

Common for creating a central zone with multiple branches:

```csharp
// Make start a hub with 3+ connections
start = planner.UpdateNode(start with { MaxConnections = 3 });
var startZone = planner.GetZone(start)!;
startZone.GenHubGeomorph(level.Complex);  // Set appropriate large geomorph

// Add main progression zone
var (end, endZone) = AddZone(start, new ZoneNode { Branch = "primary" });

// Add side branch for keycard
var keycardNodes = AddBranch(start, 1, "keycard");

// Lock end behind keycard
AddKeycardPuzzle(end, keycardNodes.Last());
```

### Adding Progression Puzzles

**File**: `src/DataBlocks/LevelLayout.ZoneProgression.cs`

#### Generator (Power Cell) Puzzle

```csharp
// lockedNode = zone whose door requires power
// cellNode = zone where power cell is placed
AddGeneratorPuzzle(lockedNode, cellNode);
```

This:

- Locks the door with `ProgressionPuzzle.Generator`
- Places a power cell in `cellNode`
- Turns off lights in the setup zone
- Adds events to turn on emergency lights when cell inserted

#### Keycard Puzzle

```csharp
// lockedNode = zone whose door requires keycard
// keycardNode = zone where keycard is placed
AddKeycardPuzzle(lockedNode, keycardNode);
```

#### Terminal Unlock Puzzle

```csharp
// Door locked until ACTIVATE_DOOR command run on terminal
AddTerminalUnlockPuzzle(lockedNode, terminalNode);

// With password protection (password on terminal in another zone)
AddTerminalUnlockPuzzle(lockedNode, terminalNode, new TerminalStartingState {
    PasswordProtected = true,
    GeneratePassword = true,
    TerminalZoneSelectionDatas = /* password zone reference */
});
```

### Adding Alarms and Challenges

#### Apex Alarms

High-difficulty alarms with large scans:

```csharp
// Basic apex alarm
AddApexAlarm(lockedNode);

// With custom population and settings
AddApexAlarm(lockedNode, WavePopulation.Baseline_Hybrids, WaveSettings.Baseline_Normal);
```

The `AddApexAlarm` method:

- Creates a side spawn room to prevent c-foam holding
- Selects tier-appropriate ChainedPuzzle (Class8-12 based on tier)
- Sets SecurityGate to Apex

#### Error Alarms

Continuous alarms that can optionally be turned off:

```csharp
// Error alarm with turn-off terminal
AddErrorAlarm(
    lockedNode,       // Zone with error alarm door
    terminalNode,     // Zone with DEACTIVATE_ALARMS terminal (or null)
    WaveSettings.Error_Normal,
    WavePopulation.Baseline
);
```

#### Boss Fights

```csharp
// Add tier-appropriate boss to zone
bossNode = AddAlignedBoss(bossNode);

// With hibernation reveal (sound on door open)
bossNode = AddAlignedBoss_Hibernate(bossNode);

// Wake enemies when door opens
bossNode = AddAlignedBoss_WakeOnOpen(bossNode);

// MegaMom (hardest boss)
AddAlignedBossFight_MegaMom(bossNode, onDeathEvents);
```

### Using Challenge Builders

**File**: `src/DataBlocks/LevelLayout.ZoneBuildChallenge.cs`

Pre-built challenge patterns that return `(ZoneNode end, Zone endZone)`:

```csharp
// Keycard in same zone
var (end, endZone) = BuildChallenge_KeycardInZone(start);
// Layout: start (has keycard) -> end (locked)

// Keycard in side branch
var (end, endZone) = BuildChallenge_KeycardInSide(start, sideKeycardZones: 2);
// Layout: start(hub) -> end (locked)
//                    -> keycard[0] -> keycard[1] (has keycard)

// Generator cell in side branch
var (end, endZone) = BuildChallenge_GeneratorCellInSide(start, sideCellZones: 2);
// Layout: start(hub) -> end (generator locked)
//                    -> power_cell[0] -> power_cell[1] (has cell)

// Terminal unlock
var (end, endZone) = BuildChallenge_LockedTerminalDoor(start, sideZones: 1);
// Layout: start(hub) -> end (locked)
//                    -> terminal_door_unlock[0] (has terminal)

// Apex alarm
var (end, endZone) = BuildChallenge_ApexAlarm(start, population, settings);

// Boss fight
var (end, endZone) = BuildChallenge_BossFight(start);

// Error alarm with keycard
var (end, endZone) = BuildChallenge_ErrorWithOff_KeycardInSide(
    start,
    errorZones: 2,          // Zones to traverse during error
    sideKeycardZones: 1,    // Zones to find keycard
    terminalTurnoffZones: 1 // Zones to find turn-off terminal
);

// Error alarm with generator (carry cell through error)
var (end, endZone) = BuildChallenge_ErrorWithOff_GeneratorCellCarry(
    start,
    errorZones: 2,
    terminalTurnoffZones: 1
);

// Random small challenge
var (end, endZone) = BuildChallenge_Small(start);
// Randomly selects from: simple zone, keycard, terminal, generator variations
```

### Tier-Specific Patterns

Most objective layouts use tier switching:

```csharp
switch (level.Tier, director.Bulkhead)
{
    case ("A", Bulkhead.Main):
        // Easy layout for A-tier main
        break;

    case ("B", Bulkhead.Main):
        // Harder layout for B-tier main
        break;

    case ("C", Bulkhead.Extreme):
        // C-tier extreme objective
        break;

    // ... etc

    default:
        // Fallback
        break;
}
```

Use `Generator.SelectRun()` for weighted random selection:

```csharp
Generator.SelectRun(new List<(double, Action)>
{
    (0.40, () => { /* 40% chance: layout option 1 */ }),
    (0.35, () => { /* 35% chance: layout option 2 */ }),
    (0.25, () => { /* 25% chance: layout option 3 */ }),
});
```

### Resource Distribution

Add resources to zones based on difficulty:

```csharp
zone.AmmoPacks += 5.0;
zone.ToolPacks += 3.0;
zone.HealthPacks += 4.0;
zone.DisinfectPacks += 4.0;  // For fog levels

// In callbacks during branch creation
var nodes = AddBranch(start, 3, "primary", (node, zone) => {
    zone.AmmoPacks += 3.0;
    zone.HealthPacks += 2.0;
});
```

---

## Working with the Code

### Adding a New Objective Type Layout

1. Create new file: `src/DataBlocks/LevelLayout.{ObjectiveName}.cs`

2. Add partial class method:

```csharp
namespace AutogenRundown.DataBlocks;

public partial record LevelLayout
{
    public void BuildLayout_{ObjectiveName}(BuildDirector director, WardenObjective objective, ZoneNode? startish)
    {
        if (startish == null)
        {
            Plugin.Logger.LogError($"No node returned");
            throw new Exception("No zone node returned");
        }

        var start = (ZoneNode)startish;
        var startZone = planner.GetZone(start)!;

        switch (level.Tier, director.Bulkhead)
        {
            case ("A", Bulkhead.Main):
                // Build A-tier main layout
                break;

            // Add cases for each tier/bulkhead combo

            default:
                // Fallback layout
                break;
        }
    }
}
```

3. Call from main LevelLayout.Build() flow (in `LevelLayout.cs`)

### Creating New Challenge Patterns

Add to `LevelLayout.ZoneBuildChallenge.cs`:

```csharp
public (ZoneNode, Zone) BuildChallenge_MyNewChallenge(
    ZoneNode start,
    int someParameter = 1)
{
    // 1. Setup start zone if needed
    start = planner.UpdateNode(start with { MaxConnections = 3 });
    var startZone = planner.GetZone(start)!;
    startZone.GenHubGeomorph(level.Complex);

    // 2. Create the main/end zone
    var (end, endZone) = AddZone(start, new ZoneNode { Branch = "primary" });

    // 3. Create any side branches
    var sideNodes = AddBranch(start, someParameter, "my_side_branch");

    // 4. Add puzzles/locks
    AddKeycardPuzzle(end, sideNodes.Last());

    // 5. Return the end node/zone
    return (end, endZone);
}
```

### Modifying Existing Layouts

1. Find the objective file (e.g., `LevelLayout.HsuFindSample.cs`)
2. Locate the tier/bulkhead case you want to modify
3. Adjust zone creation, branching, or puzzle setup
4. Test with fixed seed to reproduce results

---

## Debugging & Troubleshooting

### Common Generation Failure Patterns

#### "Failed to find any good StartAreas" Error

```
[Error] WARNING : Zone1 (Zone_1 - 544): Failed to find any good StartAreas in zone 0 (543) expansionType:Towards_Random
```

**Causes**:

- Too many zones branching from one source (exceeded MaxConnections)
- Zone crowding - not enough physical space
- Custom geomorph too small for connection count

**Solutions**:

1. Increase `MaxConnections` on hub zones
2. Increase zone coverage: `zone.Coverage.Min += 20; zone.Coverage.Max += 20;`
3. Use appropriate geomorphs for branching (`GenHubGeomorph`, `GenTGeomorph`)
4. Spread branches across multiple zones instead of one hub

#### Infinite Loading / Hang

The game repeatedly tries to place zones with no success.

**Solutions**:

1. Check `MaxConnections` - ensure zones don't exceed their limits
2. Verify custom geomorphs have adequate space
3. Use `PlanBulkheadPlacements()` to auto-adjust (see LayoutPlanner.cs:559-575)

### Checking Zone Graph

Use `planner.ToString()` to debug the zone graph:

```csharp
Plugin.Logger.LogDebug(planner.ToString());
```

Output:

```
Graph:
  ZoneNode(Main, 0) (door=None, expand=Forward) => [ZoneNode(Main, 1)]
  ZoneNode(Main, 1) (door=Forward, expand=Forward) => [ZoneNode(Main, 2), ZoneNode(Main, 3)]
  ...
```

### Common Mistakes

1. **Forgetting to update MaxConnections before branching**:

```csharp
// WRONG - default MaxConnections is 2
var (branch1, _) = AddZone(start);
var (branch2, _) = AddZone(start);
var (branch3, _) = AddZone(start);  // May fail!

// RIGHT
start = planner.UpdateNode(start with { MaxConnections = 3 });
// Now safe to add 3 branches
```

2. **Not setting hub geomorph for multi-branch zones**:

```csharp
// WRONG - default zone may be too small
start = planner.UpdateNode(start with { MaxConnections = 4 });

// RIGHT
start = planner.UpdateNode(start with { MaxConnections = 4 });
var startZone = planner.GetZone(start)!;
startZone.GenHubGeomorph(level.Complex);
```

3. **Using wrong branch name in queries**:

```csharp
// WRONG - "keycard_1" vs "keycard"
var keycards = AddBranch(start, 2, "keycard_1");
var zones = planner.GetZones(director.Bulkhead, "keycard");  // Returns empty!

// RIGHT - use consistent names
var keycards = AddBranch(start, 2, "keycard");
var zones = planner.GetZones(director.Bulkhead, "keycard");
```

### Log Locations

- BepInEx logs: `BepInEx/LogOutput.log`
- Generated datablocks: `BepInEx/GameData/{GameRevision}/`

### Testing with Fixed Seeds

Set seeds in config to reproduce generation:

```
BepInEx/config/000-the_tavern-AutogenRundown.cfg
```

---

## Quick Reference

### Key Files

| File                                | Purpose                      |
| ----------------------------------- | ---------------------------- |
| `LevelLayout.ZoneBuild.cs`          | Zone/branch creation methods |
| `LevelLayout.ZoneProgression.cs`    | Puzzles, alarms, bosses      |
| `LevelLayout.ZoneBuildChallenge.cs` | Reusable challenge patterns  |
| `LevelLayout.{Objective}.cs`        | Objective-specific layouts   |
| `Zones/LayoutPlanner.cs`            | Graph management, ZoneNode   |
| `Zone.cs`                           | Zone data structure          |

### Common Method Signatures

```csharp
// Zone creation
(ZoneNode, Zone) AddZone(ZoneNode source, ZoneNode? newNode = null)
(ZoneNode, Zone) AddZone_Forward/Left/Right/Backward/Side(ZoneNode source, ZoneNode? newNode = null)

// Branch creation
List<ZoneNode> AddBranch(ZoneNode baseNode, int zoneCount, string branch = "primary", Action<ZoneNode, Zone>? callback = null)
List<ZoneNode> AddBranch_Forward/Left/Right/Backward/Side(...)

// Puzzles
void AddGeneratorPuzzle(ZoneNode lockedNode, ZoneNode cellNode)
void AddKeycardPuzzle(ZoneNode lockedNode, ZoneNode keycardNode)
void AddTerminalUnlockPuzzle(ZoneNode lockedNode, ZoneNode terminalNode, TerminalStartingState? startingState = null)

// Alarms
void AddApexAlarm(ZoneNode lockedNode, WavePopulation? population = null, WaveSettings? settings = null)
void AddErrorAlarm(ZoneNode lockedNode, ZoneNode? terminalNode, WaveSettings? settings, WavePopulation? population)

// Bosses
ZoneNode AddAlignedBoss(ZoneNode bossNode)
ZoneNode AddAlignedBoss_Hibernate(ZoneNode bossNode)
void AddAlignedBossFight_MegaMom(ZoneNode bossNode, ICollection<WardenObjectiveEvent>? onDeathEvents = null)

// Challenge builders
(ZoneNode, Zone) BuildChallenge_KeycardInZone/InSide(ZoneNode start, ...)
(ZoneNode, Zone) BuildChallenge_GeneratorCellInZone/InSide(ZoneNode start, ...)
(ZoneNode, Zone) BuildChallenge_LockedTerminalDoor(ZoneNode start, int sideZones = 0)
(ZoneNode, Zone) BuildChallenge_ApexAlarm(ZoneNode start, WavePopulation population, WaveSettings settings)
(ZoneNode, Zone) BuildChallenge_BossFight(ZoneNode start)
(ZoneNode, Zone) BuildChallenge_ErrorWithOff_KeycardInSide(ZoneNode start, int errorZones, int sideKeycardZones, int terminalTurnoffZones)
```

### Common Tags

```csharp
"no_enemies"     // Skip enemy spawning
"no_scouts"      // No scout enemies
"no_blood_door"  // Block blood door
"reactor"        // Reactor zone
"boss_megamom"   // MegaMom marker
```
