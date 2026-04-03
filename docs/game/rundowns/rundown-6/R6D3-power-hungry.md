# R6D3 - "Power Hungry"

## Overview

- **Rundown:** ALT://Rundown 6.0: Destination
- **Tier:** D
- **Environment:** Main Sector + Secondary Sector + Overload Sector
- **Dimensions:** No - standard zone progression, though heavily features Shadows and Shadow Scouts

## Objectives

### Main Objective

Reactor Startup: Starting in Zone 274, collect a Bulkhead Key and navigate through multiple zones to gather a Power Cell from Zone 279 (requiring a Key from Zone 278). Enter `REACTOR_STARTUP` on a reactor terminal to initiate a 5-phase reactor sequence:

1. **Code 1 (Zone 287):** 2-minute timer, 50-second wave with basic enemies
2. **Code 2 (Zone 283):** 3.5-minute timer, 1:40 wave with mixed types
3. **Code 3 (Zone 286):** 6-minute timer, 1:40 wave, requires colored key retrieval
4. **Code 4 (Zone 291):** 5-minute timer, 1:40 wave spawning Flyers via Class IV Alarm
5. **Code 5 (Zone 289):** 60-second timer, 40-second wave with 13 Shadows plus 1 Giant

After completion, a 15-minute countdown begins before an Error Surge alarm activates.

### Secondary Objective

Gather Items: Grab a Bulkhead Key from Zone 289, plug it into a controller in Zone 291, then collect 5 of 6 available Personnel IDs from Zones 335/336/337. This objective is a race against the clock -- at most 4 minutes should be spent in each zone.

### Overload Objective

Reactor Shutdown: Enter Zone 495 and activate a reactor shutdown sequence in Zone 496 via terminal command. This triggers a Class VI Alarm spawning Flyers and Shadows. Navigate to Zone 497 to collect a Bulkhead Key while managing an Error Alarm spawning enemies toward the elevator. The Error Alarm can be deactivated via Zone 292 terminal.

## Enemy Types

**Main Sector - Common:**

- Striker
- Shooter
- Giant
- Big Shooter
- Hybrid
- Scout

**Main Sector - Unique:**

- Shadow
- Big Shadow
- Tank

**Secondary Sector:**

- Shadow
- Big Shadow
- Shadow Scout

**Overload Sector:**

- Striker
- Shadow
- Big Shadow
- Shadow Scout
- Flyer

## Level Description

Power Hungry is a D-tier level featuring all three objective tiers across an expansive zone layout (274-292 main, 335-337 secondary, 495-497 overload). The main objective centers on a demanding 5-phase reactor startup with escalating difficulty, culminating in a wave of 13 Shadows. Zone 275 functions as a respawn room ("Cocoon") containing two Tanks and multiple respawning enemies. Zone 278 features an Error Alarm spawning 4 Shadows every ~10 seconds, deactivatable via `DEACTIVATE_ALARMS` terminal command in Zone 279. After reactor completion, fog slowly rises throughout the level. The secondary objective is a timed sprint to collect Personnel IDs from Shadow-filled zones. The overload objective adds a reactor shutdown sequence with Class VI Alarm and additional Error Alarm.

Prisoner Efficiency (all three objectives) requires completing objectives in the order: Overload, Main, Secondary.

## Notable Mechanics

- **5-Phase Reactor Startup:** Escalating waves from basic enemies to Shadows, with increasing timers.
- **Respawn Zones:** Zone 275 (Cocoon) contains two Tanks and respawning enemies; Zone 276 is also a respawn zone.
- **Class VI Alarm:** Zone 277 (main), Zone 496 (overload) spawning Flyers, Shadows, and Big Shadows.
- **Error Alarms:** Zone 278 spawns 4 Shadows every ~10 seconds (deactivated via `DEACTIVATE_ALARMS` in Zone 279); Zone 497 spawns 3 Strikers/Shooters every ~20 seconds (deactivated via Zone 292 terminal).
- **Class IV Alarm:** Zone 291 spawns Flyers exclusively.
- **Rising Fog:** After main objective completion, fog slowly rises; positioning on Zone 281 bridges improves visibility.
- **Blood Doors:** Zones 286 and 289 with special mechanics.
- **Stealth Scans:** Zones 276 and 279.
- **Shadow Scouts:** Present in secondary and overload sectors.
- **15-Minute Countdown:** Post-reactor timer before Error Surge activates.

## Resources

| Zone    | Ammo   | Medi    | Tool   |
| ------- | ------ | ------- | ------ |
| 274     | 80%    | 0%      | 40%    |
| 281     | 160%   | 120%    | 80%    |
| 287     | 200%   | 40%     | 80%    |
| 289     | 140%   | 100%    | 80%    |
| 335-337 | 80% ea | 120% ea | 80% ea |
| 496     | 200%   | 60%     | 80%    |

## Strategy Notes

- All tools except C-Foam Launcher are useful (long sightlines favor sniper sentries instead).
- Thermal weapons are valuable for additional objectives.
- Precision Rifle is especially useful for killing Shadow Scouts without risk.
- Bio Tracker user should focus on defense; dedicated code hunters retrieve objective items.
- Zone 281 bridges provide optimal holding positions with fog clarity advantage at bridge tops.
- Secondary objective defense relies on melee weapons against wave enemies.
