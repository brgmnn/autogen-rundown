# Reactor Shutdown Objective

## Overview

The **Reactor Shutdown** is a mission objective in GTFO that requires players to locate an active reactor and shut it down through a verification and bioscan sequence. Unlike Reactor Startup which involves multiple waves, Reactor Shutdown is more straightforward but triggers continuous enemy spawns that persist until extraction. This objective demands defensive positioning, bioscan coordination, and sustained combat capability.

## What is a Reactor?

Reactors are critical power facilities located deep within the Complex. In Reactor Shutdown missions, prisoners must take an active reactor offline by completing a verification and bioscan shutdown sequence. The shutdown process severs power across the expedition and triggers defensive protocols that spawn continuous enemy waves, requiring the team to fight their way back to extraction.

## Objective Flow

### Phase 1: Locate the Reactor

1. **Navigate to Reactor Location**: Travel through the Complex toward the reactor zone
2. **Resource Collection**: Gather ammunition, health, and tools along the path
3. **Clear Initial Threats**: Eliminate sleepers blocking access to the reactor terminal
4. **Locate Terminal**: Find the dedicated reactor control terminal

### Phase 2: Reactor Shutdown Sequence

1. **Initiate Shutdown**: Use command `REACTOR_SHUTDOWN` at the reactor terminal
2. **Enter Verification Code**:
   - A verification code appears on the HUD (e.g., "DART", "CURE", "CELL")
   - Enter command: `REACTOR_VERIFY [CODE]` (e.g., `REACTOR_VERIFY DART`)
   - This is a single verification (unlike Reactor Startup's multiple cycles)
3. **Complete Bioscans**:
   - After verification, a series of bioscan security sequences begin
   - Similar to alarmed security door mechanics
   - Players must stand within bioscan areas to progress
   - Number of bioscans varies by expedition

### Phase 3: Alarm and Defense

1. **Security System Malfunction**: Verification triggers a Reactor Security System Malfunction
2. **Continuous Enemy Spawns**:
   - Shadow sleepers commonly spawn continuously
   - Spawning may or may not stop after bioscans complete (expedition-dependent)
   - Enemies continue spawning until extraction in most cases
3. **Power Severed**: The shutdown severs power across the entire expedition
4. **Defend Position**: Hold defensive positions during bioscan sequences

### Phase 4: Extraction

1. **Navigate to Extraction**: Fight back to the expedition starting point
2. **Continuous Combat**: Maintain defensive formation while moving
3. **Final Extraction Scan**: Complete final bioscan at extraction point
4. **Survive**: Successfully extract to complete the mission

## Core Mechanics

### Verification System

- **Single Verification**: Unlike Reactor Startup, only one verification code is required
- **HUD Display**: Code appears directly on the HUD - no terminal log retrieval needed
- **No Time Pressure**: No strict verification timer (more forgiving than Reactor Startup)
- **Pre-typing Strategy**: Players can pre-type `REACTOR_VERIFY ` before the code appears

### Bioscan Security Sequence

- **Multiple Bioscans**: Series of bioscans similar to alarmed security doors
- **Team Requirement**: Multiple players typically required in scan zones
- **Progressive Completion**: Each bioscan must be completed to advance
- **Defense During Scans**: Team must defend scan areas from spawning enemies

### Alarm Mechanics

- **Error Alarm**: Cannot be deactivated once triggered
- **Shadow Sleepers**: Common enemy type during reactor shutdown alarms
- **Continuous Spawning**: Enemies spawn continuously until extraction
- **Spawn Persistence**: Completing bioscans may not stop enemy spawns (varies by expedition)

### Power State

- **Power Severed**: Shutdown severs power across the entire expedition
- **Environmental Impact**: May affect lighting and environmental systems
- **No Restart**: Once shut down, the reactor cannot be restarted

## Recommended Strategy

### Preparation Phase

1. **Clear the Path**: Before initiating shutdown, clear all enemies from reactor zone to extraction
2. **Resource Scouting**: Locate ammo and health stations along the route back
3. **Sentry Placement**: Position automated sentries at strategic chokepoints
4. **Team Roles**:
   - **Bioscan Team**: 2-3 players to complete scans
   - **Defenders**: Cover bioscan team and manage spawns
5. **C-Foam Preparation**: Identify doors to reinforce along extraction route
6. **Bio Tracker**: Essential for tagging Shadow sleepers so sentries can target them

### During Shutdown Sequence

1. **Verification Entry**: Have one player ready to input verification code immediately
2. **Bioscan Positioning**: Establish defensive positions around bioscan areas
3. **Focus Fire**: Prioritize Shadow sleepers and other high-threat targets
4. **Sentry Coverage**: Position two shotgun sentries outside the reactor room
5. **Bio Tracker Usage**: Tag Shadow sleepers so sentries can engage them
6. **Resource Management**: Conserve heavy ammunition for extraction phase

### Extraction Phase

1. **Organized Retreat**: Fall back to extraction in coordinated manner
2. **C-Foam Doors**: Reinforce doors behind you to slow enemy pursuit
3. **Sentry Repositioning**: Pick up and reposition sentries as needed
4. **Stick Together**: Maintain team cohesion during retreat
5. **Final Defense**: Establish strong defensive position at extraction point

## Terminal Commands

- `REACTOR_SHUTDOWN` - Initiates the reactor shutdown sequence
- `REACTOR_VERIFY [CODE]` - Enters verification code (e.g., `REACTOR_VERIFY DART`)
- `PING` - Locate reactor zone or other objectives
- `LIST RESOURCES` - Find ammo, health, and tool refill stations

## AutogenRundown Modded Variant

In the AutogenRundown mod, Reactor Shutdown can feature an advanced variant:

### Password-Locked Terminal

- **Locked Reactor Terminal**: The reactor control terminal may be password-protected
- **Code Retrieval**: Password must be fetched from a terminal in another zone
- **Added Complexity**: Similar to Reactor Startup's advanced terminal log variant
- **Zone Navigation**: Requires team to split or make an additional zone run before shutdown
- **Strategic Planning**: Must decide whether to retrieve password before or after preparing defenses

This variant increases difficulty and requires additional coordination, combining elements of exploration, terminal navigation, and combat preparation before the main shutdown sequence can begin.

## Notable Expeditions

Reactor Shutdown appears less frequently than Reactor Startup:

### Reactor Shutdown (Main Objective)
- **[ALT://R6D3](https://gtfo.fandom.com/wiki/ALT://R6D3)**: High-difficulty Reactor Shutdown mission
- See [Reactor Shutdown Wiki](https://gtfo.fandom.com/wiki/Reactor_Shutdown) for complete list

### Comparison Count
- **Reactor Startup**: 13+ expeditions
- **Reactor Shutdown**: 4 expeditions (less common)

## Comparison to Reactor Startup

| Aspect | Reactor Shutdown | Reactor Startup |
|--------|------------------|-----------------|
| **Objective Type** | Bioscan defense with alarm | Wave defense with verification codes |
| **Initial State** | Well-illuminated zones | Often begins in darkness |
| **Activation Command** | `REACTOR_SHUTDOWN` | `REACTOR_STARTUP` |
| **Primary Mechanic** | Bioscan completion + sustained alarm | Timed waves + code verification |
| **Code Verification** | Single verification to begin | Multiple times (4-10 cycles) |
| **Enemy Spawning** | Continuous spawning until extraction | Wave-based, predetermined durations |
| **Power State** | Severs power across expedition | Brings reactor online |
| **Defense Phases** | Continuous combat during bioscans | Discrete waves with breaks |
| **Complexity** | Moderate (single bioscan sequence) | Higher (multiple cycles, timing) |
| **Time Pressure** | Less strict verification timing | Strict wave and verification timers |
| **Number of Expeditions** | 4 expeditions | 13+ expeditions |

## Common Challenges

### Shadow Sleeper Spawns

- **Challenge**: Shadow sleepers teleport and are difficult to track without Bio Tracker
- **Solution**: Equip Bio Tracker, use shotgun sentries positioned to cover bioscan areas, tag enemies immediately

### Continuous Enemy Pressure

- **Challenge**: Unlike wave-based objectives, enemies spawn continuously without breaks
- **Solution**: Establish sustainable defensive positions, rotate defensive duties, manage stamina and resources

### Bioscan Vulnerability

- **Challenge**: Players are stationary targets during bioscan sequences
- **Solution**: Position defenders to cover bioscan zones, use C-Foam and mines to create safe zones

### Resource Depletion During Extraction

- **Challenge**: Long fighting retreat to extraction can deplete ammunition
- **Solution**: Scout ammo stations beforehand, conserve resources during bioscans, use melee when safe

### Team Coordination

- **Challenge**: Players get separated during extraction, leading to isolated deaths
- **Solution**: Assign rally points, maintain visual contact, use C-Foam to create safe fallback positions

## Tips

- **Bio Tracker is Essential**: Shadows won't be targeted by sentries without Bio Tracker tags
- **Two Shotgun Sentries**: Place them right outside the reactor room for maximum effectiveness
- **Pre-type Verification**: Have `REACTOR_VERIFY ` ready before code appears on HUD
- **Clear Before Starting**: Don't initiate shutdown until you've cleared the path to extraction
- **Sentry Angles**: Position sentries to cover bioscan areas and main enemy approach routes
- **Pair System**: Use buddy system during bioscans - two in scan, two defending
- **Save Heavy Weapons**: Reserve powerful ammunition for the extraction phase
- **C-Foam Retreat**: Reinforce doors as you fall back to slow enemy pursuit
- **One Player Wins**: Only one player needs to complete final extraction scan
- **Communication**: Call out Shadow teleports and high-priority threats immediately
- **Don't Rush**: Take time to prepare properly before initiating shutdown
- **Power Awareness**: Shutdown severs power - plan for reduced visibility

## Sources

- [Reactor Shutdown - Official GTFO Wiki](https://gtfo.fandom.com/wiki/Reactor_Shutdown)
- [Only Terminal Guide You'll Need - Steam Community](https://steamcommunity.com/sharedfiles/filedetails/?id=1934879867)
- [The ULTIMATE GTFO Guide - Steam Community](https://steamcommunity.com/sharedfiles/filedetails/?id=2359433203)
- [Terminal - Official GTFO Wiki](https://gtfo.fandom.com/wiki/Terminal)
- [ALT://R6D3 - Official GTFO Wiki](https://gtfo.fandom.com/wiki/ALT://R6D3)
