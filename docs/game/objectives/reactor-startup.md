# Reactor Startup Objective

## Overview

The **Reactor Startup** is a high-intensity wave-defense objective in GTFO that requires players to protect a reactor through its startup sequence. Players must defend against escalating enemy waves while inputting verification codes at critical moments. This objective demands strong combat skills, careful resource management, and precise timing to succeed.

## What is a Reactor?

Reactors are critical power facilities located deep within the Complex. In Reactor Startup missions, prisoners must bring these dormant reactors online by completing a multi-phase activation sequence. The startup process triggers defensive protocols that spawn waves of enemies, requiring the team to fight while managing time-sensitive verification procedures.

## Objective Flow

### Phase 1: Locate the Reactor

1. **Navigate to Reactor Location**: Some expeditions begin in darkness, requiring careful navigation toward the reactor zone
2. **Resource Collection**: Gather ammunition, health, and tools along the path
3. **Clear Initial Threats**: Eliminate sleepers blocking access to the reactor terminal
4. **Locate Terminal**: Find the dedicated reactor control terminal

### Phase 2: Reactor Startup Sequence

1. **Initiate Startup**: Use command `REACTOR_STARTUP` at the reactor terminal
2. **Multiple Wave Cycles**: The reactor goes through multiple startup phases (typically 4-10 waves depending on expedition)
3. **Each Cycle Consists Of**:
   - **Warmup Phase**: Brief preparation time before enemies spawn
   - **Wave Defense Phase**: High-intensity combat as enemies attack (15 seconds to 4 minutes)
   - **Verification Phase**: Limited time window to input verification code (25 seconds to 17+ minutes)

### Phase 3: Verification Code Entry

**Standard Variant (HUD Display):**

- Verification code appears directly on the HUD (e.g., "DART", "CURE", "CELL")
- Enter command: `REACTOR_VERIFY [CODE]` (e.g., `REACTOR_VERIFY DART`)
- Must be completed before verification timer expires

**Advanced Variant (Terminal Logs):**

- Codes must be retrieved from log files on terminals in adjacent zones
- Zones unlock sequentially as reactor progresses through phases
- Code-retrieval zones may contain:
  - Scout patrols
  - Alarmed security doors
  - High-threat enemies (Mothers, Tanks)
- Requires splitting team or rapid zone navigation

### Phase 4: Completion and Extraction

1. **Final Wave**: Complete the last verification to finish reactor startup
2. **Proceed to Extraction**: Navigate to extraction point
3. **Complete Mission**: Final extraction scan

## Core Mechanics

### Wave Defense

- **Escalating Difficulty**: Enemy waves become progressively more challenging
- **Enemy Types**: Strikers, Shooters, Giants, Chargers, Hybrids, and in advanced missions: Tanks, Shadows, Flyers
- **Timer-Based**: Each wave runs for a predetermined duration shown on HUD
- **Resource Intensive**: Extended combat depletes ammunition and consumables

### Verification System

- **Time Pressure**: Verification must occur within strict time limits
- **Failure Consequence**: Missing verification timer resets the current wave, wasting resources and time
- **Pre-typing Strategy**: Many players pre-type `REACTOR_VERIFY ` before receiving the code to save precious seconds
- **Tab Completion**: Use TAB key to auto-complete commands

### Code Retrieval Mechanics

**HUD Codes (Basic):**

- Codes displayed automatically
- Focus remains on wave defense
- Lower complexity, higher combat intensity

**Terminal Log Codes (Advanced):**

- Requires terminal navigation during or between waves
- Adds exploration and risk management layer
- May require team splitting or rapid solo runs
- Code zones unlock progressively through the sequence

## Recommended Strategy

### Preparation Phase

1. **Resource Scouting**: Before starting reactor, locate all nearby resource crates and tool refill stations
2. **Sentry Placement**: Position automated sentries at optimal defensive chokepoints
3. **Team Roles**: Assign roles:
   - **Defenders**: Hold position at reactor (2-3 players)
   - **Code Runner**: Retrieve terminal codes if needed (1 player)
   - **Flex Support**: Adapt based on needs
4. **C-Foam Doors**: Reinforce key doorways to slow enemy approach
5. **Ammo Conservation**: Top off ammunition before initiating startup

### During Waves

1. **Positioning**: Establish defensive position with good sightlines
2. **Focus Fire**: Coordinate target priority (Giants first, then Chargers)
3. **Manage Consumables**: Use mines and C-Foam strategically, not wastefully
4. **Communication**: Call out high-priority threats immediately
5. **Turret Maintenance**: Repair or reposition sentries between waves if possible

### Verification Phase

1. **Pre-type Command**: Have `REACTOR_VERIFY ` ready before code appears
2. **Clear Terminal Area**: Ensure terminal is accessible and safe
3. **Code Runner Speed**: If retrieving from terminals, move quickly but carefully
4. **Backup Plan**: Have second player ready to input code if primary is incapacitated
5. **Time Awareness**: Track verification timer constantly

### Advanced Variant (Terminal Codes)

1. **Scout Code Zones Early**: If possible, scout zones that will unlock during sequence
2. **Clear Paths**: Pre-clear obstacles and enemies from code-retrieval routes
3. **Terminal Locations**: Memorize which terminals contain logs before starting
4. **Solo Runner**: Designate fastest/most skilled player for code retrieval
5. **Defensive Hold**: Remaining team must hold reactor position during retrieval

## Terminal Commands

- `REACTOR_STARTUP` - Initiates the reactor startup sequence
- `REACTOR_VERIFY [CODE]` - Enters verification code (e.g., `REACTOR_VERIFY DART`)
- `PING` - Use in code-retrieval zones to locate specific terminals
- `LIST LOGS` - View available log files on a terminal (for code retrieval variant)

## Notable Expeditions

### Reactor Startup (Main Objective)

- **[ALT://R1C1](https://gtfo.fandom.com/wiki/ALT://R1C1)**: 8 waves, introductory difficulty
- **[ALT://R2D2](https://gtfo.wiki.gg/wiki/ALT://R2D2)**: 10 waves, extended sequence
- **[ALT://R3A3](https://gtfo.fandom.com/wiki/ALT://R3A3)**: Advanced enemy types
- **[ALT://R4D1](https://gtfo.fandom.com/wiki/ALT://R4D1)**: Contains 4-minute wave duration
- **[ALT://R5D2](https://gtfo.fandom.com/wiki/ALT://R5D2)**: Only 4 waves, shortest sequence
- **[ALT://R6D1](https://gtfo.fandom.com/wiki/ALT://R6D1)**: High-difficulty variant
- **[ALT://R6D3](https://gtfo.fandom.com/wiki/ALT://R6D3)**: Extreme difficulty
- **[R7E1](https://gtfo.fandom.com/wiki/R7E1)**: Rundown 7 E-tier mission

### Reactor Startup (Secondary/Overload)

- **[ALT://R4B1](https://gtfo.fandom.com/wiki/ALT://R4B1)**: Secondary objective variant
- **[ALT://R4E1](https://gtfo.fandom.com/wiki/ALT://R4E1)**: Overload objective, 25-second verification
- **[ALT://R5B1](https://gtfo.fandom.com/wiki/ALT://R5B1)**: Secondary variant
- **[ALT://R5C2](https://gtfo.wiki.gg/wiki/ALT://R5C2)**: Combined with other objectives
- **[R8E2](https://gtfo.fandom.com/wiki/R8E2)**: Features Nightmare enemies

## Comparison to Reactor Shutdown

| Aspect                    | Reactor Startup                      | Reactor Shutdown                     |
| ------------------------- | ------------------------------------ | ------------------------------------ |
| **Objective Type**        | Wave defense with verification codes | Bioscan defense with alarm           |
| **Initial State**         | Often begins in darkness             | Well-illuminated zones               |
| **Activation Command**    | `REACTOR_STARTUP`                    | `REACTOR_SHUTDOWN`                   |
| **Primary Mechanic**      | Timed waves + code verification      | Bioscan completion + sustained alarm |
| **Code Verification**     | Multiple times (4-10 cycles)         | Single verification to begin         |
| **Enemy Spawning**        | Wave-based, predetermined durations  | Continuous spawning until extraction |
| **Power State**           | Brings reactor online                | Severs power across expedition       |
| **Defense Phases**        | Discrete waves with breaks           | Continuous combat during bioscans    |
| **Complexity**            | Higher (multiple cycles, timing)     | Moderate (single bioscan sequence)   |
| **Number of Expeditions** | 13+ expeditions                      | 4 expeditions                        |

## Common Challenges

### Wave Overwhelming

- **Challenge**: Later waves spawn too many enemies, overwhelming defenses
- **Solution**: Prioritize high-threat targets, use chokepoints, maintain sentry uptime

### Missed Verification Timer

- **Challenge**: Failing to input code in time, causing wave reset
- **Solution**: Pre-type commands, designate backup verifier, clear terminal access path

### Code Retrieval Under Pressure

- **Challenge**: Code runner gets killed or trapped in advanced variant
- **Solution**: Scout zones beforehand, clear paths, have escape routes planned

### Resource Depletion

- **Challenge**: Running out of ammunition or consumables mid-sequence
- **Solution**: Locate resource crates before starting, conserve ammo, use melee when safe

### Team Coordination Breakdown

- **Challenge**: Poor communication leads to missed verifications or defensive failures
- **Solution**: Assign clear roles, call out threats and timers, maintain discipline

## Tips

- **Pre-type Everything**: Have `REACTOR_VERIFY ` ready before each verification phase
- **Count Your Waves**: Track which wave you're on to anticipate difficulty spikes
- **Sentry Angles**: Position sentries to cover multiple approach angles, not just one corridor
- **Save Big Tools**: Reserve mines and C-Foam for later, harder waves
- **Scan Terminal Logs Early**: If using advanced variant, check which terminals have logs before starting
- **Backup Ammo Awareness**: Know where nearest ammo refill is if you run dry
- **Don't Panic on Reset**: If wave resets due to missed verification, regroup and try again methodically
- **Watch the Clock**: Verification timers vary wildly (25 seconds to 17+ minutes) - check your specific timer
- **Enemy Priority**: Giants > Big Chargers > Shooters > Strikers for target priority
- **Communicate Constantly**: Call out verification codes verbally to ensure whole team knows them

## Sources

- [Reactor Startup - Official GTFO Wiki](https://gtfo.fandom.com/wiki/Reactor_Startup)
- [Reactor Shutdown - Official GTFO Wiki](https://gtfo.fandom.com/wiki/Reactor_Shutdown)
- [List of Startup Reactor Sequences - Official GTFO Wiki](https://gtfo.fandom.com/wiki/List_of_Startup_Reactor_Sequences)
- [Terminal - Official GTFO Wiki](https://gtfo.fandom.com/wiki/Terminal)
- [Terminal Command - Official GTFO Wiki](https://gtfo.wiki.gg/wiki/Terminal_Command)
- [Only Terminal Guide You'll Need - Steam Community](https://steamcommunity.com/sharedfiles/filedetails/?id=1934879867)
- [The ULTIMATE GTFO Guide - Steam Community](https://steamcommunity.com/sharedfiles/filedetails/?id=2359433203)
- [GTFO: Using Terminals (Tips & Tricks) - Screen Rant](https://screenrant.com/gtfo-using-terminals-tips-tricks/)
- [ALT://R2D2 - Official GTFO Wiki](https://gtfo.wiki.gg/wiki/ALT://R2D2)
