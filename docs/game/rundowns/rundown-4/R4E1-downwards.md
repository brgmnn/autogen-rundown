# R4E1 - "Downwards"

## Overview

- **Rundown:** ALT://Rundown 4.0: Contact
- **Tier:** E
- **Environment:** Mines
- **Dimensions:** No

## Objectives

### Main Objective

Distribute Powercells: Distribute Power Cells to lift the ongoing Emergency Lockdown. The expedition features a continuous Error Alarm spawning a Tank approximately every 4 minutes, with the first appearing 10 seconds after entry. Non-Infectious Fog gradually fills the area, reducing visibility. Players navigate through Zones 455-458, collect two Power Cells from the starting area, and insert them into Generators in Zone 458. After insertion, progress through Zones 459-462 (Stealth Scan alarms, Shadows, Shadow Scouts), then through Zone 463 (Scream Door containing a Tank and Big Mother, CLASS VI SMALL SUSTAIN alarm) to reach extraction at Zone 464 (triggers Error Alarm spawning Big Chargers).

### Secondary Objective

Reactor Startup: Return to Zone 455 and insert a Bulkhead Key in Zone 455B to access Zone 510. Reach the Reactor Terminal in Zone 511 and initiate startup. Complete 6 waves with timer-based code inputs (25 seconds per code, 15-second preparation, 45-second wave duration). Codes appear in Zones 512, 514, 515, 517, and 516. Waves 1-3 spawn Strikers and Shooters; Waves 4+ add Shadows; Wave 6 additionally includes Hybrids. Strongly recommended as it provides significant additional resources for the main objective.

**Reactor Wave Details:**

- Waves 1-3: Strikers and Shooters
- Waves 4-5: Add Shadows
- Wave 6: Add Hybrids
- Code note: After the first code, codes are intended to be failed repeatedly while prisoners search for them.

### Overload Objective

None

## Enemy Types

### Main Sector

- Striker
- Shooter
- Giant
- Big Shooter
- Scout
- Hybrid
- Big Charger
- Shadow
- Big Shadow
- Shadow Scout
- Big Mother (boss)
- Tank (boss, recurring via Error Alarm every ~4 minutes)

### Secondary Sector

- Striker
- Shooter
- Giant
- Big Shooter
- Charger
- Big Charger
- Shadow
- Big Shadow
- Shadow Scout
- Hybrid
- Mother (4 sleeping in Zone 513)
- Baby Striker (spawned by awakened Mothers, ~60-80 total possible)

## Level Description

R4E1 is the final expedition of ALT://Rundown 4.0 and the only E-tier level. The expedition name "Downwards" is part of the slogan "Onwards and downwards!" used by the Santonian Mining Company, connecting it thematically to R4A3 "Onwards." A story log is located in the Zone 459 terminal.

This is an extremely challenging level defined by its relentless Tank spawning mechanic -- a Tank appears every ~4 minutes throughout the entire expedition, and these Tanks can awaken rooms by screaming or awaken nearby Sleepers when breaking doors. The first Tank spawn can be somewhat controlled by positioning players in Zone 455B when its roar is heard.

The Secondary objective, while optional, is strongly recommended as the secondary zones contain massive resource stockpiles (Zone 510 alone has 400 Ammo, 100 Medi, 180 Tool). The level progressively layers environmental hazards: non-infectious fog during the Error Alarm phase, then infectious fog after Power Cell insertion, culminating in Shadow-filled zones and a climactic encounter with a Tank and Big Mother behind a Scream Door.

## Notable Mechanics

- **Persistent Tank Spawning:** Error Alarm spawns a Tank every ~4 minutes starting 10 seconds after entry. Tanks can awaken sleeper rooms by screaming and awaken nearby Sleepers by breaking doors. No cap on simultaneous Tanks alive.
- **Fog Phases:** Non-Infectious Fog fills during Error Alarm phase. After Power Cell insertion, non-Infectious Fog clears but Infectious Fog takes over in Zone 458 and surrounding areas.
- **Stealth Scan Alarms:** Zones 460-462 feature stealth-based scans.
- **Scream Door:** Zone 463 entry is a Scream Door containing both a Tank and Big Mother that awaken immediately upon opening.
- **CLASS VI SMALL SUSTAIN Alarm:** Used for Zone 463.
- **Blood Door:** Zone 515 entrance spawns 2 Hybrids and ~12 Strikers/Shooters.
- **Sleeping Mothers Warning:** Zone 513 contains 4 sleeping Mothers. Awakening them all can cause significant performance issues due to ~60-80 Baby Strikers spawning.
- **Reactor Code Mechanic:** After the first code, codes are designed to be failed repeatedly while prisoners search for terminals.
- **Disinfection Scarcity:** A single Disinfection Pack is always found in Zone 457. Fog Repellers rarely appear in Zone 459. No Fog Repellers exist before Zone 459.
- **Resource Distribution:** Secondary zones contain massive resources (Zone 510: 400A/100M/180T, Zone 511: 300A/140M/120T, Zone 512: 240A/200M/120T). Main zones are comparatively sparse.
- **Extraction:** Zone 464 triggers an Error Alarm spawning Big Chargers.

## Known Bugs

- **Lighting Bug:** After ~30 minutes with Error Alarm active, lighting breaks and becomes much darker; fog particles become greenish; flashlights barely function. Resolves after Power Cells are inserted.
- **Tank Stuck Issue:** First Tank is prone to getting stuck in Zone 455G. Modded fixes available (EnemyAnimationFix, StuckEnemyFix).
- **Host Migration Duplication:** Extremely rare issue during Reactor Startup causing all sleeping/patrolling enemies to duplicate (only 3 known community occurrences).
