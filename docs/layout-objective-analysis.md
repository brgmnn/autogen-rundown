# LevelLayout & WardenObjective Analysis Report

Analysis of every objective type's LevelLayout and WardenObjective implementation, scored by sophistication, with rework prioritization.

## Scoring Methodology

Each objective is scored 1-5 based on:

| Criteria | Description |
|----------|-------------|
| **SelectRun usage** | Does the layout use `Generator.SelectRun` to pick from weighted layout variants? More variants = more replay value. |
| **Tier coverage** | Does it handle tiers A-E (and bulkhead variants) individually? Or does it use a single codepath for all? |
| **Challenge composition** | Does it use `BuildChallenge_*` helpers (keycards, generators, boss fights, error alarms, apex alarms, sensors)? |
| **Bulkhead awareness** | Does Main/Extreme/Overload get distinct treatment? |
| **Layout variety** | Linear-only vs hub+branches, forward extract candidates, garden tiles, geomorph variety. |
| **WardenObjective depth** | Tier-scaled parameters, atmospheric intel, event chains, lore integration. |

**Rating scale:**
- **5** - Fully reworked. Per-tier/bulkhead SelectRun with 3-6 weighted variants each. Rich challenge composition. Excellent replay value.
- **4** - Strong. Multiple SelectRun blocks, tier-aware, good variety. Minor gaps in coverage.
- **3** - Decent. Some SelectRun usage or tier variation, but incomplete coverage or limited variety.
- **2** - Basic. Has structure but mostly deterministic. Little to no SelectRun. Single codepath.
- **1** - Minimal. Effectively one layout every time. No SelectRun. No challenge composition.

---

## Per-Objective Breakdown

### 1. TerminalUplink

| Metric | Value |
|--------|-------|
| Layout file | `LevelLayout.TerminalUplink.cs` |
| Layout lines | 869 |
| Layout last modified | 2026-01-28 |
| Layout SelectRun count | 15 |
| Objective file | `WardenObjective.TerminalUplink.cs` |
| Objective lines | 539 |
| Objective last modified | 2026-01-15 |
| **Rating** | **5** |

**Layout:** The gold standard. Every tier (A-E) and every bulkhead (Main/Extreme/Overload) has its own `SelectRun` block with 3-6 weighted variants. Uses the full challenge toolkit: `BuildChallenge_KeycardInSide`, `BuildChallenge_GeneratorCellInSide`, `BuildChallenge_LockedTerminalDoor`, `BuildChallenge_BossFight`, `BuildChallenge_ApexAlarm`, `BuildChallenge_ErrorWithOff_*`, `AddSecuritySensors`. Conditional layouts based on terminal count. Hub-style layouts for multi-terminal runs.

**Objective:** Tier-scaled terminal count (1-4) and verification rounds (3-12), inversely balanced. Rich atmospheric intel messages.

**Deficiencies:** None significant. This is the benchmark.

---

### 2. CentralGeneratorCluster

| Metric | Value |
|--------|-------|
| Layout file | `LevelLayout.CentralGeneratorCluster.cs` |
| Layout lines | 1564 |
| Layout last modified | 2026-01-31 |
| Layout SelectRun count | 23 |
| Objective file | `WardenObjective.CentralGeneratorCluster.cs` |
| Objective lines | 327 |
| Objective last modified | 2026-01-11 |
| **Rating** | **5** |

**Layout:** Highest SelectRun count of any objective. Extensive tier/bulkhead coverage. Complex fog progression system with ascending/descending shapes. Multiple generator counts (2-5) scaled by tier. Power cell delivery + fog mechanic creates unique gameplay.

**Objective:** Fog step configuration, generator counts, tier-based complexity scaling. Uses custom `GeneralFogStep` system.

**Deficiencies:** None significant.

---

### 3. HsuFindSample

| Metric | Value |
|--------|-------|
| Layout file | `LevelLayout.HsuFindSample.cs` |
| Layout lines | 1216 |
| Layout last modified | 2026-01-31 |
| Layout SelectRun count | 13 |
| Objective file | `WardenObjective.HsuFindSample.cs` |
| Objective lines | 188 |
| Objective last modified | 2025-12-10 |
| **Rating** | **5** |

**Layout:** Comprehensive tier coverage with SelectRun per tier. Uses challenge helpers, has a `_Fast` variant for sub-objectives. Rich layout ideas documented in comments (error alarms, locked zones, class 10 alarm paths). Good variety of approaches per tier.

**Objective:** Simple but effective HSU zone placement. TODO note about adding resources for error alarm zones.

**Deficiencies:** Objective file hasn't been updated alongside layout rework.

---

### 4. GatherSmallItems

| Metric | Value |
|--------|-------|
| Layout file | `LevelLayout.GatherSmallItems.cs` |
| Layout lines | 1106 |
| Layout last modified | 2025-12-10 |
| Layout SelectRun count | 15 |
| Objective file | `WardenObjective.GatherSmallItems.cs` |
| Objective lines | 318 |
| Objective last modified | 2025-12-10 |
| **Rating** | **4** |

**Layout:** Strong SelectRun usage with 15 blocks. Handles multiple item types (IDs, decoders, drives, GLPs, data cubes). Has AlphaSix portal variant with MatterWaveProjector geomorph. Good variety.

**Objective:** Item distribution across zones with `find_items` tagging. 300+ context-appropriate search messages for atmosphere.

**Deficiencies:** Last modified Dec 10 - hasn't benefited from the Jan challenge toolkit improvements. Could use more tier/bulkhead-specific variants.

---

### 5. CorruptedTerminalUplink

| Metric | Value |
|--------|-------|
| Layout file | `LevelLayout.CorruptedTerminalUplink.cs` |
| Layout lines | 774 |
| Layout last modified | 2026-01-31 |
| Layout SelectRun count | 10 |
| Objective file | `WardenObjective.CorruptedTerminalUplink.cs` |
| Objective lines | 539 |
| Objective last modified | 2026-01-15 |
| **Rating** | **4** |

**Layout:** Good SelectRun coverage per tier. Uses challenge helpers. Handles terminal count variations. Has TODO to flesh out D/E tier more, especially E-tier with 3 terminals.

**Objective:** Corrupted variant where codes go to log files. 1-3 terminals, 2-5 rounds. Solid intel messages.

**Deficiencies:** TODO note acknowledges D/E tier coverage is thin. E-tier with 3 terminals not fully developed.

---

### 6. ClearPath

| Metric | Value |
|--------|-------|
| Layout file | `LevelLayout.ClearPath.cs` |
| Layout lines | 514 |
| Layout last modified | 2026-01-31 |
| Layout SelectRun count | 7 |
| Objective file | `WardenObjective.ClearPath.cs` |
| Objective lines | 312 |
| Objective last modified | 2025-12-10 |
| **Rating** | **4** |

**Layout:** Good per-tier SelectRun with 4-7 variants per tier. Uses full challenge toolkit including nested SelectRun for D/E tier boss fight preludes (MegaMom with generator/keycard/apex access variants). Exit zone configuration with enemy hoards. Main-only objective.

**Objective:** 300+ atmospheric warden intel messages. Simple reach-exit configuration.

**Deficiencies:** Objective file is older (Dec 10). No bulkhead variants (Main-only by design).

---

### 7. HsuActivateSmall

| Metric | Value |
|--------|-------|
| Layout file | `LevelLayout.HsuActivateSmall.cs` |
| Layout lines | 509 |
| Layout last modified | 2025-12-10 |
| Layout SelectRun count | 7 |
| Objective file | `WardenObjective.HsuActivateSmall.cs` |
| Objective lines | 166 |
| Objective last modified | 2025-12-10 |
| **Rating** | **4** |

**Layout:** Per-tier SelectRun with multiple variants. Uses challenge helpers (generator, keycard, boss, apex). Structured similarly to ClearPath with exit zone placement.

**Objective:** Bring item (DataSphere/Neonate) to machine. Simple but functional.

**Deficiencies:** Last modified Dec 10 - could benefit from Jan toolkit improvements. No bulkhead-specific variants.

---

### 8. GatherTerminal

| Metric | Value |
|--------|-------|
| Layout file | `LevelLayout.GatherTerminal.cs` |
| Layout lines | 430 |
| Layout last modified | 2026-01-28 |
| Layout SelectRun count | 5 |
| Objective file | `WardenObjective.GatherTerminal.cs` |
| Objective lines | 478 |
| Objective last modified | 2025-12-10 |
| **Rating** | **3** |

**Layout:** Has SelectRun but organized by tier+bulkhead combos with spawn counts (3-spawn, 4-spawn, 6-spawn). Hub+branches topology with dead-end geomorphs. Has `_Fast` variant. Security sensor variants for D-Overload and E-Overload. TODO for 6-spawn E-Main (commented out). Forward extract candidates.

**Objective:** 2-6 terminals based on tier. EXTRACT_DECRYPTION_KEY command. Cardinal number dialogue. Info log with terminal zone locations.

**Deficiencies:** E-Main 6-spawn not implemented (commented out). Many tier/bulkhead combos fall to default linear branch. Objective file hasn't been updated.

---

### 9. ReachKdsDeep

| Metric | Value |
|--------|-------|
| Layout file | `LevelLayout.ReachKdsDeep.cs` |
| Layout lines | 731 |
| Layout last modified | 2026-01-19 |
| Layout SelectRun count | 5 |
| Objective file | `WardenObjective.ReachKdsDeep.cs` |
| Objective lines | 429 |
| Objective last modified | 2025-12-10 |
| **Rating** | **3** |

**Layout:** Custom R8E1-style exit sequence with portal geomorphs, MatterWaveProjector, Garganta warning. Uses SelectRun for mid-level variants. Lore-heavy with custom terminal commands (ADMIN_TEMP_OVERRIDE). Unique mechanics: error alarm chase, reactor explosion, WinOnDeath timer.

**Objective:** ErrorAlarmChase subtype. Complex event chains. Checkpoint system. Custom success screen.

**Deficiencies:** Highly specialized - only works as a specific E-tier level. Not really comparable to other objectives. SelectRun usage is moderate.

---

### 10. Survival

| Metric | Value |
|--------|-------|
| Layout file | `LevelLayout.Survival.cs` |
| Layout lines | 292 |
| Layout last modified | 2026-01-19 |
| Layout SelectRun count | 2 |
| Objective file | `WardenObjective.Survival.cs` |
| Objective lines | 360 |
| Objective last modified | 2025-12-10 |
| **Rating** | **3** |

**Layout:** Has an E-tier SelectRun with boss error + puzzle variants (generator or keycard). Default tier uses basic `AddBranch` with resource boosts. Interesting security control side-zone with OVERRIDE_LOCKDOWN_PROTOCOL terminal command, event loops for flashing lights, and hybrid wave escalation. Exit zone with locked door.

**Objective:** Timed survival with force-open-doors shortcut. AWO event loops. TODO: limit bosses, maybe restrict error alarm to E-tier.

**Deficiencies:** Only E-tier has SelectRun; A-D all use the same default branch. `if (true)` guard on security control zone (TODO: don't always spawn). Limited layout variety for most tiers.

---

### 11. SpecialTerminalCommand

| Metric | Value |
|--------|-------|
| Layout file | `LevelLayout.SpecialTerminalCommand.cs` |
| Layout lines | 166 |
| Layout last modified | 2025-12-10 |
| Layout SelectRun count | 1 (empty!) |
| Objective file | `WardenObjective.SpecialTerminalCommand.cs` |
| Objective lines | 1238 |
| Objective last modified | 2025-12-10 |
| **Rating** | **2** |

**Layout:** Has one SelectRun block that is **completely empty** (A-tier case with empty list). KingOfTheHill path is the most developed with hill geomorph, spawn zones, and locked door events. ErrorAlarm sub-method exists but has TODO about adjusting. Normal path is barebones: `AddBranch` + keycard puzzle + forward extract. Has `_Fast` variant.

**Objective:** Actually complex (1238 lines). 4 command types with tier-weighted probabilities. KingOfTheHill has full AWO integration. Extensive TODO wishlist: spawn boss, fog mechanics, error alarm variants, unit waves, survival mega wave.

**Deficiencies:** Layout is the weak link - the objective file is sophisticated but layouts are almost all identical. Empty SelectRun block. Most of the heavy lifting is in the objective, not the layout. Overload case has empty block.

---

### 12. TimedTerminalSequence

| Metric | Value |
|--------|-------|
| Layout file | `LevelLayout.TimedTerminalSequence.cs` |
| Layout lines | 147 |
| Layout last modified | 2026-01-19 |
| Layout SelectRun count | 0 |
| Objective file | `WardenObjective.TimedTerminalSequence.cs` |
| Objective lines | 487 |
| Objective last modified | 2025-12-10 |
| **Rating** | **2** |

**Layout:** Single deterministic layout. Corridor entrance -> hub -> branches (one per terminal). No SelectRun at all. Has error alarm variant for D/E tiers with an alarm control turn-off zone. TODO: "don't always do triple error alarms".

**Objective:** More complex (487 lines). Tier-based terminal counts and time limits. Lore-heavy with atmospheric messages.

**Deficiencies:** Zero layout variety. Every TimedTerminalSequence level looks the same: corridor-hub-branches. No tier-specific layout variants. No challenge composition. The objective file does the heavy lifting but the physical layout never changes.

---

### 13. RetrieveBigItems

| Metric | Value |
|--------|-------|
| Layout file | `LevelLayout.RetrieveBigItems.cs` |
| Layout lines | 236 |
| Layout last modified | 2025-12-10 |
| Layout SelectRun count | 0 |
| Objective file | `WardenObjective.RetrieveBigItems.cs` |
| Objective lines | 720 |
| Objective last modified | 2025-12-10 |
| **Rating** | **2** |

**Layout:** No SelectRun. MatterWaveProjector gets a special path (corridor -> MWP geomorph with tier-scaled enemies), everything else gets a hub+branches loop. Has ~30 lines of commented-out code (old corridor/hub approach). TODO: "re-evaluate this objective". Entrance zone is a coin flip between hub and large zone.

**Objective:** 1-4 items based on tier (DataSphere, CargoCrate, CryoCase, MatterWaveProjector). Solid item type variety but simple placement.

**Deficiencies:** Zero SelectRun. Commented-out dead code. Same layout regardless of tier. No challenge composition (no keycards, generators, alarms). MatterWaveProjector variant has tier-scaled enemies but all other items get generic branch layout.

---

### 14. PowerCellDistribution

| Metric | Value |
|--------|-------|
| Layout file | `LevelLayout.PowerCellDistribution.cs` |
| Layout lines | 163 |
| Layout last modified | 2025-12-10 |
| Layout SelectRun count | 0 |
| Objective file | `WardenObjective.PowerCellDistribution.cs` |
| Objective lines | 417 |
| Objective last modified | 2025-12-10 |
| **Rating** | **2** |

**Layout:** No SelectRun. Single deterministic path: corridor entrance -> hub -> generator branches (loop-based). Handles 4-5 generators with a second hub, but the structure is always the same. Has `_Fast` variant for sub-objectives.

**Objective:** 1-5 power cells based on tier. Organized intel by themes (carrying, navigation, activation, coordination). Solid atmosphere.

**Deficiencies:** Zero layout variety. Every level: corridor-hub-branches. No challenge composition. No tier-specific layout differences. Same physical layout whether A-tier or E-tier.

---

### 15. ReactorStartup

| Metric | Value |
|--------|-------|
| Layout file | `LevelLayout.ReactorStartup.cs` |
| Layout lines | 156 |
| Layout last modified | 2025-12-10 |
| Layout SelectRun count | 0 |
| Objective file | `WardenObjective.ReactorStartup.cs` |
| Objective lines | 1713 |
| Objective last modified | 2025-12-10 |
| **Rating** | **2** |

**Layout:** No SelectRun. Two methods: `_Simple` (prelude zones -> reactor) and `_FetchCodes` (reactor -> fetch branches). Simple variant scales prelude count by tier. FetchCodes variant creates terminal branches per fetch wave with extra terminals for confusion, locked entrances, and event-driven door opening. Tier-aware branch sizing and open-door chances.

**Objective:** The largest objective file (1713 lines). Extremely complex wave configuration, difficulty progression, 3-10 waves with fetch/defense mixing. Very sophisticated objective configuration.

**Deficiencies:** Layout is the weak link. The objective file is incredibly complex but the physical layout is deterministic. No SelectRun means the zone structure is always linear prelude + reactor (simple) or reactor + fan-out branches (fetch). No challenge composition in the layout. The complexity is entirely in wave timing, not zone variety.

---

### 16. ReactorShutdown

| Metric | Value |
|--------|-------|
| Layout file | `LevelLayout.ReactorShutdown.cs` |
| Layout lines | 111 |
| Layout last modified | 2025-12-10 |
| Layout SelectRun count | 0 |
| Objective file | `WardenObjective.ReactorShutdown.cs` |
| Objective lines | 1038 |
| Objective last modified | 2025-12-10 |
| **Rating** | **1** |

**Layout:** The simplest layout in the project. No SelectRun. Single codepath: 0-2 prelude zones -> reactor zone. Coin flip for locked reactor with password terminal in a side branch (1-2 zones). Garden tile chance on password terminal zone. That's it. 111 lines, most of which is boilerplate.

**Objective:** Very complex (1038 lines). Multi-step shutdown with verification codes, custom alarm variants with large scan starts, tier-based puzzle selection, surprise boss spawns. Uses EOS LayoutDefinitions.

**Deficiencies:** Extreme mismatch between layout simplicity and objective complexity. The objective file does everything while the layout does almost nothing. No tier-specific layout variants. No challenge composition. No variety in zone structure.

---

## Summary Table

| # | Objective | Layout Lines | SelectRun | Obj Lines | Last Modified | Rating |
|---|-----------|-------------|-----------|-----------|---------------|--------|
| 1 | TerminalUplink | 869 | 15 | 539 | Jan 28 | **5** |
| 2 | CentralGeneratorCluster | 1564 | 23 | 327 | Jan 31 | **5** |
| 3 | HsuFindSample | 1216 | 13 | 188 | Jan 31 | **5** |
| 4 | GatherSmallItems | 1106 | 15 | 318 | Dec 10 | **4** |
| 5 | CorruptedTerminalUplink | 774 | 10 | 539 | Jan 31 | **4** |
| 6 | ClearPath | 514 | 7 | 312 | Jan 31 | **4** |
| 7 | HsuActivateSmall | 509 | 7 | 166 | Dec 10 | **4** |
| 8 | GatherTerminal | 430 | 5 | 478 | Jan 28 | **3** |
| 9 | ReachKdsDeep | 731 | 5 | 429 | Jan 19 | **3** |
| 10 | Survival | 292 | 2 | 360 | Jan 19 | **3** |
| 11 | SpecialTerminalCommand | 166 | 1 (empty) | 1238 | Dec 10 | **2** |
| 12 | TimedTerminalSequence | 147 | 0 | 487 | Jan 19 | **2** |
| 13 | RetrieveBigItems | 236 | 0 | 720 | Dec 10 | **2** |
| 14 | PowerCellDistribution | 163 | 0 | 417 | Dec 10 | **2** |
| 15 | ReactorStartup | 156 | 0 | 1713 | Dec 10 | **2** |
| 16 | ReactorShutdown | 111 | 0 | 1038 | Dec 10 | **1** |

---

## Rework Priority Ranking

### Priority 1: ReactorShutdown
- **Current state:** 111 lines, 0 SelectRun, rating 1. The simplest layout in the entire project.
- **Why rework first:** Massive mismatch - the objective file is 1038 lines of complex shutdown mechanics, but the physical layout is always the same: a straight line to a reactor with a maybe-locked door. Reactor levels are some of the most memorable in vanilla GTFO and deserve distinct layouts. The locked/unlocked reactor flip is the only variation.
- **Rework scope:** Add per-tier SelectRun with challenge composition. A-tier could be a simple forward path. C-D tier could use keycard/generator challenges before the reactor. E-tier could use error alarms or boss fights. Consider multi-room reactor complexes, branching approaches, or environmental hazards (fog, sensors) on the path to the reactor.

### Priority 2: ReactorStartup
- **Current state:** 156 lines, 0 SelectRun, rating 2. Two methods but both deterministic.
- **Why rework second:** Same reactor mismatch problem. The objective file (1713 lines!) is the most complex in the project, but layouts are either "prelude -> reactor" or "reactor -> branches". Pairs naturally with ReactorShutdown rework. Also the most complex objective type that players spend the most time in.
- **Rework scope:** Add SelectRun for prelude approaches (challenge composition before reaching the reactor). For fetch codes, vary the branch topology (hub-and-spoke vs. sequential vs. mixed). Consider tier-specific branch challenges.

### Priority 3: PowerCellDistribution
- **Current state:** 163 lines, 0 SelectRun, rating 2. Always corridor-hub-branches.
- **Why rework third:** High frequency objective. Every run has the same physical structure: corridor entrance, hub, branches with generators. No tier differentiation in layout. Good candidate for the TerminalUplink treatment with per-tier/bulkhead SelectRun.
- **Rework scope:** Add SelectRun per tier. Vary hub topology. Add challenge composition to reach generators (keycards for locked generator rooms, error alarms, boss fights in E-tier). Consider linear approaches for some variants instead of always hub-and-spoke.

### Priority 4: RetrieveBigItems
- **Current state:** 236 lines, 0 SelectRun, rating 2. Has commented-out code and TODO.
- **Why rework fourth:** The TODO says "re-evaluate this objective". Has dead code suggesting previous rework was abandoned. MatterWaveProjector variant is decent but the generic path is hub+branches with no variety.
- **Rework scope:** Clean up commented-out code. Add SelectRun per tier. Item type could influence layout (e.g., CryoCase through fog zones, DataSphere through sensor corridors). Add challenge composition.

### Priority 5: SpecialTerminalCommand
- **Current state:** 166 lines, 1 SelectRun (empty!), rating 2. KingOfTheHill path is good, everything else is generic.
- **Why rework fifth:** The objective file is massive (1238 lines) with 4 command types, but the layout is nearly identical for all. The empty SelectRun block for A-tier suggests rework was started but abandoned. Large TODO wishlist in the objective file.
- **Rework scope:** Fill in the empty SelectRun blocks. Add per-tier layouts. Different command types could drive different layouts (e.g., FillWithFog could use fog-themed zones, LightsOff could have dark zones with sensors). ErrorAlarm sub-method needs adjustment per its own TODO.

### Priority 6: TimedTerminalSequence
- **Current state:** 147 lines, 0 SelectRun, rating 2. Always corridor-hub-branches.
- **Why rework sixth:** Same problem as PowerCellDistribution - every run has the same hub+branches structure. The error alarm variant for D/E is nice but the base layout never varies. TODO: "don't always do triple error alarms".
- **Rework scope:** Add SelectRun per tier. Vary topology (some linear sequences, some hub-and-spoke). Add challenge composition to reach terminal zones. Consider tiered error alarm configurations instead of always triple.

### Priority 7: Survival (partial)
- **Current state:** 292 lines, 2 SelectRun, rating 3. E-tier is good, A-D all the same.
- **Why partially rework:** E-tier has good SelectRun with boss error + puzzle variants. A-D all use the same default branch. The security control zone always spawns (TODO note). Lower priority because the survival objective's variety comes more from wave configuration than zone layout.
- **Rework scope:** Add SelectRun for C-D tiers (currently only E has it). Make security control zone spawn probabilistic. Add sensor or challenge variants for C-D.

### Priority 8: GatherTerminal (partial)
- **Current state:** 430 lines, 5 SelectRun, rating 3. Good for covered combos, gaps elsewhere.
- **Why partially rework:** Several tier/bulkhead combos have good SelectRun (B-Main, C/D-Main, D/E-Overload). E-Main 6-spawn is commented out (TODO). Many combos fall to default linear branch.
- **Rework scope:** Implement E-Main 6-spawn. Fill in remaining tier/bulkhead gaps with per-combo SelectRun.

---

## Benchmark Examples: What "Good" Looks Like

### TerminalUplink - The Gold Standard

The TerminalUplink layout demonstrates the ideal pattern:

1. **Per tier+bulkhead `switch`**: Every combination (A/Main, A/Extreme, A/Overload, B/Main, ..., E/Overload) gets its own case.
2. **`SelectRun` per case**: Each case has 3-6 weighted variants with probabilities summing to ~1.0.
3. **Challenge composition**: Variants combine `BuildChallenge_*` helpers in different ways (keycard -> generator, error -> boss, apex -> keycard, etc.)
4. **Difficulty escalation**: A-tier has simple forward paths and single challenges. E-tier has brutal apex alarms, double bosses, and error+generator combos.
5. **Conditional layouts**: Some variants only appear when terminal count meets a threshold (e.g., hub layout only for 2+ terminals).
6. **Zone comments**: Each SelectRun entry documents zone count for that variant.

### ClearPath - Nested SelectRun

The ClearPath layout shows effective use of nested SelectRun for complex scenarios. D/E tier's MegaMom boss fight has an outer SelectRun for the overall layout and an inner SelectRun for how players approach the boss room (generator access, keycard access, or apex alarm). This creates combinatorial variety without code duplication.

### CentralGeneratorCluster - Domain-Specific Mechanics

The CGC layout shows how objective-specific mechanics (fog steps, generator counts, ascending/descending fog shapes) can be woven into the SelectRun system. The fog progression creates emergent gameplay that varies with each run even before considering layout topology.
