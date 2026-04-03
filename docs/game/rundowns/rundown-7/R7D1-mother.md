# R7D1 - "Mother"

## Overview

- **Rundown:** Rundown 7.0: Rise
- **Tier:** D
- **Environment:** Gardens
- **Dimensions:** No

## Objectives

### Main Objective

**Process Item (Timed Sequence):** Navigate through multiple zones and process a Neonate HSU through a timed connection sequence.

1. Start in Zone 191 with the Neonate HSU
2. Navigate through Zones 192-204, managing Class X and Class V alarms using terminal commands like `OVERRIDE_LOCKDOWN_ALPHA[CLASS_X]`
3. Execute four escalating Timed Sequences in Zones 194-196 while inputting `INIT_TIMED_CONNECTION` and `CONFIRM_TIMED_CONNECTION` commands
4. Extract through Zone 207

**Critical:** This is one of the few expeditions potentially impossible to complete without at least two human players, depending on the location of required terminals, due to strict timing requirements for simultaneous terminal inputs.

### Secondary Objective

None

### Overload Objective

**Terminal Command:** Input `FILTRATION_STARTUP` in Zone 312, which spawns one Snatcher every 4 minutes and creates persistent fog across the level. The overload spawns approximately 19 Snatchers over roughly 1 hour and 26 minutes.

## Enemy Types

**Common:**

- Striker
- Shooter
- Scout
- Giant
- Big Shooter
- Hybrid

**Unique:**

- Charger
- Charger Scout
- Shadow
- Snatcher

**Boss:**

- Mother
- Tank
- Baby Striker (spawned on failed sequences)

## Level Description

R7D1 is a narrative-heavy D-tier expedition centered on processing a Neonate HSU through timed connection sequences. The level makes the prisoners create a new Conduit for The Collectors to communicate through, similar to the one Schaeffer confronted in a previous rundown. The Neonate HSU undergoes visible maturation through the mission and can be seen transforming into an adult form when viewed with thermal weaponry.

The expedition features four progressive combat phases with escalating enemy difficulty, where strict timing on terminal inputs is essential. Multiple Class X alarms can be downgraded or bypassed using specific terminal commands, adding a strategic layer to the combat.

The overload objective creates an interesting long-duration challenge, spawning Snatchers at regular intervals over nearly 90 minutes while maintaining persistent fog across the entire level.

## Notable Mechanics

- **Timed Sequences:** Four progressive combat phases requiring precise terminal coordination
- **Class X Alarms:** Can be downgraded or bypassed using terminal commands like `OVERRIDE_LOCKDOWN_ALPHA[CLASS_X]`
- **Terminal Commands:** `INIT_TIMED_CONNECTION`, `CONFIRM_TIMED_CONNECTION`, `DEACTIVATE_ALARMS`, `FILTRATION_STARTUP`
- **Error Alarms:** High-spawn-rate alarms that can be deactivated via `DEACTIVATE_ALARMS`
- **Full Room Alarms:** Security scans that spawn specific enemy types during progression
- **Persistent Fog:** Overload objective creates level-wide fog limiting visibility
- **Snatcher Spawns:** Overload objective spawns ~19 Snatchers over ~86 minutes at 4-minute intervals
- **Minimum 2 Human Players:** Strict terminal timing makes solo or bot-only completion potentially impossible
- **Neonate Maturation:** The HSU visibly changes through the mission, observable with thermal weapons

## Zones

Main: 191-207
Overload: 312
