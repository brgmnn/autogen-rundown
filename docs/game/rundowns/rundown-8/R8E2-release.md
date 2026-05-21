# R8E2 - "Release"

## Overview

- **Rundown:** Rundown 8.0: Duality
- **Tier:** E (highest difficulty)
- **Environment:** Deep Complex / KDS Deep (Alpha One)
- **Dimensions:** Yes -- Zone 545 (KDS Deep) is a shared dimensional space that exists across multiple expeditions (R5E1, R8E1, R8E2). The Destination arena in the Reactor secondary is a callback to R6D1 where Alpha One merged with The Complex.

## Objectives

### Main Objective

**Process Item:** Transport the Matter Wave Projector from Zone 490 to Zone 497/545 (KDS Deep). A unique 1-minute countdown timer initiates at level start -- if it reaches zero, a permanent Surge Error Alarm activates spawning only Nightmare Strikers and Nightmare Shooters. The timer is extended via `ADMIN_TEMP_OVERRIDE` terminal commands found throughout the level. Maximum mission runtime is approximately 93 minutes.

**Zone Progression:**

1. **Zone 490** (Start): No enemies. Lock Melters, Long Range Flashlights, Glow Sticks. Terminal grants 5 extra minutes.
2. **Zone 491**: Heavy resistance (Strikers, Shooters, Giants, Hybrids, Shadows, Big Shadows, Mother, Zoomer Scout, Scout, 2 Charger Scouts). +5 minutes.
3. **Zone 492** (Respawn/Cocoon): Mother, Tank, 3 Charger Scouts. MWP can be deposited in 492C. Zone 492D leads to Zone 493 via Class III Stealth Scan. +5 minutes.
4. **Zone 493**: Nightmare Strikers/Shooters, Shadows, Big Shadows, Tank, Charger Scout, 4 Zoomer Scouts, Scout. Execute `BYPASS_Z494`. Story Log. +5 minutes.
5. **Zone 494** (Respawn): 2 Mothers, Shadow Scout, Zoomer Scout, 2 Scouts. Class IV Stealth Scan. Retrieve Power Cell for Generator in 494D. +9 minutes. Generator unlock triggers Class VI Mixed Alarm to Zone 495 (vast Nightmare spawns).
6. **Zone 495**: Tank, 2 Charger Scouts, Shadow Scout, Nightmare Scout. +8 minutes. Collect Bulkhead Key for Secondary access.
7. **Zone 496** (Safe Zone): Resources, no enemies. +3 minutes. Door to Zone 498 requires Class VI Mixed Alarm (pure Nightmare Strikers/Shooters/Giants). Big Mother spawns upon alarm completion.
8. **Zone 498**: Shadows, Big Shadows. +10 minutes. Door to Zone 499 requires Class VI Mixed Alarm (Shadows/Big Shadows).
9. **Zone 499** (Alpha One): 2-3 Tanks. Execute `MAIN_WASTE_FLUSH` to trigger Kraken fight. Story Log. +14 minutes (final extension). Zone exists outside map boundary with performance issues.
10. **Zone 545** (KDS Deep): HearSay Terminal communication with alternate team. Collect Data Cubes. Plug MWP into Jump Gate platform. Class III Alarm sequence (3 scans): lower floor, Jump Gate platform (2 Snatchers at ~40%), center of Jump Gate (multiple Tanks post-completion). Scans display as "Solo Scan" on HUD despite being Full Team Scans. Mission ends: `.////SIGNAL_LOST`

### Secondary Objective

**Reactor Startup:** Mandatory for mission completion despite being labeled secondary. Complete 3-wave Reactor Startup in Zone 618. Entering grants 22 extra minutes.

**Wave 1:** Strikers, Shooters, Giants, Flyers, Big Shadows. Code in Zone 619 (1 Tank, 1 Zoomer Scout, 3 Corrupt Scans). Lights toggle on/off every 18 seconds. Corrupt Scans trigger Destination survival (Nightmare spawns for ~1 minute).

**Wave 2:** Strikers, Shooters, Giants, Flyers, 1 Tank. Code among 4 possible terminals in Zone 620 (Hybrids, Giants, Strikers, Shooters).

**Wave 3:** Nightmare Strikers, Nightmare Shooters, Big Shadows, 3 Tanks (periodic spawns). Code in Zone 621 (2 Mothers, 2 Shadow Scouts, Shadows, Corrupt Scans). Host recommended for code hunt to prevent room awakening.

Reactor completion grants 5 extra minutes and opens Zone 496 from Zone 495.

### Overload Objective

None

## Enemy Types

- Striker, Shooter, Giant, Big Shooter, Hybrid, Scout
- Charger, Charger Scout
- Shadow, Big Shadow, Shadow Scout
- Nightmare Striker, Nightmare Shooter, Nightmare Scout
- Zoomer Scout, Snatcher
- Flyer (Secondary only)
- Mother (multiple), Big Mother
- Tank (multiple encounters)
- Kraken (final boss, Zone 499)

## Level Description

R8E2 is the final expedition in GTFO and the second ending of the game. It is the longest possible mission (~93 minutes maximum runtime) and serves as a knowledge check of all enemy types and mechanics. The level takes place directly underneath both R5E1 and R8E1 canonically, with the final zone (KDS Deep/Alpha One) shared between all three expeditions. Chronologically it occurs before R8E1 events. The OG Team likely triggers the Kraken, subsequently initiating KDS Deep defenses.

The Destination arena in the Reactor secondary is a callback to R6D1 where Alpha One merged with The Complex. The mission ends with the cryptic message `.////SIGNAL_LOST`, concluding GTFO's storyline.

## Notable Mechanics

- **1-minute countdown timer** from level start -- permanent Surge Error Alarm if it reaches zero (only Nightmare Strikers/Shooters)
- **`ADMIN_TEMP_OVERRIDE` terminals** throughout the level extend the countdown by variable amounts (5-22 minutes)
- **~93 minute maximum runtime** -- longest mission in GTFO
- **Mandatory "Secondary" Objective** -- Reactor Startup must be completed to progress
- **Kraken boss fight** triggered by `MAIN_WASTE_FLUSH` in Zone 499
- **Class VI Mixed Alarms** ("NM6" / "Shadow 6") -- extended, extremely dangerous alarms requiring funneling and C-Foam strategy
- **Class III/IV Stealth Scans** requiring team coordination
- **Respawn zones** (Cocoons) in Zones 492 and 494
- **Corrupt Scans** in Secondary with 18-second light cycling
- **HearSay Terminal** in KDS Deep for communication with alternate team
- **Jump Gate platform** -- MWP insertion point for final sequence
- **Zone 499 performance issues** -- tiles outside map boundary create Broken Cells; Kraken spawn location unpredictable (can softlock)
- **"Solo Scan" display bug** -- HUD shows Solo Scan despite Full Team Scan mechanics in Zone 545
- Terminal commands: `ADMIN_TEMP_OVERRIDE`, `BYPASS_Z494`, `MAIN_WASTE_FLUSH`
- Recommended performance mods: BorkenCellGeoFix, LGTuner (MTFO must be off)
