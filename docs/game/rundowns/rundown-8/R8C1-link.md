# R8C1 - "Link"

## Overview

- **Rundown:** Rundown 8.0: Duality
- **Tier:** C
- **Environment:** Labs (Research Environments)
- **Dimensions:** No (fog cycling mechanic, but no dimensional travel)

## Objectives

### Main Objective

**Establish Uplink (Dual Corrupted Uplinks):** Locate a terminal in Zone 241, enter `SERVER_VENTILATION`, proceed through Zone 242 (triggers fog cycling), reach Zone 249 to collect a Microdrive, enter `REQUEST_ACCESS_PROJECTINSIGHT_[1]` at the terminal, then establish 2 Corrupted Uplinks in Zones 245-248 (Server Farm 1) using specified terminals. Completion triggers an Error Alarm spawning enemies every ~30 seconds. Extraction at Zone 243 via Team Extraction Scan (all players must be standing).

### Secondary Objective

**Terminal Command (Restart Ventilation):** Use a Bulkhead Key in Zone 242 to access Zone 360, proceed to Zone 362 during a Class VII Mixed Alarm to collect a Colored Key, unlock Zone 364, and enter `RESTART_VENTILATION` at the terminal. This makes fog cycling intervals more favorable and spawns additional Shadows/Big Shadows during the Overload objective.

### Overload Objective

**Timed Sequence (Timed Connection):** Enter `REQUEST_ACCESS_PROJECTINSIGHT_[2]` in Zone 249 (locks out Main objective). Proceed through Zone 369 (Class V Sustain Mixed Alarm), then complete 3 Timed Sequences in Zones 370-373. Each sequence requires: `INIT_TIMED_CONNECTION` on a Central Terminal (3:20 timer), `VERIFY_TIMED_CONNECTION` on a random Parallel Terminal (overrides timer, gives 10-second window), then `CONFIRM_TIMED_CONNECTION` back on Central Terminal. Side zones require Full Team Scans to unlock. Completing all sequences triggers an Error Alarm. Extraction at Zone 243 via Team Extraction Scan.

Completing Overload awards Main Objective completion. Prisoner Efficiency requires Secondary + Overload only.

## Enemy Types

### Main Sector

- Striker, Shooter, Scout
- Snatcher, Giant
- Nightmare Scout (debut -- extremely high health, requires repeated melee/shooting of front and back tumors)

### Secondary Sector

- Shadow, Big Shadow, Shadow Scout

### Overload Sector

- Shadow, Big Shadow, Shadow Scout
- Charger, Nightmare Striker, Nightmare Shooter, Nightmare Scout
- Snatcher, Hybrid, Zoomer Scout
- Tank (boss)

This level features approximately 20 naturally spawning Scouts -- the highest count in the game across all Scout variants.

## Level Description

R8C1 takes place in Labs/Research Environments and introduces the Nightmare Scout enemy type, which has extremely high health and requires targeting both front and back tumors. The level features three objective tiers with increasingly complex mechanics. The Overload objective's Timed Sequence mechanic demands precise coordination between Central and Parallel terminals under tight time pressure. The fog cycling mechanic (triggered by opening Zone 242's Security Door) creates periodic visibility challenges that the Secondary objective can partially mitigate.

## Notable Mechanics

- **Fog Cycling:** Activated when Zone 242 Security Door opens; fog density changes periodically. Completing Secondary makes intervals more favorable.
- **Team Extraction Scan:** Unique extraction requiring all players standing (not downed) simultaneously within scan area.
- **Timed Sequences (Overload):** Three sequences with 3:20 timers, Central/Parallel terminal coordination, 10-second verification windows. Parallel Terminal locations change each attempt (even after checkpoint restart).
- **Nightmare Scout debut:** Extremely high health enemy requiring front and back tumor targeting.
- **~20 naturally spawning Scouts** across all variants (highest count in the game).
- **Alarm types:** Class V, Class VII Mixed, Class V Sustain Mixed, Error Alarms.
- **Enemy wave scaling during Timed Sequences:** 1st wave spawns Giants; 2nd/3rd waves add 6 Nightmare Strikers/Shooters. Snatchers spawn during 2nd sequence (~3 min intervals), Tank spawns during 3rd (~5 min intervals).
- **Full Team Scans** required to unlock side zones 371-373 during Overload.
- **Zone 242B Bug:** Left door is bugged (impassable) both ways; right door bugged for enemies only.
- **Out-of-bounds issues** in Server Farm 1 and Server Farm 2.
- Terminal commands: `SERVER_VENTILATION`, `REQUEST_ACCESS_PROJECTINSIGHT_[1]`, `REQUEST_ACCESS_PROJECTINSIGHT_[2]`, `RESTART_VENTILATION`, `INIT_TIMED_CONNECTION`, `VERIFY_TIMED_CONNECTION`, `CONFIRM_TIMED_CONNECTION`
