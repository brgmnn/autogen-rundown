# R8B4 - "Insurrection"

## Overview

- **Rundown:** Rundown 8.0: Duality
- **Tier:** B (4th in tier)
- **Environment:** Contaminated industrial facilities
- **Dimensions:** No

## Objectives

### Main Objective

**Retrieve Item:** Retrieve Datasphere 741. Begin in Zone 155 by executing `COMMENCE_SCAN` at a terminal, initiating a Team Traveling Scan. At ~15% scan progress, input `PM_VENT_PROTOCOL` to ventilate Infectious Fog. Enter Zone 159 (Class V Alarm) where a Mother boss spawns after scan completion. Optional Blood Door in Zone 157 provides extra resources but awakens numerous enemies.

A second Traveling Scan occurs in Zone 159 using a Locked Terminal (password from Zones 160/161). Three sequential fog-ventilation commands: `PM_VENT_START`, `PM_VENT_PATH`, `PM_VENT_EXECUTE`. This Red Traveling Scan progresses faster with more players but doesn't require all present.

Zone 162 serves as a checkpoint. Retrieve the Datasphere, collect a Bulkhead Key, and open the Main Objective Bulkhead Door to Zone 163, triggering an Error Alarm (1 Giant + 6 Strikers every ~10 seconds, escalating over 8 minutes, enemies spawn 2 rooms away toward Zone 155).

Locate a Power Cell in Zone 165 (always in Zone 165C on a back wall data rack shelf) and insert into a Generator. Zone 166 contains a sleeping Mother with randomized location. A second Power Cell is randomly in Zone 167 or 168. Extract with the Datasphere from Zone 169.

### Secondary Objective

**Reactor Shutdown:** Enter Zone 294 and proceed to Zone 295 for the Reactor Terminal. Input `LOWER_RODS` to initiate a Class II Megacluster Surge Alarm (spawns Strikers, Shooters, Hybrids). After the initial Team Scan, 25 Cluster Scans deploy throughout the zone. Upon completion, the door to Zone 296 unlocks. Infectious Fog begins rising to maximum levels. An Error Alarm initiates (3 Nightmare Strikers/Shooters every 20 seconds for the rest of the expedition), stacking with the Main Error Alarm. Collect Bulkhead Key from Zone 296. Optionally return to Zone 155 and execute `PM_VENT_PROTOCOL` to halt fog rise.

### Overload Objective

None

## Enemy Types

- Striker, Shooter, Giant, Big Shooter, Hybrid, Scout
- Flyer, Baby Striker
- Zoomer Scout, Snatcher
- Nightmare Striker, Nightmare Shooter
- Mother (boss, spawns in Zone 159 and randomized in Zone 166)

## Level Description

Insurrection is centered on retrieving Datasphere 741 amid abundant Infectious Fog with scarce cleansing resources. The expedition name references an uprising -- the story involves conflict between The Warden and Schaeffer over control of BIOCOM and Datasphere 741. Klas Henrikson's previous warning about reactor meltdown and prisoners needing to remove control chips contextualizes the Secondary Objective.

If players complete only the Main Objective, Schaeffer becomes hostile expressing desperation for control. If completing the Secondary Objective, Schaeffer directs prisoners to "get out of there, quickly," implying cooperation. In both scenarios, the Reactor Core Control Rods lower and Schaeffer loses Objective Stack control, allowing The Warden to succeed in Datasphere retrieval for subsequent missions.

## Notable Mechanics

- **Infectious Fog** with scarce disinfection resources throughout
- **Traveling Scans:** Orange Team Traveling Scans (require all prisoners) and Red Traveling Scans (faster with more players, don't require all)
- **Dual stacking Error Alarms** if both Main and Secondary objectives are active
- **Class II Megacluster Surge Alarm** with 25 Cluster Scans
- **Class V Alarm** in Zone 159 upon Mother spawn
- **Error Alarm escalation** over ~8 minutes, doubling spawn amounts
- **Fog ventilation terminal commands:** `COMMENCE_SCAN`, `PM_VENT_PROTOCOL`, `PM_VENT_START`, `PM_VENT_PATH`, `PM_VENT_EXECUTE`, `LOWER_RODS`
- **Blood Door** in Zone 157 (optional, awakens Baby Strikers, Strikers, Shooters, Giants, Nightmare variants)
- **Checkpoint bug:** Restarting from Zone 162 checkpoint is bugged in co-op, causing Error Alarm enemies to spawn from any direction 2 rooms away instead of consistently from spawn elevator direction
