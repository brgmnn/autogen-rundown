# R8C2 - "Unplugged"

## Overview

- **Rundown:** Rundown 8.0: Duality
- **Tier:** C
- **Environment:** Industrial/Datacenter
- **Dimensions:** No

## Objectives

### Main Objective

**Activate Generator Cluster:** One of the longest missions in the game. Five-phase progression:

**Phase 1 - Data Extraction (Zones 265-266B):** Advance to Zone 266B and insert Datasphere to extract Micro Drive. Zone 266 contains heavy resistance (Strikers, Shooters, Chargers, Big Chargers, Scout, Zoomer Scout).

**Phase 2 - VoGen Recording (Zone 249):** Navigate through Zone 267 to reach Zone 249 (Class VI Mixed Alarm). Place Microdrive on terminal table and record message. Zone 267 contains Chargers, Big Chargers, and Charger Scout.

**Phase 3 - Blood Door Passage (Zones 269-270):** Enter Blood Door Zone 269, proceed to Class VII Mixed Alarm Zone 270. Tank awakens automatically upon zone entry. Checkpoint activated.

**Phase 4 - Power Cell Collection (Four Cells):**

1. North Path (Zone 271-272): Class IV Mixed Alarm spawning Strikers, Chargers, Hybrids.
2. South Path (Zone 273-274): Error Alarm spawning 10 Strikers/Shooters every ~30 seconds until deactivated via Terminal command `ALARM_DEACTIVATE` in Zone 274B. Snatcher spawns during alarm.
3. East Path (Zone 277): Class III Mixed Alarm causing persistent blackout, spawning Shadows and Big Shadows. Tank present.
4. West Path (Zone 275-276): No alarm triggered. Chargers and Charger Scout present.

**Phase 5 - MRI Initiation:** Execute `INITIATE_MRI` at marked terminal in Zone 270. Three-phase alarm sequence:

- Room Scan (~60 sec): Tank and 2 Big Chargers spawn from 2 rooms away
- Center Scan (~50 sec): Large team scan, Snatcher + Nightmare Strikers spawn
- MRI Machine Scan (~35 sec): Medium team scan, 2 Snatchers + multiple Big Chargers; counts as extraction scan

### Secondary Objective

None

### Overload Objective

None

## Enemy Types

- Striker, Shooter, Giant, Scout, Hybrid
- Charger, Big Charger, Charger Scout
- Shadow, Big Shadow, Shadow Scout
- Nightmare Striker, Nightmare Shooter
- Zoomer Scout, Snatcher
- Tank (boss)

## Level Description

R8C2 is one of the longest missions in the game, featuring a single checkpoint early in the mission relative to its total length. The level connects directly to R8C1 through shared room Zone 249 -- both expeditions access this room via different routes (R8C1 via Zone 244, R8C2 via Zone 267). The VoGen message recorded here is heard in R8C1. Mutual Emergency Lockdowns prevent team cross-contamination between the two expeditions. Project Insight Archive context is mentioned regarding a clean Membrane location.

A secondary Micro Drive exists in Zone 266A but serves no objective function -- noted as a developer oversight.

## Notable Mechanics

- **Extreme length** with a single early checkpoint, requiring significant post-checkpoint progression
- **Four branching Power Cell collection paths** from Zone 270 (North, South, East, West)
- **Deactivatable Error Alarm** in South path via `ALARM_DEACTIVATE` terminal command
- **Persistent blackout** triggered by East path Class III Mixed Alarm
- **MRI Machine** requiring four power cells with a three-phase extraction alarm
- **Blood Doors** in multiple zones (269, 271, 273, 275)
- **Tank screaming mechanic** preventing adjacent zone awakening
- **Alarm types:** Class VI Mixed, Class VII Mixed, Class IV Mixed, Class III Mixed, Error Alarm
- **Shared zone with R8C1** (Zone 249) with mutual Emergency Lockdowns
- Tight resource economy; Zone 276 has highest resources (200% Ammo, 240% Medi, 120% Tool)
- Terminal commands: `ALARM_DEACTIVATE`, `INITIATE_MRI`
