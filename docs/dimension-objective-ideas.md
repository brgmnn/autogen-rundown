# Dimension/Portal Objective Ideas

Ideas for level objectives that use the portal and dimension systems. Each concept extends an existing base game objective type and maps to engine capabilities already present in the game.

## Dimension System Summary

Key capabilities available for objective design:

- **20 alternate dimensions** (Dimension1-20) plus Reality and Arena
- **Two warp modes**: `DimensionWarpTeam` (permanent) and `DimensionFlashTeam` (temporary, auto-returns after `Duration` seconds)
- **Per-dimension configuration**: fog, lighting, atmosphere, enemy population, resource distribution, terminal placement
- **Static dimensions**: pre-built environments (desert, mining shaft, jungle, boss arenas) with unique atmosphere (sandstorms, clouds, sun)
- **Item restrictions**: `ForbidCarryItemWarps` (items stay behind), `LeaveDeployablesOnWarp` (sentries/mines persist in origin)
- **Portal mechanics**: physical objects requiring item insertion (MWP) + optional chained puzzle to activate
- **Portal events**: `EventsOnPortalWarp` can trigger spawns, fog changes, lighting, sounds on each use
- **`ClearDimension`**: kills all enemies in a dimension on warp (useful for arena resets)

Existing dimension usage: Pouncer Arena (boss fights), desert static dimensions (R7/R8 set pieces), `ReachKdsDeep` (portal + error alarm chase), `GatherTerminal_AlphaSix` (gather items in desert dimension).

---

## 1. Displaced Artifact Recovery

**Base objective**: GatherSmallItems | **Tier**: A/B/C | **Feasibility**: Very High

A gather-small-items objective where some items are scattered in Reality and others are in pocket dimensions accessed through portals off the main path. Each dimension is a brief side excursion with a unique environment and threat profile.

### Objective Flow

1. Players need to collect 4-6 items (Personnel IDs, GLP data logs, etc.)
2. Terminal queries reveal 3 items in Reality zones, 1-2 in "dimensionally displaced sectors"
3. Players clear Reality zones to find the normally-placed items
4. For displaced items, players find portal zones off the main path (behind keycards or blood doors)
5. Each portal leads to a small static dimension (desert camp, mining shaft)
6. The dimension contains the item plus a few sleeping enemies -- retrieve and survive
7. `DimensionFlashTeam` returns players after 30-45 seconds
8. `ClearDimension = true` on return ensures no lingering threats

### Dimension Usage

- 1-2 static dimensions using different prefabs for visual variety (e.g. `AlphaSix` desert + `AlphaThree_Shaft` mining)
- `DimensionFlashTeam` for brief timed excursions (30-45 seconds)
- `ForbidWaveSpawning = true` in dimensions -- pure exploration/stealth encounters
- `ForbidCarryItemWarps = false` -- items must come back with the players
- Each dimension has distinct atmosphere (sandstorm desert vs dark mining shaft)

### Why It's Interesting

Gentlest introduction to dimension mechanics -- suitable for A/B tiers as players' first portal encounter. Each dimension is brief and self-contained, like a dangerous side room. The environmental variety (stepping from dark underground tunnels into a windswept desert) is a memorable moment that costs nothing in complexity. Directly extends the existing `GatherTerminal_AlphaSix` pattern.

### Implementation Notes

Nearly mirrors existing `BuildLayout_GatherTerminal_AlphaSix` code which already creates portal zones, MWP pickup zones, and dimension configuration with terminal placement in a static dimension. Extend to place small items instead of (or in addition to) terminals. Multiple dimensions just require additional `DimensionDatas` entries.

---

## 2. Collapse Protocol (Escape to Portal)

**Base objective**: EscapeToPortal (enum 21, unimplemented) | **Tier**: B/C/D | **Feasibility**: High

The level starts with a standard-seeming objective, but completing it triggers a catastrophic event -- fog floods, lights fail, error alarm activates. The only escape is through a dimensional portal at the far end of the level. Players race through increasingly hostile zones to reach the portal before the Complex becomes uninhabitable.

### Objective Flow

1. Players complete a standard first-half objective (terminal command, generator power-up, or small gather)
2. Completing the objective triggers catastrophe: fog rises, lights flicker, error alarm activates
3. Warden intel: "DIMENSIONAL BREACH DETECTED -- REALITY DESTABILIZING -- REACH EMERGENCY PORTAL"
4. A countdown timer begins (AWO `Countdown` event)
5. Players navigate 4-6 zones toward the portal, fighting through progressively worse conditions
6. Fog gets thicker in each zone via `SetFogSettings` events with increasing density
7. Zone doors auto-unlock ahead of the players on a timer
8. Portal zone: insert MWP (found along the path), complete a team scan
9. Portal warps to a static outdoor dimension -- extraction scan completes the level

### Dimension Usage

- `DimensionWarpTeam` for the final permanent warp (one-way escape)
- Destination is a static outdoor dimension (desert or jungle) representing escape from the Complex
- `ClearDimension = true` on the warp event to clear any pursuing enemies
- `ForbidWaveSpawning = true` in destination (safety after escape)
- `ForceInstantWin` event triggers after a short bioscan in the destination dimension
- Destination has generous resources as a reward/buffer for the final scan

### Why It's Interesting

Flips the standard GTFO flow on its head. Instead of "go deep, extract back," players flee forward into the unknown. Progressive environmental degradation creates escalating horror. The portal at the end is genuine salvation -- warping from a collapsing underground nightmare into an open sky is an enormous emotional payoff. The `EscapeToPortal` enum value (21) already exists but is unimplemented, making this a natural candidate.

### Implementation Notes

Naturally extends ClearPath (navigate to exit) with event-driven environmental deterioration. Fog progression, door unlocking, and countdown systems all exist as event types. Portal mechanics are proven in `GatherTerminal_AlphaSix`. The static destination dimension is the simplest dimension configuration. Scales well across tiers (lower tiers get more time, fewer enemies; higher tiers get shorter countdown, more hostiles).

---

## 3. Cross-Dimensional Reactor Startup

**Base objective**: ReactorStartup | **Tier**: C/D | **Feasibility**: High

A reactor startup where verification codes must be retrieved from an alternate dimension. Players warp through a portal to find each code, then warp back and enter it at the reactor terminal before the verification window expires. The alternate dimension has different enemies, fog, and lighting, creating a jarring contrast with each crossing.

### Objective Flow

1. Players navigate to the reactor zone and initiate startup
2. When the first verification phase begins, the code is NOT on the HUD -- warden intel states the code is "dimensionally displaced"
3. Players backtrack to a portal zone (2-3 zones from reactor), insert MWP, complete bioscan
4. Portal warps team to Dimension1 -- a small static desert dimension with a terminal containing the verification code in a log file
5. Players read the code, wait for the `DimensionFlash` timer to return them (15-25 seconds)
6. Rush back to the reactor terminal and enter the code before the verification window closes
7. Each subsequent wave/verification cycle repeats, but the dimension becomes progressively more hostile (fog fills in, enemies spawn on warp)
8. Later rounds could use Dimension2 with a different environment

### Dimension Usage

- `DimensionFlashTeam` for temporary warps with timed return
- Dimension1 uses a static dimension with terminal placements for log files containing codes
- `EventsOnPortalWarp` spawns escalating enemy waves in the dimension on each visit
- `ForbidCarryItemWarps = true` prevents bringing resources from dimension
- `LeaveDeployablesOnWarp = true` so defenses at reactor persist while team warps

### Why It's Interesting

Transforms reactor startup from a pure holdout into a frantic relay race between dimensions. The time pressure of the verification window combined with travel time to/from the portal creates agonizing decisions: rush to the portal during the wave, or fight to safety first? The dimension becomes more dangerous each visit so earlier rounds feel manageable while later rounds are terrifying. Natural extension of the existing "codes from terminals in other zones" variant -- it just makes those zones literally in another dimension.

### Implementation Notes

Maps directly to existing ReactorStartup with code retrieval. The reactor already supports codes in log files on terminals. `DimensionFlashTeam` is a native event type. Main new work: building the portal zone into the reactor layout, configuring static dimensions with terminal placements, and timing the flash duration to match verification windows. Could reuse the existing `GatherTerminal_AlphaSix` pattern for the dimension setup.

---

## 4. Containment Breach Protocol

**Base objective**: PowerCellDistribution | **Tier**: C/D/E | **Feasibility**: Medium-High

A power cell distribution where completing each generator causes a dimensional breach. After each generator is powered, a `DimensionFlashTeam` event yanks the team into a hostile dimension for a brief combat encounter. The more generators powered, the longer and harder the breach encounters become. The final generator triggers a full boss fight before extraction.

### Objective Flow

1. Players distribute 3-4 power cells to generators across the level (standard layout)
2. Powering Generator 1 triggers: warden intel "DIMENSIONAL CONTAINMENT BREACH", flash to Dimension1
3. Dimension1 is a Pouncer Arena -- brief 20-second fight, then auto-return
4. Powering Generator 2 triggers flash to Dimension2 (desert arena) -- 25-second charger wave
5. Powering Generator 3 triggers flash to Dimension3 (mining shaft) -- 30-second fight against shadows + giant
6. After all generators powered, main security door unlocks
7. Final zone extraction door triggers one last warp: 45-second boss encounter in a boss arena dimension
8. Surviving the boss flash warps players back to Reality for extraction

### Dimension Usage

- Multiple dimensions (Dimension1-4) with different static prefabs for variety
- `DimensionFlashTeam` with escalating `Duration` values (20s, 25s, 30s, 45s)
- Each dimension has pre-spawned enemies via `StaticEnemySpawningInZone`
- `ForbidCarryItemWarps = true` -- no resources from arena
- `LeaveDeployablesOnWarp = true` -- sentries guard Reality while team is flashed
- `ForbidWaveSpawning = true` in all dimensions (controlled pre-spawned encounters)
- `ClearDimension = true` on return to reset each arena
- Boss arena uses existing `AlphaOne` (Kraken arena) or Pouncer Arena

### Why It's Interesting

Transforms the relatively calm power cell distribution into a gauntlet of dimensional combat encounters. Each generator becomes a decision point: "are we ready for what comes next?" Players must budget resources not just for the cell distribution but for 3-4 forced combat encounters they cannot avoid. The variety of arenas (different environments, enemies, durations) keeps each breach fresh. The final boss flash is a climactic capstone replacing the usual "extract through empty zones" anticlimax.

### Implementation Notes

Builds on existing PowerCellDistribution. Flash events attach to generator activation events via objective step progression. Static dimensions are simple to configure. Main complexity is registering 3-4 dimensions on a single level. The Pouncer Arena is already registered by default on all levels. Additional arenas use existing static dimension data (`AlphaOne`, `AlphaThree_Shaft`, etc.).

---

## 5. Cross-Frequency Terminal Uplink

**Base objective**: CorruptedTerminalUplink | **Tier**: C/D/E | **Feasibility**: Medium-High

A terminal uplink where the uplink terminal is in Reality but verification codes appear on terminals in an alternate dimension. The team must split: some stay in Reality to defend the uplink terminal, while others warp to the dimension to read codes and communicate them via voice chat. The dimension is stealth-only while Reality is combat.

### Objective Flow

1. Players find and initiate the terminal uplink in Reality
2. Verification codes do NOT display on HUD -- warden intel reveals they are "broadcasting on an alternate frequency"
3. A portal in an adjacent zone leads to Dimension1 (static dimension with terminal placements)
4. One or two players warp to Dimension1 to find the terminal with the code
5. Dimension1 has sleeping enemies, reduced lighting, and no wave spawning -- pure stealth
6. Meanwhile, enemies attack the uplink terminal in Reality -- remaining players defend
7. Dimension players read the code from a log file, call it out to teammates via voice
8. Reality players type the code at the uplink terminal
9. After each verification round, the `DimensionFlash` returns dimension players
10. Final rounds may have dimension players dealing with scouts or chargers that have awakened

### Dimension Usage

- `DimensionFlashTeam` with `Duration` matching verification windows
- Static dimension with terminal placements containing log files with codes
- `ForbidWaveSpawning = true` in dimension (stealth only)
- `LeaveDeployablesOnWarp = true` so Reality defenses persist
- Dimension has dark/horror lighting contrasting with Reality
- `ForbidCarryItemWarps = true` so dimension players cannot bring back resources

### Why It's Interesting

The most team-coordination-intensive dimension concept. Creates genuine information asymmetry -- dimension players have the code but cannot type it; Reality players have the terminal but not the code. Voice communication becomes the critical link. The stealth-in-dimension / combat-in-Reality split means each sub-team faces a completely different challenge simultaneously. A scout detection in the dimension while teammates are already fighting a wave in Reality is a nightmare scenario. Amplifies GTFO's core pillar of team coordination in a way no existing objective does.

### Implementation Notes

The corrupted terminal uplink system already supports codes from log files. The main extension is placing those log files on terminals in a static dimension instead of Reality zones. `DimensionFlashTeam` handles the temporary warp. Team splitting happens naturally since not all players need to enter the portal. Could leverage the `GatherTerminal_AlphaSix` pattern for dimension terminal placements.

---

## 6. Rift Defense

**Base objective**: Survival | **Tier**: D/E | **Feasibility**: Medium-High

A survival objective where periodic `DimensionFlashTeam` events forcibly teleport the entire team into a hostile dimension for 15-30 seconds. Each flash is a survival-within-survival: different enemies, no defenses, pure reaction. Between flashes, players defend their position in Reality against standard waves.

### Objective Flow

1. Players reach the survival zone and trigger the protocol
2. Standard survival timer begins (e.g. 180 seconds on C tier)
3. Every 40-60 seconds, a `DimensionFlashTeam` event fires with a warning klaxon (2-second delay)
4. Team is forcibly warped to a static arena dimension for 15-20 seconds
5. Arena has pre-spawned enemies (chargers, shadows) -- pure combat, no prep
6. After flash duration expires, team automatically returns to Reality
7. Reality enemies have continued spawning during the flash -- the position may be overrun
8. `LeaveDeployablesOnWarp = true` means sentries kept firing while team was gone
9. Each subsequent flash sends players to a progressively harder arena
10. Final flash is the longest and hardest -- tank or mother in the arena

### Dimension Usage

- `DimensionFlashTeam` with varying `Duration` values (15s, 20s, 25s escalating)
- Arena dimension or Pouncer Arena (already configured on all levels)
- `ForbidCarryItemWarps = true` -- no items travel to/from arena
- `LeaveDeployablesOnWarp = true` -- sentries guard Reality while team is flashed
- `ForbidWaveSpawning = true` in arena (enemies are pre-spawned)
- `ClearDimension = true` on return to reset arena for next flash
- Different arena configurations per flash (using Dimension1, Dimension2, Dimension3)

### Why It's Interesting

Creates a unique "dual survival" experience. Players must maintain their defensive position in Reality while handling periodic forced combat encounters in a completely different environment. Being ripped from a carefully fortified position into a bare arena is genuinely terrifying. The resource management dilemma is real: over-invest in sentries and you have less personal ammo for the arena; under-invest and Reality is overrun when you return. Amplifies the survival objective's resource management by splitting it across two simultaneous theaters.

### Implementation Notes

Survival objective mechanics exist. `DimensionFlashTeam` is a native event type. Pouncer Arena is already configured and available. Main new work: adding timed flash events to the survival event sequence. Enemy pre-spawning in static dimensions uses `StaticEnemySpawningInZone`. Escalation comes from using multiple dimension indexes with different configurations.

---

## 7. Frequency Oscillation

**Base objective**: TimedTerminalSequence | **Tier**: D/E | **Feasibility**: Medium

A timed terminal sequence where terminals alternate between Reality and an alternate dimension. Each round, the active terminal exists in a different dimension -- players must warp back and forth to reach the correct terminal before the timer expires. The warp itself costs seconds, making route planning critical.

### Objective Flow

1. Players locate 3 terminals: 2 in Reality, 1 in Dimension1
2. Warden intel reveals the "oscillating frequency pattern" -- terminals activate in alternating dimensions
3. Round 1: Terminal in Reality Zone A activates -- reach it and enter command
4. Round 2: Terminal in Dimension1 activates -- find portal, warp, navigate to terminal
5. Round 3: Terminal back in Reality Zone B activates -- warp back via `DimensionFlash` return or second portal
6. Each round has a time limit; failing spawns a penalty wave and resets the round
7. Warp transit time (portal insertion + scan + teleport delay) eats into available time
8. Between rounds, enemies spawn to drain resources and create time pressure

### Dimension Usage

- Mixed `DimensionWarpTeam` and `DimensionFlashTeam` depending on routing
- Dimension1 is a small procedural or static dimension with 1-2 zones
- `ForbidTerminalsInDimension = false` required for the dimension terminal
- Dimension has different enemy composition (shadows/chargers for speed-based threat)
- `LeaveDeployablesOnWarp = true` so sentries guard terminals in both dimensions
- Portal zones in both Reality and Dimension1 for bidirectional travel

### Why It's Interesting

Transforms timed terminal sequence from a spatial routing puzzle within one space into a cross-dimensional race. The warp transit time creates a fascinating time budget problem -- pre-position at the portal before the round starts, or gather resources? Alternating dimensions means you can never fully set up in one location. Each warp leaves fortifications behind in the other dimension.

### Implementation Notes

Extends existing TimedTerminalSequence. Main challenge is placing terminals in Dimension1 -- `StaticTerminalPlacements` on DimensionData supports this for static dimensions. Timing system already exists. Warp events can be triggered by zone events or portal interaction. Complexity comes from ensuring terminal command targets work across dimensions.

---

## 8. Breach and Retrieve

**Base objective**: RetrieveBigItems / PowerCellDistribution | **Tier**: C/D | **Feasibility**: Medium

Critical items (power cells, fog turbines, neonate HSUs) exist only in an alternate dimension. Players must warp through a portal, collect items, and bring them back to Reality where they are needed for generators or HSU activation points. The catch: items cannot return through the same portal (Portal A has `ForbidCarryItemWarps`). Players must find a second portal (Portal B) within the dimension that allows items through.

### Objective Flow

1. Players find generators/turbine slots in Reality that need power cells or items
2. Terminals reveal required items were "displaced during a dimensional breach"
3. Players locate Portal A in Reality, insert MWP, complete chained puzzle, warp to Dimension1
4. Dimension1 is a procedurally generated mini-complex (3-4 zones) with required items scattered throughout
5. Players collect items, but Portal A's return point has `ForbidCarryItemWarps = true`
6. They navigate through Dimension1 to find Portal B, which allows items through
7. Portal B requires clearing a blood door or completing a scan in the dimension
8. Portal B warps back to Reality near the delivery zone
9. Repeat for additional items if needed

### Dimension Usage

- Dimension1 is procedurally generated (`IsStaticDimension = false`) with its own `LevelLayoutData`
- Dimension1 has its own fog, enemy population, and resource distribution (scarce ammo)
- Two distinct warp events: Portal A (outbound, no items) and Portal B (return, items allowed)
- `ForbidCarryItemWarps` selectively applied to create the routing puzzle
- Different fog/lighting to sell the "other dimension" feeling

### Why It's Interesting

Inverts the normal resource flow. Instead of finding items along your path forward, you must make a dedicated trip to another dimension. The two-portal routing creates a mini navigation puzzle within the dimension -- you cannot simply grab and go. Resource scarcity in the dimension means the supply trip itself costs resources. Amplifies GTFO's core pillar of resource management. The procedural generation of the dimension means it feels like a real place, not just an arena.

### Implementation Notes

Builds on existing PowerCellDistribution or RetrieveBigItems. The dimension zone layout requires a second `LevelLayoutData` entry. Two-portal mechanic requires two `DimensionWarpTeam` events pointed at different zones. Main complexity: ensuring the zone planner can build zones in both Reality and Dimension1 coherently. Most infrastructure-heavy of the concepts.

---

## Summary

| #   | Name                        | Base Objective          | Dims Used           | Warp Type            | Tier  | Feasibility |
| --- | --------------------------- | ----------------------- | ------------------- | -------------------- | ----- | ----------- |
| 1   | Displaced Artifact Recovery | GatherSmallItems        | 1-2 static          | Flash (timed)        | A/B/C | Very High   |
| 2   | Collapse Protocol           | EscapeToPortal          | 1 static            | Warp (one-way)       | B/C/D | High        |
| 3   | Cross-Dim Reactor Startup   | ReactorStartup          | 1-2 static          | Flash (timed)        | C/D   | High        |
| 4   | Containment Breach Protocol | PowerCellDistribution   | 3-4 static arenas   | Flash (escalating)   | C/D/E | Medium-High |
| 5   | Cross-Frequency Uplink      | CorruptedTerminalUplink | 1 static            | Flash (timed)        | C/D/E | Medium-High |
| 6   | Rift Defense                | Survival                | 1-3 static arenas   | Flash (forced)       | D/E   | Medium-High |
| 7   | Frequency Oscillation       | TimedTerminalSequence   | 1 static/procedural | Warp + Flash         | D/E   | Medium      |
| 8   | Breach and Retrieve         | PowerCell/RetrieveBig   | 1 procedural        | Warp (bidirectional) | C/D   | Medium      |

## Implementation Priority

Ordered by feasibility and reuse of existing patterns:

1. **Displaced Artifact Recovery** -- Nearly free, extends existing `GatherTerminal_AlphaSix` pattern directly
2. **Collapse Protocol** -- `EscapeToPortal` enum (21) already registered; high narrative impact for low implementation cost
3. **Cross-Dimensional Reactor Startup** -- Natural extension of reactor codes-from-terminals variant
4. **Containment Breach Protocol** -- Straightforward flash events on generator power-up; Pouncer Arena already available
5. **Cross-Frequency Terminal Uplink** -- Unique team-split gameplay; reuses corrupted uplink log mechanic
6. **Rift Defense** -- Pouncer Arena already available; strong horror potential
7. **Frequency Oscillation** -- Most complex routing; implement after simpler dimension objectives are proven
8. **Breach and Retrieve** -- Procedural dimension zones require the most new infrastructure (second `LevelLayoutData`)
