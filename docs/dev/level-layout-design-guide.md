# Level Layout Design Guide

This guide covers the design patterns and requirements for creating well-rounded level layouts with proper tier and bulkhead differentiation. For technical API usage, see `zone-layout-system.md`.

## Design Philosophy

Level layouts should:

1. **Scale with tier difficulty** - A-tier should be easier than E-tier
2. **Respect bulkhead purpose** - Main, Extreme, and Overload have different roles
3. **Consider multi-objective complexity** - Account for combined run times
4. **Provide variety** - Multiple layout variants prevent repetition

---

## Tier Differentiation (A-E)

Each tier represents increasing difficulty. Layouts should scale accordingly:

| Tier | Difficulty | Zone Target Count | Challenge Types                           | Enemy Density |
| ---- | ---------- | ----------------- | ----------------------------------------- | ------------- |
| A    | Easy       | 2-4               | Simple keycards,                          | Low           |
| B    | Normal     | 3-5               | Keycards                                  | Moderate      |
| C    | Moderate   | 4-7               | Generator puzzles, class alarms           | Medium        |
| D    | Hard       | 5-9               | Error alarms, terminal puzzles            | High          |
| E    | Extreme    | 6-12+             | Apex alarms, boss fights, complex puzzles | Very High     |

### Progression Example

```csharp
switch (level.Tier)
{
    case "A":
        // Simple linear layout, maybe one keycard
        var nodes = AddBranch_Forward(start, 3, "primary");
        AddKeycardPuzzle(nodes.Last(), nodes[1]);
        break;

    case "B":
        // Add side branches
        start = planner.UpdateNode(start with { MaxConnections = 3 });
        var (end, _) = BuildChallenge_KeycardInSide(start, sideKeycardZones: 1);
        break;

    case "C":
        // Generator puzzle, some scans
        var (end, endZone) = BuildChallenge_GeneratorCellInSide(start, sideCellZones: 2);
        endZone.Alarm = ChainedPuzzle.TeamScan;
        break;

    case "D":
        // Error alarm with turn-off
        var (end, _) = BuildChallenge_ErrorWithOff_KeycardInSide(start,
            errorZones: 2, sideKeycardZones: 2, terminalTurnoffZones: 1);
        break;

    case "E":
        // Apex alarm, boss fight, complex multi-part
        var (mid, _) = BuildChallenge_ApexAlarm(start, WavePopulation.Baseline, WaveSettings.Apex_Normal);
        var (end, _) = BuildChallenge_BossFight(mid);
        break;
}
```

---

## Bulkhead Types

### Main (Primary Objective)

The Main bulkhead is the primary mission path that players must complete.

**Characteristics:**

- Longest and most substantial layout
- Contains the primary objective
- Usually 5-12 zones depending on tier
- Can include complex multi-stage challenges

**Zone Budget:** 8-15 zones (or ~15 when secondary objectives exist)

### Extreme (Easier Secondary)

Extreme is the **easier** secondary objective, typically done before Overload.

**Characteristics:**

- Shorter than Main
- Lower difficulty scaling than same-tier Main
- Players are often already depleted from Main when attempting
- Should provide resources to help with Overload

**Zone Budget:** 4-8 zones

**Design Pattern:**

```csharp
case ("C", Bulkhead.Extreme):
    // Extreme should feel like a B-tier Main
    // Shorter, less complex puzzles
    var nodes = AddBranch_Forward(start, 4, "primary");
    var (end, _) = BuildChallenge_KeycardInSide(nodes.Last(), sideKeycardZones: 1);
    break;
```

### Overload (Harder Secondary)

Overload is the **hardest** secondary objective, meant for experienced players.

**Characteristics:**

- Shortest but most intense
- Higher difficulty than same-tier Main
- Often features apex alarms, bosses, or error alarms
- Players are typically at lowest resources

**Zone Budget:** 3-6 zones

**Design Pattern:**

```csharp
case ("C", Bulkhead.Overload):
    // Overload should feel like a D-tier Main but shorter
    // Intense challenges, fewer zones
    var (end, _) = BuildChallenge_ErrorWithOff_KeycardInSide(start,
        errorZones: 2, sideKeycardZones: 1, terminalTurnoffZones: 1);
    AddAlignedBoss(end);
    break;
```

---

## Multi-Objective Complexity

Levels can have different objective combinations. The layout must account for total session time.

### Main Only (Single Objective)

When Main is the only objective:

- Can use full zone budget (15-20 zones)
- Include comprehensive resource distribution
- Can have longer multi-stage challenges

**Target Time:** ~1.5-2 hours

### Main + Secondary (Two Objectives)

When Main has Extreme OR Overload:

- Main: 10-15 zones
- Secondary: 5-10 zones
- Total: ~20-25 zones

**Target Time:** ~1.5-2 hours combined

### Prisoner Efficiency (All Three Objectives)

"PE runs" include Main + Extreme + Overload. This is the most demanding configuration.

**Zone Budget:**

- Main: 8-12 zones (reduced from full)
- Extreme: 4-6 zones
- Overload: 3-5 zones
- **Total: ~20-25 zones maximum**

**Critical Considerations:**

- Players will be resource-depleted by Overload
- Each section should provide enough resources for itself + some carry-over
- Overload should be short but intense, not drawn-out

**Design Pattern:**

Basic design patterns to follow:

- Use the `level` instancec variable to access the parent level object that this layout belongs to. See `Level` class for details.
- Use the `director` instance variable to access build settings for this particular bulkhead. Things like whether it's Main/Extreme/Overload, what level settings there are etc. See `BuildDirector` class for details.

Examples:

```csharp
// Check if the level has an extreme optional
if (level.HasExtreme) { /* extreme only logic */ }

// Check if the level has an overload optional
if (level.HasOverload) { /* overload only logic */ }

// check if the level has both extreme and overload (will fail if it only has one or the other)
if (level.HasPrisonerEfficiency) { /* extreme + overload logic */ }

// Level's tier: A, B, C, D, E
level.Tier;

// Whether the current layout is for Main, Extreme, or Overload
director.Bulkhead
```

---

## Zone Limits and Timing

### Time Calculations

- **Target session time:** ~2 hours maximum
- **Average time per zone:** ~5 minutes (including combat, exploration, puzzles)
- **Maximum practical zone count:** ~25 zones across all objectives

### Zone Count Guidelines

| Scenario        | Main  | Extreme | Overload | Total |
| --------------- | ----- | ------- | -------- | ----- |
| Main only       | 15-20 | -       | -        | 15-20 |
| Main + Extreme  | 12-15 | 5-8     | -        | 17-23 |
| Main + Overload | 12-15 | -       | 4-6      | 16-21 |
| Full PE         | 8-12  | 4-6     | 3-5      | 15-23 |

### Hard Limits

- **Maximum per layout:** 20 zones (game limit)
- **Recommended Main with secondaries:** 15 zones
- **Recommended secondaries:** 5-10 zones each
- **Max total:** ~25 zones for PE runs

---

## Using Generator.SelectRun()

`Generator.SelectRun()` provides weighted random selection of layout variants, ensuring variety while controlling probability.

### Basic Pattern

```csharp
Generator.SelectRun(new List<(double, Action)>
{
    (0.40, () => {
        // 40% chance: Layout variant A
        var (end, _) = BuildChallenge_KeycardInSide(start, sideKeycardZones: 2);
    }),
    (0.35, () => {
        // 35% chance: Layout variant B
        var (end, _) = BuildChallenge_GeneratorCellInSide(start, sideCellZones: 1);
    }),
    (0.25, () => {
        // 25% chance: Layout variant C
        var (end, _) = BuildChallenge_ApexAlarm(start, WavePopulation.Baseline, WaveSettings.Apex_Normal);
    }),
});
```

### Best Practices

1. **Weights should sum to ~1.0** for clarity (though not required)
2. **More common/balanced layouts get higher weights**
3. **Challenging or unique layouts get lower weights**
4. **Each variant should be complete** - don't leave partial builds

### Combining with Tier/Bulkhead Switching

The most robust pattern combines tier/bulkhead switching with `SelectRun()`:

```csharp
switch (level.Tier, director.Bulkhead)
{
    case ("A", Bulkhead.Main):
        // A-tier Main: simple variants
        Generator.SelectRun(new List<(double, Action)>
        {
            (0.5, () => BuildSimpleLinear(start)),
            (0.5, () => BuildKeycardBranch(start)),
        });
        break;

    case ("C", Bulkhead.Main):
        // C-tier Main: more complex variants
        Generator.SelectRun(new List<(double, Action)>
        {
            (0.30, () => BuildGeneratorChallenge(start)),
            (0.30, () => BuildScanSequence(start)),
            (0.25, () => BuildErrorAlarmRun(start)),
            (0.15, () => BuildBossFight(start)),
        });
        break;

    case ("E", Bulkhead.Overload):
        // E-tier Overload: extreme variants
        Generator.SelectRun(new List<(double, Action)>
        {
            (0.35, () => BuildApexWithBoss(start)),
            (0.35, () => BuildDoubleError(start)),
            (0.30, () => BuildMegaMom(start)),
        });
        break;

    default:
        // Fallback for uncovered cases
        BuildDefaultLayout(start);
        break;
}
```

---

## Layout File Analysis and Status

### Well-Developed (Full Tier/Bulkhead Coverage)

These files demonstrate best practices with extensive `SelectRun()` usage and full tier/bulkhead coverage:

| File                                     | Lines | Notes                                    |
| ---------------------------------------- | ----- | ---------------------------------------- |
| `LevelLayout.CentralGeneratorCluster.cs` | ~1425 | Full A-E x Main/Extreme/Overload matrix  |
| `LevelLayout.HsuFindSample.cs`           | ~1010 | Extensive SelectRun with full coverage   |
| `LevelLayout.GatherSmallItems.cs`        | ~1106 | Full coverage with many layout variants  |
| `LevelLayout.CorruptedTerminalUplink.cs` | ~685  | Tier x terminal count variants           |
| `LevelLayout.HsuActivateSmall.cs`        | ~509  | A-E tiers with multiple challenge paths  |
| `LevelLayout.ForwardExtract.cs`          | ~468  | Tier x Bulkhead combinations             |
| `LevelLayout.ClearPath.cs`               | ~460  | A-E tiers with boss fights, error alarms |
| `LevelLayout.ReachKdsDeep.cs`            | ~731  | Tier differentiation with KDS modules    |

### Moderately Developed

These have some differentiation but could use more variants:

| File                             | Lines | Notes                                                 |
| -------------------------------- | ----- | ----------------------------------------------------- |
| `LevelLayout.Survival.cs`        | ~292  | Has tier differentiation, could add bulkhead variants |
| `LevelLayout.GatherTerminal.cs`  | ~237  | Basic tier switching                                  |
| `LevelLayout.ReactorStartup.cs`  | ~157  | Code count variants, limited tier variety             |
| `LevelLayout.ReactorShutdown.cs` | ~111  | Basic implementation                                  |

### Needs Updates (Priority)

These files need significant updates to match the well-developed patterns:

| File                                    | Lines | Issue                                        | Recommendation                                |
| --------------------------------------- | ----- | -------------------------------------------- | --------------------------------------------- |
| `LevelLayout.TerminalUplink.cs`         | ~70   | No tier differentiation, only terminal count | Add tier/bulkhead switch, SelectRun variants  |
| `LevelLayout.PowerCellDistribution.cs`  | ~163  | No tier/bulkhead differentiation             | Add full tier/bulkhead matrix                 |
| `LevelLayout.RetrieveBigItems.cs`       | ~237  | Minimal variety, no tier differentiation     | Add SelectRun, tier variants                  |
| `LevelLayout.TimedTerminalSequence.cs`  | ~147  | No tier/bulkhead switch                      | Add tier-based terminal counts and challenges |
| `LevelLayout.SpecialTerminalCommand.cs` | ~166  | Skeleton SelectRun with empty cases          | Implement actual layout variants              |
| `LevelLayout.Reactor.cs`                | ~72   | Just dispatches to Startup/Shutdown          | Base file, but could add common setup         |

---

## Implementation Checklist

When creating or updating a level layout:

- [ ] **Tier coverage** - Handle A, B, C, D, E tiers differently
- [ ] **Bulkhead coverage** - Handle Main, Extreme, Overload appropriately
- [ ] **SelectRun usage** - Provide multiple weighted variants per tier/bulkhead
- [ ] **Zone count check** - Respect zone limits (20 max, adjust for multi-objective)
- [ ] **Difficulty scaling** - Easier challenges in lower tiers, harder in higher
- [ ] **Resource distribution** - Add appropriate packs throughout
- [ ] **Default fallback** - Include a sensible default case

### Template for New Layouts

```csharp
public void BuildLayout_{ObjectiveName}(
    BuildDirector director,
    WardenObjective objective,
    ZoneNode? startish)
{
    if (startish == null)
    {
        Plugin.Logger.LogError($"No node returned when calling Planner.GetLastZone({director.Bulkhead})");
        throw new Exception("No zone node returned");
    }

    var start = (ZoneNode)startish;
    var startZone = planner.GetZone(start)!;

    switch (level.Tier, director.Bulkhead)
    {
        case ("A", Bulkhead.Main):
            // Easiest main variant
            Generator.SelectRun(new List<(double, Action)>
            {
                (0.5, () => { /* Variant A */ }),
                (0.5, () => { /* Variant B */ }),
            });
            break;

        case ("B", Bulkhead.Main):
            // B-tier main variants
            break;

        case ("C", Bulkhead.Main):
            // C-tier main variants
            break;

        case ("D", Bulkhead.Main):
            // D-tier main variants
            break;

        case ("E", Bulkhead.Main):
            // E-tier main variants
            break;

        case ("A", Bulkhead.Extreme):
            // A-tier extreme
            break;

        case ("B", Bulkhead.Extreme):
            // B-tier extreme
            break;

        case ("C", Bulkhead.Extreme):
            // C-tier extreme
            break;

        case ("D", Bulkhead.Extreme):
            // D-tier extreme
            break;

        case ("E", Bulkhead.Extreme):
            // E-tier extreme
            break;

        case ("A", Bulkhead.Overload):
            // A-tier extreme
            break;

        case ("B", Bulkhead.Overload):
            // B-tier extreme
            break;

        case ("C", Bulkhead.Overload):
            // C-tier extreme
            break;

        case ("D", Bulkhead.Overload):
            // D-tier extreme
            break;

        case ("E", Bulkhead.Overload):
            // E-tier extreme
            break;

        default:
            Plugin.Logger.LogWarning($"Unhandled tier/bulkhead: {level.Tier}/{director.Bulkhead}");
            // Fallback layout
            break;
    }
}
```

---

## Related Documentation

- `zone-layout-system.md` - Technical API reference for zone building
- `../game/objectives/*.md` - Individual objective documentation
