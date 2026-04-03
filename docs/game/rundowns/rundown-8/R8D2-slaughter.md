# R8D2 - "Slaughter"

## Overview

- **Rundown:** Rundown 8.0: Duality
- **Tier:** D
- **Environment:** Industrial canal/drainage system
- **Dimensions:** No

## Objectives

### Main Objective

**Clear a Path:** "Find Schaeffer." Navigate from Zone 33 through Zone 52, culminating in extraction from Zone 45. Key phases:

1. **Zones 33-35:** Start area with heavy Nightmare presence. Optional Zone 35 (C-Foam Storage) accessed via Colored Key.
2. **Zone 36 (Security Station):** Blood Door. Execute `LOWER_SECURITY_CLASS_ZONE_37` to reduce Zone 37's alarm from Class XII to Class IV.
3. **Zone 37:** Class IV Mixed Alarm (post-reduction). Collect Bulkhead Key.
4. **Zones 38-39:** Checkpoint. Optional Supply Depot with sleeping Tank.
5. **Zones 40-42:** Voice Recorder interaction triggers Error Alarm with optional floodgate terminals in Zones 37/38/39. Blackout sequence requiring ~113 seconds of defense.
6. **Zone 43 (Main Canal):** Mega Mother boss encounter. No return to previous zones after this point.
7. **Zone 46:** Execute `BYPASS_LOCKDOWN`, initiating Dual Scan requiring split team.
8. **Zone 52:** Execute `ACTIVATE_DRAINAGE_MICROPHONE` and `FLOOD_DRAINAGE`.
9. **Zone 45 (Extraction):** Transport Matter Wave Projector (MWP) while managing Surge Alarm of Nightmare Strikers and Snatchers.

### Secondary Objective

**Terminal Command:** Infectious fog management via ventilation. Access Zone 445 using Fog Turbine (contains Nightmare Strikers, Shooters, two Nightmare Scouts). In Zone 446, execute `OVERRIDE_QUARANTINE_LOCKDOWN` to unlock three ventilation tiers:

- **Moderate** (`FORCE_VENTILATION_MODERATE`): Class V Alarm; removes 30% fog
- **High** (`FORCE_VENTILATION_HIGH`): Class VII Alarm; removes 69% fog with Nightmare spawns
- **Extreme** (`FORCE_VENTILATION_EXTREME`): Class IX S Alarm; removes 99% fog; spawns Tank, Hybrids, Nightmares, possible Snatcher

Zone 447 dead-end confirms objective completion.

### Overload Objective

None

## Enemy Types

- Striker, Shooter, Giant, Hybrid
- Nightmare Striker (frequent throughout), Nightmare Shooter, Nightmare Scout (multiple instances)
- Zoomer Scout (Zone 34)
- Snatcher (Surge Alarm and extreme secondary)
- Flyer (Zone 40+ sequences)
- Tank (boss, sleeping in Zones 34 and 39; actively spawned in extreme secondary)
- Mega Mother (boss, Zone 43, tumor-based combat)

**Special Scout Behavior:** Any Scouts alerted during this expedition spawn waves of purely Nightmare Strikers and Nightmare Shooters rather than mixed enemy types.

## Level Description

R8D2 is the penultimate mission of GTFO, a grueling expedition through industrial canal and drainage systems to find Schaeffer. The level features some of the highest alarm classes in the game (up to Class XII before reduction) and culminates in a Mega Mother boss fight in the Main Canal. The final extraction requires transporting the Matter Wave Projector through a Surge Alarm while fending off Nightmare Strikers and Snatchers. Extended dialogue sequences occur during drainage system activation, advancing the story toward the game's conclusion.

## Notable Mechanics

- **Class XII Alarm** reducible to Class IV via terminal command -- strongly recommended to reduce
- **Mega Mother boss** in Zone 43 with tumor-based combat and spawned enemy waves
- **Dual Scan** in Zone 46 requiring team to split
- **Matter Wave Projector transport** during extraction Surge Alarm (limited to single carrier)
- **Blackout sequence** (~113 seconds of defense in darkness)
- **Floodgate terminals** (optional, prevent specific enemy spawns during Error Alarm)
- **Three-tier fog ventilation** with escalating difficulty and fog removal (30%/69%/99%)
- **Scout alert behavior** spawns pure Nightmare waves instead of mixed types
- **Blood Doors** requiring alarm completion
- **Persistent Infectious Fog** manageable through Secondary objective
- Terminal commands: `LOWER_SECURITY_CLASS_ZONE_37`, `BYPASS_LOCKDOWN`, `ACTIVATE_DRAINAGE_MICROPHONE`, `FLOOD_DRAINAGE`, `OVERRIDE_QUARANTINE_LOCKDOWN`, `FORCE_VENTILATION_MODERATE/HIGH/EXTREME`
- No return to previous zones after Zone 43
