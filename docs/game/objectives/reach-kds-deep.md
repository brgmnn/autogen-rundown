# Reach KDS Deep Objective

## Overview

**Reach KDS Deep** is a custom AutogenRundown objective that recreates the iconic R8E1 "Valiant" mission from GTFO. Players must fight through the Complex to reach the KDS Deep facility, but arrive too late—the reactor has already been sabotaged and exploded ahead of them, plunging critical zones into darkness and destroying the extraction point. The mission culminates in a desperate survival encounter in the devastated HSU storage crater, with victory determined by a custom "Resources Expended" win screen rather than traditional extraction.

**In lore, this is a fatal betrayal**: A duplicate team has teleported out and sabotaged the reactor, abandoning the player team to die in the Complex. While the game counts it as a victory if the team survives long enough, there is no escape—the players are eventually overrun and killed. The "Resources Expended" refers to the player team being sacrificed. This objective features dramatic environmental events, tier-scaled difficulty, and tragic narrative stakes unique among GTFO missions.

## What is Reach KDS Deep?

Reach KDS Deep represents a desperate fight to reach the deepest section of the Kovac Defense Services (KDS) facility in the Mining complex, located at the Chicxulub crater in Mexico's Yucatan Peninsula. This is where the ancient alien spacecraft "The Inner" rests, and where Garganta—the alien life force harboring NAM-V virus—resides in stored HSU units.

The objective simulates a catastrophic facility failure where **players arrive too late**. The KDS Deep reactor has been sabotaged by a duplicate team that teleported out, leaving the player team to die. As players approach the final zones, they witness the reactor explosion firsthand—a scripted sequence that plunges the corridors they're traversing into complete darkness before emergency auxiliary power activates. Upon reaching KDS Deep, they find the facility destroyed, running on emergency power, with extraction deliberately sabotaged.

The mission culminates in a final survival encounter in the devastated HSU storage crater. Unlike standard objectives, there is **no elevator extraction**—the duplicate team has destroyed it. The game counts it as a "victory" if players survive a tier-scaled timer (30-90 seconds), displaying the custom **"Resources Expended"** success screen. However, **in lore this is not a victory**—it's a fatal backstab. The duplicate team has escaped via matter wave projector, blowing the reactor to cover their tracks and abandoning the player team. The timer measures how long the players survive their last stand before inevitable death. "Resources Expended" refers to the player team—they are the resources that were sacrificed.

This is an **AutogenRundown-exclusive custom objective** that does not appear in vanilla GTFO, though it faithfully recreates the structure, atmosphere, and tragic narrative of the base game's R8E1 "Valiant" expedition.

## Objective Flow

### Phase 1: Start Challenge (Corridor Approach)

1. **Deploy Without Starting Area**: Team spawns directly into the mission with no safe starting zone
2. **Navigate Entry Corridors**: Traverse 3 corridor zones with no enemies or blood doors
   - Minimal resources (low ammo distribution: 1.0)
   - No health packs or tool refills in corridors
3. **Reach Central Hub**: Arrive at a dig site hub zone with multiple connections
   - Heavy resources available (health 1.0, tools 1.0, ammo 3.0)
   - Random puzzle type determines progression:
     - **Keycard Puzzle**: Locate keycard to unlock next zone
     - **Generator Puzzle**: Power up generators to open security door
     - **Terminal Unlock**: Use terminal to deactivate security
4. **Clear Side Branches**: Navigate 2 additional corridor zones beyond hub
   - Moderate resources (health 1.0, ammo 1.0)
   - Enemy encounters begin spawning

**Enemy Waves During Start Challenge**:
- **40% traverse time**: Shadow wave spawns (20 points)
- **25% traverse time**: Single Pouncer mini-boss appears
- **85% traverse time**: Giant Shooters spawn (6-12 points, tier-dependent)
- **90% traverse time** (D/E tier only): Pouncer Shadow mini-boss

### Phase 2: Mid Challenge (Two Alternate Paths)

Players encounter one of two randomly selected path types:

#### Path A: Terminal Unlock (Security Zone)

1. **Reach Security Hub**: Arrive at dig site hub with 3 connections
   - Heavy resources (health 4.0, tools 4.0, ammo 8.0)
2. **Locate Security Terminal**: Find dead-end security zone containing terminal
3. **Execute Defense Grid Command**: Enter `KDS-DEEP_DEACTIVATE_DEFENSE` at terminal
   - Terminal authenticates with BIOCOM
   - Confirms terminal ID and operative credentials
   - Deactivates KDS Deep defense grid
   - Unlocks admin-locked progression door (11 second delay)
4. **Objective Update**: Message changes to "Proceed to KDS Deep"
5. **Clear Spawn Wave**: Defeat Shadow wave that spawns 25 seconds after command execution
6. **Wait for Time Lock**: Security zone door unlocks after countdown based on previous clear times

#### Path B: Holdout (HSU Staging Hub)

1. **Reach Staging Hub**: Arrive at storage hub with 3 connections
   - Heavy resources (health 4.0, tools 4.0, ammo 8.0)
2. **Encounter Time-Locked Door**: Next zone has tier-based security scan alarm
   - **C-tier**: StealthScan2 (Class 2 scan)
   - **D-tier**: StealthScan3 (Class 3 scan)
   - **E-tier**: StealthScan4 (Class 4 scan)
3. **Defend During Lockdown**: Survive timed countdown while enemies spawn
   - **C-tier**: 45 second countdown
   - **D-tier**: 60 second countdown
   - **E-tier**: 90 second countdown
4. **Boss Encounter**: Tank or Mother spawns 15-30 seconds into scan (Mother for E-tier)
5. **Initial Spawn Wave**: Mixed baseline enemies spawn immediately (40% standard, 30% chargers, 30% nightmare)

### Phase 3: End Challenge (Final Approach)

1. **Navigate Final Corridors**: Clear 2 corridor zones before KDS Deep
   - Minimal resources (ammo 2.0 only)
   - Enemies spawn on door open:
     - Tank Pouncer (20 second delay)
     - Giant Shooters 6-8 points (120 second delay)
     - Pouncer Shadow (180 second delay)
2. **Reach Final Hub**: Arrive at general hub with 3 connections
   - Very heavy resources (health 4.0, tools 8.0, ammo 8.0)
   - Random puzzle type (Keycard, Generator, or Terminal)
3. **Stock Up**: This is the last resource opportunity before the finale

### Phase 4: The Reactor Explosion

This phase features the dramatic reactor explosion that has already occurred in KDS Deep, with players experiencing its effects as they traverse the final approach corridors.

1. **Enter First Approach Corridor** (Corridor 1 - Snatcher Scan Zone):
   - Red-to-Yellow lighting indicating danger ahead
   - Small coverage area with minimal enemies
   - Players are unaware the reactor has already failed

2. **Enter Second Approach Corridor** (Corridor 2 - Explosion Trigger Zone):
   - Red-to-White dramatic lighting
   - Apex-level security gate
   - Zone aliased as "KDS Deep, ZONE"
   - Tier-based enemy spawns:
     - **D-tier**: Charger Giant (8 points)
     - **E-tier**: Nightmare Giant (8 points)
     - **Others**: Charger (6 points)
   - Complete surge alarm or team scan to unlock progression

3. **Reactor Explosion Sequence** (triggered on scan completion):

Players witness the reactor explosion **in the zones they're currently occupying**. The explosion affects Corridors 1 and 2—the two zones players are in or have just traversed.

| Event | Delay | Description |
|-------|-------|-------------|
| Machinery Blow | 1-4 seconds | Distant reactor malfunction sound from KDS Deep ahead |
| Screen Shake | +17 seconds | Violent 1-second screen shake (amplitude 5.0, 90Hz frequency) |
| Lights Out (Corridor 1) | +17.5 seconds | Zone players traversed plunges into darkness |
| Lights Out (Corridor 2) | +17.7 seconds | Zone players occupy plunges into darkness |
| Power Failure Sound | +18 seconds | System-wide power-down failure alarm |
| Auxiliary Power (Both) | +21.5 seconds | Emergency auxiliary lights activate in both corridors |
| Lights On Sound | +21 seconds | Loud auxiliary power engagement |

4. **Experience the Explosion**:
   - Players are plunged into **complete darkness** for ~4 seconds
   - Screen shakes violently indicating catastrophic failure
   - All zones lose main power simultaneously
   - Emergency auxiliary power restores dim red lighting

5. **Navigate Through the Aftermath**:
   - Continue forward under emergency lighting
   - Realize the reactor ahead has failed
   - Proceed to KDS Deep exit zone to find it destroyed

### Phase 5: KDS Deep Survival

Players arrive at KDS Deep to find the facility **already destroyed**, sabotaged by the duplicate team that has abandoned them. The reactor has been blown, extraction is impossible, and no rescue is coming. This is the player team's last stand before inevitable death.

1. **Enter Destroyed Exit Zone**: Reach custom R8E1 HSU exit geomorph
   - Crater-like destroyed area—the duplicate team has blown the reactor
   - Emergency reactor lighting (blue-to-red, auxiliary power only)
   - Zone aliased as "KDS Deep, ZONE" / "KDS"
   - Dramatic tension music begins playing
   - **No elevator—the duplicate team has destroyed extraction**
   - Players are abandoned, left behind to die

2. **Activate Exit Scan**: Complete team scan at entry point
   - This initiates the final survival encounter
   - **WinOnDeath** timer begins counting (Event Type 26)
   - No rescue is coming—this measures the last stand

3. **"SURVIVE" Message** (6 seconds after scan):
   - Objective updates to survival mode
   - Game win condition: Survive the tier-based timer duration
   - Lore reality: Hold out as long as possible before being overrun

4. **Survive Enemy Waves**:
   - **Critical**: Game win condition is surviving the timer, NOT killing all enemies
   - In lore: Fight to the end, knowing death is inevitable
   - Players don't need to clear all spawns, just stay alive as long as they can

**Survival Duration by Tier**:
- **A-tier**: 30 seconds
- **B-tier**: 40 seconds
- **C-tier**: 50 seconds
- **D-tier**: 70 seconds
- **E-tier**: 90 seconds

**Enemy Wave Sequence**:
- **8 seconds**: Error_VeryHard baseline population wave
- **D-tier 25% timer**: Pouncer Shadow mini-boss
- **D-tier 66% timer**: Infected Hybrids wave
- **E-tier 20% timer**: Pouncer mini-boss
- **E-tier 55% timer**: Chargers wave
- **E-tier 85% timer**: Giant Shooters wave
- **Survival end + 4s**: Final boss wave (Tank for most tiers, Mother for E-tier)

5. **Timer Expires → Instant Win** (game mechanics):
   - When the survival timer hits zero, **mission immediately succeeds**
   - Players do NOT actually die in-game—the WinOnDeath name is misleading
   - Mission transitions directly to victory screen
   - **However, in lore**: The players are eventually overrun and killed after this point

6. **"WARDEN SECURITY SYSTEMS DISABLED"** (survival duration + 2.5 seconds):
   - Game victory message displays
   - In lore: The duplicate team has successfully escaped, systems are failing

7. **Custom Victory Screen** (the dark irony):
   - **"Resources Expended"** success screen displays (R8E1 Valiant-style)
   - Game counts it as a mission success
   - **Lore reality**: This is a fatal betrayal, not a victory
   - The player team is the "resource" that was "expended"
   - No extraction, no escape—abandoned to die in KDS Deep
   - Timer only measured how long they survived before inevitable death

### Phase 6: Mission Complete

This mission uses a **custom win condition** unique among GTFO objectives, with a dark narrative twist:

**Win Condition**: `GoToExitGeo` (NOT `GoToElevator`)
- Players must reach the exit geomorph tile (KDS Deep destroyed zone)
- **No elevator extraction**—the duplicate team has sabotaged it
- Survival timer completion triggers instant mission success

**Custom Success Screen**: **"Resources Expended"**
- Same dramatic ending as vanilla R8E1 "Valiant"
- UI displays: `"CM_PageExpeditionSuccess_Resources expended_CellUI 2"`
- **The dark irony**: "Resources Expended" refers to the player team
- They are the resources that were sacrificed to allow the duplicate team's escape

**WinOnDeath Mechanic** (Event Type 26):
```csharp
.AddMessage("SURVIVE", 6.0)
.AddWinOnDeath(surviveDuration)  // Instant win when timer expires
.AddMessage("WARDEN SECURITY SYSTEMS DISABLED", surviveDuration + 2.5)
```

**What Actually Happens (Game Mechanics)**:
1. Survival timer expires (30-90 seconds based on tier)
2. Mission **instantly succeeds**—no need to kill all enemies
3. "WARDEN SECURITY SYSTEMS DISABLED" message confirms victory
4. Custom "Resources Expended" win screen displays
5. **Players do not actually die in-game**—the name "WinOnDeath" is misleading
   - It means "instant win when this timer hits zero"
   - Not "win only if all players die"

**What Actually Happens (Lore Reality)**:
1. **Duplicate team has teleported out** via matter wave projector
2. **They sabotaged the reactor** to cover their escape
3. **Player team is abandoned** with no extraction possible
4. Timer measures how long players survive before being overrun
5. **Players are eventually killed**—there is no rescue
6. **"Victory" is a fatal backstab**, not a real success
7. The duplicate team succeeds; the player team dies
8. Warden counts it as "mission success" only because objectives were completed

**Mission Narrative**:
The prisoners have reached KDS Deep too late—not because of bad timing, but because **they were never meant to make it out**. A duplicate team has used them as a distraction, teleporting to safety and blowing the reactor to eliminate witnesses. The player team is left behind to die in the Complex. The game counts it as a "victory" if they survive long enough for the duplicate team to escape, but in lore, this is a pyrrhic victory at best—a fatal betrayal at worst. "Resources Expended" is grimly literal: the player team is the resource that was expended.

## Core Mechanics

### Reactor Explosion Sequence

The reactor explosion is a carefully choreographed environmental event that players **experience firsthand** as they traverse the final approach to KDS Deep.

**Narrative Context**:
- The KDS Deep reactor has **already failed** while players fought through the Complex
- Players witness the explosion's effects in real-time as they reach the final corridors
- Explosion affects the zones players are **currently occupying**, not zones behind them
- This explains why KDS Deep is destroyed when they arrive

**Trigger**: Completing the door scan in Corridor 2 (the penultimate zone)

**Sequence Timeline**:
```
Initial Delay: Random 1-4 seconds
Explosion Delay: Initial + 17 seconds
Auxiliary Delay: Explosion + 4 seconds
```

**Environmental Effects**:
1. **MachineryBlow Sound** (ID: 1007447703): Distant reactor malfunction from KDS Deep ahead
2. **Screen Shake**: Violent 1-second shake at 5.0 amplitude, 90Hz frequency, 10.0 radius
3. **Progressive Lights Out**: Corridors 1 and 2 (zones players occupy) go completely dark in sequence (0.1s duration each)
4. **PowerdownFailure Sound** (ID: 3655606696): System-wide power failure alarm
5. **Auxiliary Power**: Emergency lights activate in both corridors (0.1s duration to full power)
6. **LightsOn_Vol3 Sound** (ID: 3206896766): Loud auxiliary power-up sound

**Player Experience**:
- Brief warning period (distant machinery blow sound from ahead)
- Sudden violent shake indicating catastrophic reactor failure
- **Complete darkness** for ~4 seconds in the zones they're standing in
- Dramatic red emergency lighting activation as auxiliary power kicks in
- Realization that they're too late—the reactor ahead has already exploded

**Affected Zones**:
- **Corridor 1**: First approach corridor (loses power)
- **Corridor 2**: Second approach corridor where scan is completed (loses power)
- **KDS Deep Exit**: Final zone already running on auxiliary power when players arrive

### Survival Encounter Mechanics

The final survival encounter uses **unique win conditions** not found in standard GTFO objectives, with dark lore implications:

**Custom Win Condition**: `GoToExitGeo`
- Players must reach the exit geomorph (KDS Deep destroyed zone)
- **NOT a standard elevator extraction** (`GoToElevator`)
- Win condition triggers instant success upon survival timer completion
- **Lore context**: No extraction exists—the duplicate team sabotaged it

**Custom Success Screen**: `SuccessScreen.ResourcesExpended`
- Displays "Resources Expended" R8E1 Valiant-style ending
- UI element: `"CM_PageExpeditionSuccess_Resources expended_CellUI 2"`
- **Game interpretation**: Emphasizes the costly nature of reaching KDS Deep
- **Lore interpretation**: The player team IS the resource that was expended

**WinOnDeath Timer** (Event Type 26):
```csharp
scanDoneEvents
    .AddMessage("SURVIVE", 6.0)
    .AddWinOnDeath(surviveDuration)  // Instant win at timer expiration
    .AddMessage("WARDEN SECURITY SYSTEMS DISABLED", surviveDuration + 2.5);
```

**How WinOnDeath Works (Game Mechanics)**:
- Starts immediately when exit scan completes
- Counts down based on tier (30-90 seconds)
- **When timer hits zero → instant mission success**
- **Players do NOT actually die in-game**—the name is misleading
- Mission succeeds even if enemies are still alive
- "WinOnDeath" means "instant win when this timer expires"

**How WinOnDeath Works (Lore Reality)**:
- Timer measures how long the player team survives their last stand
- When timer completes, the duplicate team has successfully escaped
- Players are eventually overrun and killed after this point
- "WinOnDeath" is grimly accurate—death comes after the timer
- The abandoned team is doomed, but they hold out long enough

**Wave-Based Spawns**:
- Continuous enemy spawns throughout duration
- Tier D/E receive additional mid-timer waves
- Final boss wave spawns at/after survival completion (can be ignored if timer finished)
- Spawn density designed to be overwhelming—intentionally unwinnable long-term

**Objective**: Survive, not eliminate all enemies
- Focus on staying alive, not kill counts
- Defensive positioning critical
- Consumables and resources expendable
- Game victory: Outlast the timer, not clear the zone
- Lore reality: Hold out as long as possible before inevitable death

### Two Mid-Level Path Options

**Terminal Path Characteristics**:
- Controlled pacing (player chooses when to activate)
- Requires finding and using specific terminal
- Admin-locked door unlocks on command
- Single Shadow wave spawn
- Better for coordinated teams who want timing control

**Holdout Path Characteristics**:
- Forced timer-based pacing
- Heavier combat encounter
- Tier-scaled scan difficulty and duration
- Boss spawn during countdown
- Better for teams who want more resources upfront

**Both paths lead to similar outcomes**: Resource distribution and progression timing are balanced across both options.

### ErrorAlarmChase Subtype

Special objective subtype that triggers on elevator landing:

**Event Sequence**:
1. **Error Alarm Sound** (2.0 second delay): R8E1_ErrorAlarm (ID: 1068424543)
2. **Garganta Warning** (7.0 second delay): R8E1_GargantaWarning (ID: 3030964334)
   - Subtitle ID: 442824023
   - References lore about Garganta approaching
3. **Spawn Wave** (30.0 second delay):
   - Direction: FromElevatorDirection (enemies spawn from elevator shaft)
   - Wave Settings: Error_VeryHard
   - Population: Baseline_Hybrids
   - Triggers alarm

**Purpose**: Creates immediate pressure and narrative context (facility under threat from Garganta)

### Tier Scaling

Nearly all difficulty parameters scale by tier (A through E):

**Resource Scaling**:
- Hub resource distribution remains consistent
- Enemy point values increase at higher tiers

**Enemy Composition**:
- **A-C tiers**: Standard baseline enemies, Shadows, Pouncer scouts
- **D-tier**: Adds Charger Giants, Pouncer Shadows, Infected Hybrids
- **E-tier**: Adds Nightmare Giants, Chargers waves, Mother boss

**Survival Duration**:
- Linear scaling from 30 seconds (A) to 90 seconds (E)
- 10-20 second increases per tier
- D/E receive additional mid-timer enemy waves

**Boss Types**:
- **A-D tiers**: Tank boss at survival end
- **E-tier**: Mother boss at survival end

## Recommended Strategy

### Preparation Phase

**Before Starting**:
- Understand this is NOT an extraction mission
- Final phase is survival, not evacuation
- Stock up on consumables for finale
- Coordinate team roles for survival encounter

**Hub Resource Management**:
- First hub: Moderate stockpile, expect more resources ahead
- Mid-challenge hub: Heavy stockpile, prepare for final approach
- Final hub: Take EVERYTHING - last resource opportunity

**Path Selection**:
- Terminal path if team wants controlled timing
- Holdout path if team prefers heavier resources upfront
- Both paths are viable, choose based on team coordination preference

### Execution Phase (Start to Mid Challenge)

**Corridor Navigation**:
1. Clear start corridors quickly to avoid ErrorAlarmChase spawn wave
2. Don't linger near elevator - enemies spawn from that direction
3. Conserve resources in early corridors (limited supplies)

**Hub Puzzles**:
1. Identify puzzle type quickly (Keycard/Generator/Terminal)
2. Coordinate team split for efficient completion
3. Stock up on resources before continuing

**Enemy Wave Management**:
1. Shadow wave (40% traverse): Clear quickly, minimal threat
2. Pouncer scout (25% traverse): Isolate and eliminate
3. Giant Shooters (85% traverse): Suppress fire, coordinate focus
4. Pouncer Shadow (90% D/E): Priority target, high threat

**Mid-Challenge Path Execution**:

**Terminal Path**:
1. Locate security zone terminal quickly
2. Execute `KDS-DEEP_DEACTIVATE_DEFENSE` command
3. Prepare for Shadow wave spawn (25 second delay)
4. Wait for admin door unlock (11 second delay)
5. Stock up at hub before continuing

**Holdout Path**:
1. Position defensively before triggering time lock
2. Prepare for immediate mixed enemy spawn
3. Focus fire on boss when it spawns (15-30s in)
4. Survive full countdown duration
5. Stock up immediately after completion

### Final Approach Phase

**Corridor Management**:
1. Clear minimal corridors with limited resources
2. Prepare for Tank Pouncer spawn (20s delay on door open)
3. Avoid wasting resources on delayed spawns (120s+ delays)
4. Push forward quickly

**Final Hub**:
1. Take ALL resources - health, tools, ammo
2. Discuss survival strategy with team
3. Assign defensive positions for finale
4. Ensure full ammo and consumables

### Reactor Explosion Phase

Players experience the reactor explosion **in the zones they're traversing**, not behind them. The explosion affects Corridors 1 and 2 as players complete the door scan.

**Pre-Explosion** (In Corridor 2):
1. Complete surge alarm/team scan quickly to unlock progression
2. Listen for distant machinery blow sound (first warning from KDS Deep ahead)
3. Brace for screen shake at +17 seconds (violent 1-second shake)
4. Know the path forward to final KDS Deep exit zone

**During Explosion** (17-21 seconds after scan):
1. **Screen shakes violently** - indicates reactor catastrophic failure
2. **Complete darkness descends** on Corridors 1 and 2 (zones you're in)
3. **Activate flashlights immediately** - you'll be blind for ~4 seconds
4. **DO NOT PANIC** - darkness is temporary, auxiliary power is coming
5. **Stay together as a team** - call out positions verbally
6. **Do NOT try to sprint forward blindly** - wait for lighting restoration
7. Auxiliary power activates after ~4 seconds with dramatic red emergency lighting

**Post-Explosion** (Under Emergency Lighting):
1. Realize the reactor ahead has already failed—explosion wasn't behind you
2. Navigate forward under emergency red auxiliary lighting
3. Understand that KDS Deep will be destroyed when you arrive
4. Prepare mentally for survival encounter (no extraction will be available)
5. Stock up on any remaining resources before entering final zone

**Key Understanding**:
- You're NOT outrunning an explosion chasing you from behind
- You're witnessing an explosion that's ALREADY occurred ahead of you
- The zones you're in lose power because the reactor ahead has failed
- This explains why KDS Deep is destroyed and has no extraction

### Survival Encounter Phase

**Initial Setup** (Before scan):
1. Position team in defensible formation
2. Identify cover and fallback positions
3. Assign sectors/lanes to team members
4. Prepare consumables for immediate use
5. **Understand the reality**: This is a last stand, not an escape

**During Survival** (30-90 seconds):
1. **Focus on STAYING ALIVE, not kill counts**
2. Use all consumables liberally (mine, C-foam, sentries)
3. Rotate positions to avoid being pinned
4. Call out high-priority targets (Giants, Chargers)
5. Revive downed teammates quickly
6. Watch survival timer—hold out to the end
7. **No rescue is coming**—timer measures your last stand

**Wave Management**:
1. **8-second wave**: Baseline enemies, moderate threat
2. **Mid-timer waves** (D/E only): High threat, coordinate focus
3. **Final boss wave**: Spawns AT/AFTER survival completion
4. Boss can be ignored if survival timer completes
5. Spawns are overwhelming by design—you can't clear everything

**Win Condition Awareness**:
- Game: Timer completion = mission success
- Lore: Timer measures how long you survive before being overrun
- Prioritize time over kills
- Last player alive can achieve game victory by surviving to timer end
- Don't give up if most of team is down—go down fighting
- In lore, you're all doomed, but make it count

## Terminal Commands

### Objective-Specific Commands

**KDS-DEEP_DEACTIVATE_DEFENSE** (Terminal Path Only)
- **Description**: Deactivate KDS Deep defense grid to unlock progression
- **Usage**: Enter command at security terminal in mid-challenge zone
- **Output Sequence**:
  1. "Authenticating with BIOCOM..." (2.5s with spinner)
  2. "Confirming valid terminal ID" (1.5s)
  3. "Confirming operative credentials" (1.5s)
  4. "Deactivating defense grid..." (4.0s with spinner)
  5. "KDS Defense system **inactive**" (1.0s)
- **Effects**:
  - Unlocks admin-locked door after 11 seconds
  - Updates objective to "Proceed to KDS Deep"
  - Spawns Shadow wave after 25 seconds

### Navigation Commands

**PING [ZONE_ALIAS]** - Locate zones by alias
- `PING KDS` - Locate KDS Deep zones
- `PING` - General area scan

**LIST** - Show current objectives and progress

## Notable Expeditions

**R8E1 "Valiant"** (Rundown 8, E-tier)
- The vanilla GTFO expedition that this objective recreates
- Features similar reactor explosion and KDS Deep arrival
- Known for dramatic environmental storytelling
- One of the most memorable missions in GTFO's history
- [R8E1 Wiki Page](https://gtfo.wiki.gg/wiki/R8E1)

**AutogenRundown Implementation**
- Custom objective exclusive to AutogenRundown mod
- Appears in C-tier expeditions primarily
- Can potentially appear in E-tier with full difficulty scaling
- Uses procedural generation while maintaining R8E1's structure
- Features custom R8E1-specific geomorphs and assets

## Comparison to Related Objectives

| Aspect | Reach KDS Deep | Clear Path | Reactor Startup | Survival |
|--------|----------------|------------|-----------------|----------|
| **Objective Type** | Custom | Vanilla | Vanilla | Vanilla |
| **Main Goal** | Reach zone + survive | Reach extraction | Reactor verification | Timed survival |
| **Time Pressure** | High (explosion) | Low | High (waves) | Very High |
| **Combat Intensity** | Very High | Low-Medium | Very High | Extreme |
| **Unique Mechanics** | Explosion, lights out | Simple navigation | Verification codes | None |
| **Team Coordination** | High | Low | High | Very High |
| **Environmental Hazards** | Darkness, explosion | Minimal | None | None |
| **Win Condition** | Survival timer | Reach extraction | Complete sequence | Survival timer |
| **Solo Feasibility** | Very Difficult | Easy | Difficult | Nearly Impossible |
| **Resource Availability** | High | Varies | High | Pre-stocked |

## Common Challenges

### Challenge: Reactor Explosion Timing

**Problem**: Lights go out in the zones players are currently occupying, completely disorienting the team in total darkness for several seconds

**Solution**:
- **Understand what's happening**: The reactor ahead has exploded, cutting power to zones you're in
- **You're NOT being chased**: The explosion is ahead of you, not behind you
- Activate flashlights **IMMEDIATELY** when lights cut
- **Do NOT sprint forward blindly** - you're in darkness, not running from it
- Stay together as a team during the ~4-second blackout
- Use voice communication to call out positions constantly
- Wait for auxiliary power restoration (comes automatically after ~4 seconds)
- Don't panic - darkness is brief, scripted, and unavoidable
- Know the path forward before triggering the scan
- This explains why KDS Deep will be destroyed when you arrive

### Challenge: Final Survival Encounter

**Problem**: Overwhelming enemy waves spawn continuously, making it feel impossible to survive the full timer. Additionally, there's no extraction available—what happens when the timer ends?

**Solution**:
- **Understand the win condition**: Timer expiration = instant win via WinOnDeath mechanic
- **Players do NOT actually die in-game** - mission succeeds when timer hits zero
- Focus ONLY on staying alive, not killing enemies
- Use all consumables (mines, sentries, C-foam) immediately
- Position in defensible location with cover
- Coordinate team positioning to watch all angles
- Revive downed teammates quickly
- Remember: Timer completion = instant mission success
- Final boss spawns AFTER survival timer, can be completely ignored
- Custom "Resources Expended" win screen displays (no traditional extraction)
- You've arrived too late to extract—survival IS the victory condition

### Challenge: Understanding the "Victory"

**Problem**: The "Resources Expended" victory screen feels hollow or confusing. Is this really a successful mission? Why does it feel like a defeat?

**Explanation**:
The victory screen is intentionally ambiguous because **in lore, this is not a happy ending**:

**Game Mechanics Perspective**:
- Mission counted as success if survival timer completes
- "Resources Expended" is a valid victory condition
- Players don't die in-game—mission ends successfully
- All objectives technically completed

**Lore Reality Perspective**:
- **This is a fatal betrayal**, not a real victory
- A duplicate team has teleported out via matter wave projector
- They deliberately sabotaged the reactor to cover their escape
- Player team was left behind to die—no rescue is coming
- The timer measures how long you survive before being overrun
- Players are eventually killed by the endless enemy waves
- "Resources Expended" refers to **YOUR team**—you are the resources
- The duplicate team succeeds; the player team dies

**Why Warden Counts It As Success**:
- Objectives were completed (reached KDS Deep)
- The duplicate team's mission succeeded
- Player team surviving long enough allowed duplicate team to escape
- From Warden's perspective, expendable resources fulfilled their purpose

**The Dark Irony**:
This is one of GTFO's bleakest narratives—a pyrrhic victory where you "win" the game by being successfully sacrificed. The triumph belongs to the team that abandoned you, not to you.

### Challenge: Mid-Level Path Choice

**Problem**: Uncertain which path (Terminal vs Holdout) is better for the team

**Solution**:
- **Choose Terminal path if**:
  - Team wants controlled timing
  - Better at coordinated command execution
  - Prefers lower combat intensity
- **Choose Holdout path if**:
  - Team wants more guaranteed resources
  - Comfortable with boss fights
  - Prefers straightforward combat over puzzle mechanics
- Both paths are balanced and viable

### Challenge: Resource Management

**Problem**: Running out of ammo and consumables before reaching the final survival encounter

**Solution**:
- Conserve ammo in early corridors (limited enemy spawns)
- Stock up heavily at EVERY hub zone
- Use melee on lone enemies when safe
- Save sentries and mines for final survival
- Final hub is last resource opportunity - take EVERYTHING
- Don't waste consumables on delayed spawns (120s+ timers)

### Challenge: Team Coordination During Explosion

**Problem**: Team members get separated during the lights-out sequence, leading to disorganization

**Solution**:
- Establish leader before explosion zone
- All players follow leader's position
- Use voice comms constantly during darkness
- Move slowly and deliberately, not frantically
- Regroup immediately when auxiliary power activates
- Practice staying close even when you can see

### Challenge: Boss Spawns

**Problem**: Tank or Mother spawns during critical moments (mid-challenge holdout or final survival)

**Solution**:
- **Mid-challenge boss**:
  - Spawns 15-30 seconds into countdown
  - Coordinate burst damage immediately
  - Use weak points (back tumors for Tank, occiput for Mother)
  - Don't need to kill, just survive the timer
- **Final survival boss**:
  - Spawns at/after survival timer completion
  - Can be completely ignored if timer finished
  - If timer still running, focus fire on weak points

### Challenge: ErrorAlarmChase Spawn Wave

**Problem**: Enemies spawn from elevator direction early in mission (30 seconds after land), overwhelming team

**Solution**:
- Clear start corridors QUICKLY
- Don't linger near elevator area
- Push forward to first hub before 30-second mark
- Team should be prepared for wave when it spawns
- Error_VeryHard wave is manageable if expected
- Use hub resources to recover after engagement

### Challenge: No Starting Area

**Problem**: Mission starts immediately with no safe zone to prepare

**Solution**:
- Discuss strategy during elevator descent
- Assign roles before landing (leader, scout, defender)
- Start moving immediately on landing
- Don't try to prepare in spawn zone - it's not safe
- ErrorAlarmChase wave spawns at 30 seconds regardless

## Tips

### Path Selection
- Scout the two mid-level paths before choosing if possible
- Terminal path gives more control over timing and pacing
- Holdout path provides heavier resource stockpile upfront
- Both paths lead to similar total resources
- Terminal path has lower combat intensity
- Holdout path is more straightforward (no terminal commands)

### Reactor Explosion
- **Critical**: Explosion happens IN FRONT in zones you're occupying, NOT behind you
- You witness the reactor failure firsthand in Corridors 1 & 2
- Prepare for total darkness lasting ~4 seconds in your current zones
- **Do NOT try to sprint forward** - you're experiencing the explosion, not fleeing it
- Stock up on flashlight batteries before explosion zone
- Auxiliary power provides dramatic red emergency lighting after ~4 seconds
- Screen shake is the clearest warning of explosion timing
- Distant machinery blow sound from KDS Deep ahead is the first warning
- PowerdownFailure sound indicates system-wide power failure
- Explosion sequence is scripted at 17+ seconds after door scan completion
- This explains why KDS Deep is destroyed when you arrive - the reactor has already failed
- Know the path forward before triggering the scan

### Survival Encounter
- **No extraction available** - duplicate team sabotaged the reactor and extraction
- **Custom win condition**: `GoToExitGeo` (NOT standard elevator extraction)
- **Custom "Resources Expended" win screen** displays (R8E1 Valiant-style)
- **Lore context**: "Resources Expended" refers to YOUR team being sacrificed
- Save ALL consumables for final survival encounter
- **Survival timer completion = instant win** via WinOnDeath mechanic
- **Players do NOT actually die in-game** - WinOnDeath means "instant win at timer expiration"
- **In lore**: Players are eventually overrun and killed after timer ends
- **Game win condition**: Survive the timer, NOT kill all enemies
- **Lore reality**: Last stand before inevitable death—hold out as long as you can
- "SURVIVE" message appears at 6 seconds into encounter
- "WARDEN SECURITY SYSTEMS DISABLED" confirms game victory at timer + 2.5s
- **Duplicate team has already escaped** - they teleported out and left you to die
- **No rescue is coming** - this is your final stand
- Timer measures how long you survive before being overrun
- Final boss wave spawns at/after survival duration end (can be ignored)
- Boss spawns are tier-dependent (E-tier gets Mother, others get Tank)
- Focus on staying alive over getting kills
- Use defensive positioning with cover
- Coordinate team sectors to watch all angles
- Mission succeeds even if enemies are still alive when timer expires
- **Dark irony**: "Victory" means you lasted long enough for the duplicate team to escape

### Reactor & Lore
- **Reactor explosion happens AHEAD of players** (in KDS Deep), not behind them
- Players arrive too late - reactor has already failed
- Explosion affects zones players are currently occupying (Corridors 1 & 2)
- Reactor explosion is scripted, not random or avoidable
- KDS Deep zone uses custom R8E1-specific "destroyed" geomorph
- KDS Deep found destroyed and on auxiliary power when players arrive
- Garganta warning sound at start references deep lore (catastrophe in progress)
- ErrorAlarmChase spawn wave comes from elevator direction (automated defenses)
- Facility location is Chicxulub crater (dinosaur extinction site)
- The Inner spacecraft is beneath KDS Deep facility
- "Resources Expended" reflects the cost of reaching KDS Deep too late

### Combat & Resources
- Clear start zones quickly to avoid early ErrorAlarmChase pressure
- Hub puzzles are randomly selected (Keycard/Generator/Terminal)
- Both mid-level paths lead to similar outcomes
- Higher tiers get more dangerous bosses and longer survival
- Nightmare Giants and Chargers appear in E-tier only
- Final hub before explosion is last resource opportunity
- Conserve ammo in early phases, spend freely in finale

### Difficulty Scaling
- Final survival duration scales by tier (30-90 seconds):
  - A: 30s, B: 40s, C: 50s, D: 70s, E: 90s
- D/E tiers receive additional mid-survival enemy waves
- E-tier gets Mother boss instead of Tank
- Higher tiers have more dangerous enemy compositions
- Resource distribution remains consistent across tiers
- Puzzle difficulty does not scale by tier

### Technical Details
- **Win condition**: `GoToExitGeo` (NOT `GoToElevator`)
- **Success screen**: `SuccessScreen.ResourcesExpended` (R8E1 Valiant-style)
- **WinOnDeath event** (Type 26): Instant win at timer expiration
- Machinery blow sound is first warning (distant, from KDS Deep ahead)
- Explosion delay is random 1-4 seconds + 17 seconds from scan completion
- Screen shake: 5.0 amplitude, 90Hz frequency, 10.0 radius (1-second duration)
- Lights out duration: ~4 seconds before auxiliary power restoration
- Lights out affects Corridors 1 & 2 (zones players occupy)
- Exit zone geomorph: `geo_64x64_mining_HSU_exit_R8E1.prefab` (destroyed)
- Exit zone lighting: `Reactor_blue_to_red_all_on_1` (auxiliary power)
- "SURVIVE" message at exit scan + 6 seconds
- "WARDEN SECURITY SYSTEMS DISABLED" at survival duration + 2.5s
- WinOnDeath timer starts on exit scan completion
- No extraction required - survival timer completion = instant win
- Players do NOT die - WinOnDeath name is misleading (means instant win)

## Sources

- AutogenRundown Implementation: `WardenObjective.ReachKdsDeep.cs`
- AutogenRundown Layout: `LevelLayout.ReachKdsDeep.cs`
- [R8E1 "Valiant" - Official GTFO Wiki](https://gtfo.wiki.gg/wiki/R8E1)
- [GTFO Rundown 8 - GTFO Fandom](https://gtfo.fandom.com/wiki/Rundown_008)
- AutogenRundown Knowledge Base: `kb/base-game.md`
- [GTFO Game Mechanics - Screen Rant](https://screenrant.com/gtfo-survival-horror-game-mechanics-explained/)
