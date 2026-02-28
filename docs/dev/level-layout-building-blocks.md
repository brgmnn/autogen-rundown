# Level Layout Building Blocks Reference

This document provides detailed examples of every zone building, progression, and challenge method available in the level layout system. Use this as a reference when creating new objective layouts.

**Source Files:**

- `LevelLayout.ZoneBuild.cs` - Zone creation and branching
- `LevelLayout.ZoneProgression.cs` - Puzzles, alarms, bosses
- `LevelLayout.ZoneBuildChallenge.cs` - Pre-built challenge patterns
- `Zone.Geomorph.cs` - Geomorph/tile type settings

---

## Default Zone Behavior

By default, zones automatically roll the following properties unless specific tags are added to disable or change them:

| Property                | Default Behavior                    | Tags to Modify                                       |
| ----------------------- | ----------------------------------- | ---------------------------------------------------- |
| **Alarms**              | Door alarms rolled based on tier    | `no_alarm` to disable                                |
| **Scouts**              | Scout spawns rolled based on tier   | `no_scouts` to disable                               |
| **Hibernating Enemies** | Enemy population rolled             | `no_enemies` to disable                              |
| **Resources**           | Ammo, tool, and health packs rolled | Manually set `AmmoPacks`, `ToolPacks`, `HealthPacks` |
| **Lights**              | Zone lighting rolled                | Use `LightSettings` property or events               |
| **Blood Doors**         | Can spawn on zone entrances         | `no_blood_door` to disable                           |

These automatic rolls are tier-appropriate and designed to create varied gameplay without manual configuration. When you need explicit control over a zone, add the appropriate tags or set properties directly.

**Example: Disabling automatic enemies and scouts**

```csharp
var (node, zone) = AddZone(sourceNode, new ZoneNode {
    Tags = new Tags("no_enemies", "no_scouts")
});
```

---

## Zone Creation (ZoneBuild.cs)

### AddZone

**Zones Added:** +1

Adds a single zone connected to the source zone.

```csharp
// Basic zone creation
var (node, zone) = AddZone(sourceNode);

// With custom node properties
var (node, zone) = AddZone(sourceNode, new ZoneNode { Branch = "keycard" });

// With max connections set (for hubs)
var (node, zone) = AddZone(sourceNode, new ZoneNode { MaxConnections = 3 });

// With tags
var (node, zone) = AddZone(sourceNode, new ZoneNode {
    Branch = "boss_fight",
    Tags = new Tags("no_blood_door", "no_enemies")
});
```

**Returns:** `(ZoneNode node, Zone zone)` - The created node and zone

---

### AddZone_Forward / AddZone_Left / AddZone_Right / AddZone_Backward

**Zones Added:** +1

Add a zone that expands in a specific direction relative to the bulkhead orientation.

```csharp
// Force expansion forward
var (node, zone) = AddZone_Forward(sourceNode);

// Force expansion left
var (node, zone) = AddZone_Left(sourceNode);

// Force expansion right
var (node, zone) = AddZone_Right(sourceNode);

// Force expansion backward
var (node, zone) = AddZone_Backward(sourceNode);
```

**Returns:** `(ZoneNode node, Zone zone)` - The created node and zone with expansion direction set

---

### AddZone_Side

**Zones Added:** +1

Randomly picks left or right expansion (50/50).

```csharp
// Random left or right expansion
var (node, zone) = AddZone_Side(sourceNode);
```

**Returns:** `(ZoneNode node, Zone zone)`

---

### AddBranch

**Zones Added:** +zoneCount (0 if count is 0)

Creates a chain of connected zones from a base node.

```csharp
// Basic branch: 3 zones in "primary" branch
List<ZoneNode> nodes = AddBranch(baseNode, 3, "primary");

// Access the last node
var lastNode = nodes.Last();
var lastZone = planner.GetZone(lastNode)!;

// With callback to configure each zone
var nodes = AddBranch(baseNode, 2, "power_cell", (node, zone) => {
    zone.Coverage = CoverageMinMax.Small;
    zone.AmmoPacks += 3.0;
    zone.ToolPacks += 2.0;
});

// Zero count returns the base node (no zones added)
var nodes = AddBranch(baseNode, 0, "branch");
// nodes contains only baseNode, callback not invoked
```

**Parameters:**

- `baseNode` - Zone to branch from
- `zoneCount` - Number of zones to create
- `branch` - Branch name for categorization (default: "primary")
- `zoneCallback` - Optional callback invoked for each zone

**Returns:** `List<ZoneNode>` - All created nodes (or just baseNode if count is 0)

---

### AddBranch_Forward / AddBranch_Left / AddBranch_Right / AddBranch_Backward

**Zones Added:** +zoneCount

Same as AddBranch but automatically sets expansion direction for all zones.

```csharp
// 3-zone branch expanding forward
var nodes = AddBranch_Forward(baseNode, 3, "primary");

// With callback for additional configuration
var nodes = AddBranch_Forward(baseNode, 2, "corridor", (node, zone) => {
    zone.GenCorridorGeomorph(level.Complex);
});

// Left expansion branch
var nodes = AddBranch_Left(baseNode, 2, "side_area");

// Right expansion branch
var nodes = AddBranch_Right(baseNode, 2, "side_area");

// Backward expansion (for backtracking areas)
var nodes = AddBranch_Backward(baseNode, 1, "backtrack");
```

**Returns:** `List<ZoneNode>`

---

### AddBranch_Side

**Zones Added:** +zoneCount

Randomly picks left or right for the entire branch (50/50).

```csharp
// Random direction side branch
var nodes = AddBranch_Side(baseNode, 2, "keycard");

// Good for symmetrical layouts where direction doesn't matter
var keycardNodes = AddBranch_Side(hub, 1, "keycard");
var generatorNodes = AddBranch_Side(hub, 1, "generator");
```

**Returns:** `List<ZoneNode>`

---

### CreateElevatorZone

**Zones Added:** +1 (Zone 0)

Creates the very first zone in the level (Zone 0). Only call once per level.

```csharp
// Standard elevator zone
var (elevator, elevatorZone) = CreateElevatorZone();

// With custom settings
var (elevator, elevatorZone) = CreateElevatorZone(new ZoneNode {
    Bulkhead = Bulkhead.Main | Bulkhead.StartingArea,
    Branch = "primary",
    MaxConnections = 3  // Allow more connections from elevator
});
```

**Returns:** `(ZoneNode, Zone)` - The elevator node and zone

---

## Progression Puzzles (ZoneProgression.cs)

### AddKeycardPuzzle

Locks a door behind a keycard that must be found.

```csharp
// Lock door to zone2, keycard in zone1
AddKeycardPuzzle(lockedNode, keycardNode);

// Typical usage with BuildChallenge
var keycardNodes = AddBranch(hub, 1, "keycard");
AddKeycardPuzzle(endNode, keycardNodes.Last());
```

**Layout Result:**

```
keycardNode (has keycard)
                         \
                          hub -> lockedNode (keycard required)
```

---

### AddGeneratorPuzzle

Locks a door behind a power generator that needs a cell.

```csharp
// Lock door to endNode, cell in cellNode
AddGeneratorPuzzle(lockedNode, cellNode);
```

**Side Effects:**

- Turns off lights in the zone before the locked door
- Adds events to turn on emergency lights when cell is inserted
- Places power cell in cellNode

**Layout Result:**

```
cellNode (has power cell)
                         \
                          hub -> lockedNode (generator door, lights off)
```

---

### AddTerminalUnlockPuzzle

Locks a door that must be unlocked via terminal command.

```csharp
// Basic terminal unlock
AddTerminalUnlockPuzzle(lockedNode, terminalNode);

// With password protection (password found on another terminal)
AddTerminalUnlockPuzzle(lockedNode, terminalNode, new TerminalStartingState {
    PasswordProtected = true,
    GeneratePassword = true,
    TerminalZoneSelectionDatas = new() {
        new() {
            new ZoneSelectionData {
                ZoneNumber = passwordNode.ZoneNumber,
                Bulkhead = passwordNode.Bulkhead,
            }
        }
    }
});
```

**Terminal Command Added:** `ACTIVATE_DOOR`

**Layout Result:**

```
terminalNode (terminal with ACTIVATE_DOOR command)
                                                   \
                                                    hub -> lockedNode (locked until command)
```

---

### AddKeyedPuzzle

Auto-generates a keycard branch from available open zones. Useful when you don't want to manually specify the keycard location.

```csharp
// Auto-place keycard somewhere accessible
AddKeyedPuzzle(lockedNode);

// Search specific branch for placement
AddKeyedPuzzle(lockedNode, searchBranch: "side_area");

// Force specific branch length
AddKeyedPuzzle(lockedNode, keyBranchLength: 2);
```

---

## Alarms (ZoneProgression.cs)

### AddApexAlarm

Adds a large, difficult alarm scan. Creates a side spawn room to prevent C-foam holding.

```csharp
// Basic apex alarm (tier-appropriate class)
AddApexAlarm(lockedNode);

// With custom population and settings
AddApexAlarm(lockedNode, WavePopulation.Baseline_Hybrids, WaveSettings.Baseline_Hard);

// Nightmare apex
AddApexAlarm(lockedNode, WavePopulation.Nightmare, WaveSettings.Apex_VeryHard);
```

**Side Effects:**

- Sets SecurityGate to Apex
- Creates side spawn room (prevents holding)
- Chooses tier-appropriate alarm class (8-12)
- Modifies setup zone to hub geomorph

**Tier Alarm Classes:**

- A: Class 8
- B: Class 9
- C: Class 10
- D: Class 11
- E: Class 12

---

### AddApexAlarmZone

Like AddApexAlarm but creates the target zone and returns it. Does NOT create a side spawn room.

```csharp
var (end, endZone) = AddApexAlarmZone(start);

// With custom settings
var (end, endZone) = AddApexAlarmZone(start,
    WavePopulation.Baseline,
    WaveSettings.Apex_Normal);
```

**Returns:** `(ZoneNode, Zone)` - The alarm zone

---

### AddErrorAlarm

Adds a continuous error alarm that optionally can be turned off via terminal.

```csharp
// Error alarm with turn-off terminal
AddErrorAlarm(
    lockedNode,           // Zone with error alarm door
    terminalNode,         // Zone with DEACTIVATE_ALARMS terminal (or null)
    WaveSettings.Error_Normal,
    WavePopulation.Baseline
);

// Error alarm without turn-off (permanent)
AddErrorAlarm(lockedNode, null, WaveSettings.Error_Hard, WavePopulation.Baseline);
```

**Terminal Command Added:** `DEACTIVATE_ALARMS`

**Door Text:** Shows which terminal to use for deactivation

---

### AddApexErrorAlarm

Combines Apex-level door with continuous error alarm. Very difficult.

```csharp
// Apex error alarm with turn-off
AddApexErrorAlarm(
    lockedNode,
    terminalNode,
    WaveSettings.Error_Boss_Hard,
    WavePopulation.SingleEnemy_Tank,
    spawnDelay: 1.0
);
```

**Terminal Command Added:** `FORCE_DEACTIVATE_ALARMS`

---

## Boss Fights (ZoneProgression.cs)

### AddAlignedBoss

Adds a tier-appropriate boss fight to a zone. Enemies are spawn-aligned (in fixed positions).

```csharp
// Add boss fight (tier determines enemy type)
bossNode = AddAlignedBoss(bossNode);
```

**Tier Boss Types:**

- A: Single Mother OR Double Pouncer
- B: Single Mother OR Triple Pouncer
- C: Mother
- D: PMother
- E: Tank+Pouncer, Triple Potato+Tank, or Tank+PMother

**Side Effects:**

- Sets boss-appropriate geomorph
- Adds "no_scouts" tag (except E-tier)
- Adds "no_blood_door" tag (except D/E-tier)
- Adds mother egg sacks or infection props based on boss type

---

### AddAlignedBoss_Hibernate

Like AddAlignedBoss but with a sound reveal when door opens.

```csharp
// Boss hibernating, reveals with sound
bossNode = AddAlignedBoss_Hibernate(bossNode);
```

**Side Effects:** Adds "TenseRevelation" sound on door open

---

### AddAlignedBoss_WakeOnOpen

Like AddAlignedBoss but enemies wake immediately when door opens.

```csharp
// Boss wakes up when you open the door
bossNode = AddAlignedBoss_WakeOnOpen(bossNode);
```

**Side Effects:**

- Alerts enemies in zone on door open
- Plays boss roar sound

---

### AddAlignedBossFight_MegaMom

The hardest boss fight. Spawns a Mega Mother in a large arena.

```csharp
// Basic MegaMom fight
AddAlignedBossFight_MegaMom(bossNode);

// With events on death (e.g., unlock next zone)
var deathEvents = new List<WardenObjectiveEvent>()
    .AddUnlockDoor(director.Bulkhead, nextZone.ZoneNumber);
AddAlignedBossFight_MegaMom(bossNode, deathEvents);
```

**Side Effects:**

- Uses specific large geomorphs (complex-dependent)
- Adds 300 egg sacks
- Adds 12 ammo packs, 4 tool packs
- Plays reveal sound on open
- Alerts enemies 10-12 seconds after door open
- Adds warden intel about the nest

---

## Special Zones (ZoneProgression.cs)

### AddStealth_Infested

Creates a zone filled with infested strikers (harder sleepers).

```csharp
// Add infested zone
node = AddStealth_Infested(node);
```

**Enemy Points by Tier:** A=15, B=20, C=25, D=30, E=35

---

### AddStealth_RespawnZone

Creates a zone where enemies respawn continuously from sacks.

```csharp
// Basic respawn zone
node = AddStealth_RespawnZone(node);

// With custom enemies
node = AddStealth_RespawnZone(node,
    enemyData: EnemySpawningData.Charger,
    respawnSacks: 50);
```

---

### AddScoutRoom

Creates a zone with many scouts that must be cleared.

```csharp
// Scout-filled zone
node = AddScoutRoom(node);
```

**Scout Points by Tier:** A=15, B=20, C=25, D=30, E=40

---

### AddForwardExtract

Creates an extraction point at the end of a forward branch.

```csharp
// Add extraction zone
var exitNode = AddForwardExtract(start);

// With prelude zones before extraction
var exitNode = AddForwardExtract(start, preludeZones: 2);
```

---

### AddForwardExtractStart

Marks a node as a candidate for forward extraction start point. Multiple candidates can be registered; the system picks one based on weighted probability. Only active on Main bulkhead — no-op on Extreme/Overload.

```csharp
// Register with default 100% chance
AddForwardExtractStart(lastNode);

// Register with 30% chance
AddForwardExtractStart(reactor, chance: 0.3);
```

**Parameters:**

- `node` - The zone node to mark as a forward extract start candidate
- `chance` - Probability weight for this candidate (default: 1.0)

**Behavior:**

- Adds the node to `level.ForwardExtractStartCandidates` with the given weight
- Returns immediately (no-op) if `director.Bulkhead != Bulkhead.Main`
- Unlike `AddForwardExtract` which creates extraction zones directly, this registers candidates for the system to choose from later

**Example from ReactorShutdown D/E Main:**
```csharp
// Password terminal is always a candidate
AddForwardExtractStart(pwNodes.Last());
// Reactor itself has a 30% chance of being the extract start
AddForwardExtractStart(reactor, chance: 0.3);
```

---

### AddDisinfectionZone

Adds a small side zone with a disinfection station.

```csharp
// Add disinfection station
AddDisinfectionZone(hub);
```

**Side Effects:**

- Uses complex-appropriate geomorph
- Adds 6 disinfection packs
- Adds disinfection station placement

---

### AddResourceZone

Adds a small resource-rich side zone (armory).

```csharp
AddResourceZone(start);
```

**Resources Added:** 10 ammo, 10 health, 6 tool packs

---

## Challenge Builders (ZoneBuildChallenge.cs)

### BuildChallenge_Small

**Zones Added:** +1 to +2 (random selection)

Randomly picks a small challenge (good for variety).

```csharp
var (end, endZone) = BuildChallenge_Small(start);
```

**Possible Results (equal weight):**

- Single zone with alarm
- Keycard in zone
- Terminal locked door
- Two zones with alarm
- Two zones with keycard in mid
- Two zones with terminal in mid
- Two zones with generator puzzle

---

### BuildChallenge_KeycardInZone

**Zones Added:** +1

Simple keycard puzzle - keycard in start zone, door locked.

```csharp
var (end, endZone) = BuildChallenge_KeycardInZone(start);
```

**Layout:**

```
start (has keycard) -> end (keycard locked)
```

---

### BuildChallenge_KeycardInSide

**Zones Added:** 1 + sideKeycardZones (default: +2)

Keycard in a side branch, door locked.

```csharp
// Default: 1 side zone
var (end, endZone) = BuildChallenge_KeycardInSide(start);

// With multiple keycard zones
var (end, endZone) = BuildChallenge_KeycardInSide(start, sideKeycardZones: 2);
```

**Layout:**

```
start (hub) -> end (keycard locked)
            -> keycard[0] -> keycard[1] (has keycard)
```

**Side Effects:** Modifies start to hub with 3 connections

---

### BuildChallenge_GeneratorCellInZone

**Zones Added:** +1

Generator puzzle - cell in start zone, door powered.

```csharp
var (end, endZone) = BuildChallenge_GeneratorCellInZone(start);
```

**Layout:**

```
start (has cell) -> end (generator locked)
```

---

### BuildChallenge_GeneratorCellInSide

**Zones Added:** 1 + sideCellZones (default: +2)

Generator cell in side branch, door powered.

```csharp
// Default: 1 side zone
var (end, endZone) = BuildChallenge_GeneratorCellInSide(start);

// With multiple cell zones
var (end, endZone) = BuildChallenge_GeneratorCellInSide(start, sideCellZones: 2);
```

**Layout:**

```
start (hub) -> end (generator locked)
            -> power_cell[0] -> power_cell[1] (has cell)
```

**Side Effects:** Modifies start to hub with 3 connections

---

### BuildChallenge_LockedTerminalDoor

**Zones Added:** 1 + sideZones (default: +1)

Terminal unlock puzzle.

```csharp
// Terminal in start zone
var (end, endZone) = BuildChallenge_LockedTerminalDoor(start, sideZones: 0);

// Terminal in side branch
var (end, endZone) = BuildChallenge_LockedTerminalDoor(start, sideZones: 2);
```

**Layout (with side zones):**

```
start (hub) -> end (terminal locked)
            -> terminal_door_unlock[0] -> terminal_door_unlock[1] (has terminal)
```

---

### BuildChallenge_LockedTerminalPasswordInSide

**Zones Added:** +2 (end + password zone)

Terminal unlock with password protection. Password in side zone.

```csharp
// Default: auto-creates side zone
var (end, endZone) = BuildChallenge_LockedTerminalPasswordInSide(start);

// With custom password zone builder
var (end, endZone) = BuildChallenge_LockedTerminalPasswordInSide(start,
    passwordBuilder: (node) => {
        var (pw, pwZone) = AddZone(node, new ZoneNode { Branch = "password" });
        pwZone.GenCorridorGeomorph(level.Complex);
        return (pw, pwZone);
    });
```

**Layout:**

```
start (hub, has password-locked terminal) -> end (terminal locked)
                                          -> terminal_password (has password)
```

---

### BuildChallenge_ApexAlarm

**Zones Added:** +2 (end + side spawn room)

Apex alarm challenge wrapper.

```csharp
var (end, endZone) = BuildChallenge_ApexAlarm(
    start,
    WavePopulation.Baseline,
    WaveSettings.Apex_Normal);
```

**Side Effects:**

- Modifies start to hub
- Adds 3 ammo, 2 tool packs to start
- Adds fog repellers if in fog

---

### BuildChallenge_BossFight

**Zones Added:** +1 (end zone after boss)

Boss fight challenge wrapper.

```csharp
var (end, endZone) = BuildChallenge_BossFight(start);
```

**Side Effects:** Uses AddAlignedBoss_Hibernate (boss reveals with sound)

---

### BuildChallenge_ErrorWithOff_KeycardInSide

**Zones Added:** errorZones + 1 + sideKeycardZones + terminalTurnoffZones

Complex challenge: Error alarm + keycard puzzle + turn-off terminal.

```csharp
var (end, endZone) = BuildChallenge_ErrorWithOff_KeycardInSide(
    start,
    errorZones: 2,           // Zones to traverse during error
    sideKeycardZones: 1,     // Zones to find keycard
    terminalTurnoffZones: 1  // Zones to find turn-off terminal
);
```

**Layout:**

```
start -> firstError (error alarm) -> error[1] -> penultimate (hub) -> end (keycard locked)
                                                                   -> keycard[0] (has keycard)
                                                                   -> error_off[0] (has DEACTIVATE_ALARMS)
```

**Side Effects:**

- Each zone gets 5 ammo, 3 tool, 4 health packs
- Error population adapts to level enemy settings (shadows, chargers, flyers)
- Error settings scale with tier

---

### BuildChallenge_ErrorWithOff_GeneratorCellCarry

**Zones Added:** errorZones + 1 + terminalTurnoffZones

Complex challenge: Error alarm + generator puzzle (must carry cell through error!).

```csharp
var (end, endZone) = BuildChallenge_ErrorWithOff_GeneratorCellCarry(
    start,
    errorZones: 2,
    terminalTurnoffZones: 1
);
```

**Layout:**

```
start -> firstError (error alarm, has cell) -> error[1] -> penultimate -> end (generator locked)
                                                                       -> error_off[0] (DEACTIVATE_ALARMS)
```

**Warning:** Very difficult - requires carrying cell through active error alarm. Not recommended above C-tier.

---

## Composite Builders

These methods build multi-zone structures for specific objective types.

### BuildReactor

**Zones Added:** +2 (corridor + reactor)

**Source File:** `LevelLayout.Reactor.cs`

Creates a corridor + reactor zone pair. The corridor gets reactor-corridor geomorphs and a `reactor_entrance` branch tag. The reactor zone gets reactor geomorphs, matching lighting, fog repellers, and `ForbidTerminalsInZone = true`.

```csharp
var reactorNode = BuildReactor(start);
```

**Returns:** `ZoneNode` - The reactor zone node (not the corridor)

**Side Effects:**

- Creates a corridor zone tagged `reactor_entrance` (important for locked reactor variants)
- Creates a reactor zone with `GenReactorGeomorph`, matching lighting from `Lights.GenReactorLight()`
- Sets `ForbidTerminalsInZone = true` on reactor zone
- Adds fog repellers via `ConsumableDistribution.Reactor_FogRepellers`
- Sets expansion direction and start position to `Furthest` for both zones

**Usage Pattern:**

```csharp
// Basic: build reactor and use it
var reactor = BuildReactor(start);
reactorDefinition.ZoneNumber = reactor.ZoneNumber;

// Locked variant: find the corridor later and hub it
var reactor = BuildReactor(start);
var entranceNode = planner.GetZones(director.Bulkhead, "reactor_entrance").First();
entranceNode = planner.UpdateNode(entranceNode with { MaxConnections = 3 });
planner.GetZone(entranceNode)!.GenHubGeomorph(level.Complex);
// Branch off entranceNode for unlock item
```

---

## Zone Mood Helpers (Private)

These are private helpers but show available zone theming:

### SetMotherVibe

Red/orange lighting, 200 egg sacks

### SetInfectionVibe

Green lighting, 100 spitters

### SetInfestedVibe

Orange/brown lighting (no props)

### SetRespawnVibe

Dark lighting, 50 respawn sacks

---

## Common Patterns

### Hub-and-Spoke Pattern

```csharp
// Create a hub with multiple branches
start = planner.UpdateNode(start with { MaxConnections = 3 });
planner.GetZone(start)!.GenHubGeomorph(level.Complex);

// Main path
var (end, endZone) = AddZone(start);

// Side branch 1
var keycardNodes = AddBranch(start, 1, "keycard");

// Side branch 2
var cellNodes = AddBranch(start, 1, "power_cell");

// Lock end behind keycard
AddKeycardPuzzle(end, keycardNodes.Last());
```

### Linear Progression with Challenges

```csharp
// Zone 1-2: Open path
var nodes = AddBranch_Forward(start, 2);

// Zone 3: Keycard challenge
var (mid, _) = BuildChallenge_KeycardInSide(nodes.Last());

// Zone 4-5: More open zones
var nodes2 = AddBranch_Forward(mid, 2);

// Zone 6: Generator challenge
var (end, endZone) = BuildChallenge_GeneratorCellInSide(nodes2.Last());
```

### Tier-Scaled Challenge Selection

```csharp
switch (level.Tier)
{
    case "A":
        // Simple: just zones
        var nodes = AddBranch_Forward(start, 3);
        break;

    case "B":
        // Keycard challenge
        var (end, _) = BuildChallenge_KeycardInSide(start);
        break;

    case "C":
        // Generator challenge
        var (end, _) = BuildChallenge_GeneratorCellInSide(start);
        break;

    case "D":
        // Error alarm
        var (end, _) = BuildChallenge_ErrorWithOff_KeycardInSide(start, 2, 1, 1);
        break;

    case "E":
        // Apex + boss
        var (mid, _) = BuildChallenge_ApexAlarm(start, WavePopulation.Baseline, WaveSettings.Apex_VeryHard);
        var (end, _) = BuildChallenge_BossFight(mid);
        break;
}
```

### Conditional Layout Options

When some layout variants should only be available under certain conditions, build a mutable options list and conditionally add variants before calling `SelectRun()`.

```csharp
// Build base options list
var options = new List<(double, Action)>
{
    // Always available options
    (0.30, () =>
    {
        var nodes = AddBranch_Forward(start, 2);
        var (end, _) = BuildChallenge_GeneratorCellInSide(nodes.Last());
        AddObjectiveZones(end, objective);
    }),

    (0.30, () =>
    {
        var (mid, _) = BuildChallenge_KeycardInSide(start);
        var (end, _) = BuildChallenge_LockedTerminalDoor(mid, 1);
        AddObjectiveZones(end, objective);
    }),

    (0.25, () =>
    {
        var nodes = AddBranch_Forward(start, 1);
        var (mid, _) = BuildChallenge_KeycardInSide(nodes.Last());
        var (end, _) = BuildChallenge_GeneratorCellInZone(mid);
        AddObjectiveZones(end, objective);
    }),
};

// Conditionally add variant (only when condition is met)
if (objective.ItemCount == 1)
    options.Add((0.15, () =>
    {
        // Special single-item variant
        objective.PlacementNodes.Add(start);
    }));

Generator.SelectRun(options);
```

**When to use this pattern:**

- A layout variant only makes sense under certain conditions (e.g., single terminal vs multiple)
- You want to completely exclude an option rather than have it do nothing
- Cleaner than having fallback logic inside the action

**Avoid this pattern when:**

- All options are always valid - just use inline `Generator.SelectRun()` with a list literal
- The condition affects the weight but not whether the option exists - use a ternary for the weight instead

---

## Geomorph Settings (Zone.Geomorph.cs)

Geomorph settings control the specific room/tile type for each zone. These methods are called on a `Zone` object to assign a particular tile type. Each method picks a random geomorph from a curated list appropriate for the complex type.

**Source File:** `Zone.Geomorph.cs`

### GenHubGeomorph

Hub geomorphs are large rooms that can connect to zones on multiple sides. Use with `MaxConnections = 3` in the planner.

```csharp
zone.GenHubGeomorph(level.Complex);

// Typically combined with node settings
start = planner.UpdateNode(start with { MaxConnections = 3 });
planner.GetZone(start)!.GenHubGeomorph(level.Complex);
```

**Characteristics:**

- 64x64 tiles with multiple exit points
- Can connect to 3 new zones (X-shaped layout)
- Ideal for hub-and-spoke level designs
- Sets `SubComplex`, `CustomGeomorph`, and `Coverage` automatically

---

### GenCorridorGeomorph

I-shaped corridor geomorphs that connect two zones in a line. Use with `MaxConnections = 1` in the planner.

```csharp
zone.GenCorridorGeomorph(level.Complex);
```

**Characteristics:**

- 64x64 linear tiles
- Can only connect to one new zone (I-shaped layout)
- Ideal for creating linear paths or connecting hubs
- Sets `SubComplex`, `CustomGeomorph`, and `Coverage` automatically

---

### GenTGeomorph

T-shaped geomorphs for three-way intersections.

```csharp
zone.GenTGeomorph(level.Complex);
```

**Characteristics:**

- Similar to hubs but T-shaped instead of X-shaped
- Limited selection (mostly mod geomorphs)

---

### GenExitGeomorph

Exit/extraction tiles for level endings.

```csharp
zone.GenExitGeomorph(level.Complex);
```

**Characteristics:**

- 64x64 or 32x32 tiles with extraction points
- Sets `Coverage` to `Tiny` automatically
- Required for extraction objectives

---

### GenReactorGeomorph

Reactor room geomorphs for reactor startup/shutdown objectives.

```csharp
zone.GenReactorGeomorph(level.Complex);
```

**Characteristics:**

- **Only works in Mining and Tech complexes** (Service requires mod geomorphs)
- Sets `IgnoreRandomGeomorphRotation = true`
- Sets `Coverage` to fixed 40
- Large arena-style rooms with reactor structures

---

### GenReactorCorridorGeomorph

Corridors specifically designed to lead into reactor rooms.

```csharp
zone.GenReactorCorridorGeomorph(level.Complex);
```

**Characteristics:**

- Thematic connection between regular zones and reactor zones
- Slightly larger coverage than standard corridors

---

### GenGeneratorClusterGeomorph

Geomorphs containing a generator cluster for power cell objectives.

```csharp
zone.GenGeneratorClusterGeomorph(level.Complex);
```

**Characteristics:**

- Sets `GeneratorClustersInZone = 1`
- Fixed coverage of 40
- Required for Central Generator Cluster objectives

---

### GenKingOfTheHillGeomorph

Objective-focused geomorphs for terminal uplink and survival objectives. Also positions a terminal at a specific location in the room.

```csharp
zone.GenKingOfTheHillGeomorph(level, director);
```

**Characteristics:**

- Adds terminal placement with specific position/rotation for the geomorph
- Large arena rooms suitable for wave defense
- Used for Terminal Uplink objectives

---

### GenDeadEndGeomorph

Dead-end zones for optional areas or final objective rooms.

```csharp
zone.GenDeadEndGeomorph(level.Complex);
```

**Characteristics:**

- Small coverage (5-15)
- Single entrance design
- Good for resource stashes, keycard rooms, or side objectives

---

### GenBossGeomorph

Large arena geomorphs designed for boss encounters.

```csharp
zone.GenBossGeomorph(level.Complex);
```

**Characteristics:**

- Large coverage (30-75)
- Open arena design for boss fights
- Used by `AddAlignedBoss` methods

---

### GenMatterWaveProjectorGeomorph

Specialized geomorphs for Matter Wave Projector pickup locations.

```csharp
zone.GenMatterWaveProjectorGeomorph(level.Complex);
```

**Characteristics:**

- Complex-specific tiles
- Mining: DigSite MWP rooms
- Tech: Lab dead-end with data sphere/neonate needle
- Service: Floodways hub pit room

---

### GenGardenGeomorph

Garden-themed geomorphs (Service complex only).

```csharp
zone.GenGardenGeomorph(level.Complex);
```

**Characteristics:**

- **Only works in Service complex** (no-op for other complexes)
- Sets `SubComplex` to `Gardens`
- Large coverage (50-75)
- Greenhouse/botanical aesthetic

---

### GenPortalGeomorph

Portal geomorphs for dimensional travel objectives.

```csharp
zone.GenPortalGeomorph();
```

**Characteristics:**

- **Only works in Mining and Tech complexes**
- Mining: Contains possible path forward
- Tech: Dead-end design
- Service: Logs error (not supported)

---

### Geomorph Properties Reference

When geomorph methods are called, they set these zone properties:

| Property                       | Description                                                                            |
| ------------------------------ | -------------------------------------------------------------------------------------- |
| `SubComplex`                   | The sub-complex type (DigSite, Refinery, Storage, Lab, DataCenter, Floodways, Gardens) |
| `CustomGeomorph`               | Path to the specific geomorph prefab asset                                             |
| `Coverage`                     | Room size/coverage area (Min/Max values)                                               |
| `IgnoreRandomGeomorphRotation` | When `true`, prevents random rotation (used for reactors)                              |
| `GeneratorClustersInZone`      | Number of generator clusters to spawn                                                  |

---

### Complex and SubComplex Reference

| Complex | SubComplexes               |
| ------- | -------------------------- |
| Mining  | DigSite, Refinery, Storage |
| Tech    | Lab, DataCenter            |
| Service | Floodways, Gardens         |

Geomorph methods automatically select appropriate SubComplexes for the given Complex.
