# R7C2 - "Sublimation"

## Overview

- **Rundown:** Rundown 7.0: Rise
- **Tier:** C
- **Environment:** Mines
- **Dimensions:** No

## Objectives

### Main Objective

**Activate Generator Cluster:** Place 6 Power Cells into generators in Zone 145 through three sequential ventilation sequences.

1. **First Sequence (Zones 146-150):** Execute `INIT_VENTILATION_SEQUENCE` in Zone 148, triggering a Class V Cluster Alarm. Retrieve 2 Power Cells from Zone 149 and return to the Generator Cluster.

2. **Second Sequence (Zones 151-153):** Initialize ventilation in Zone 152 (Class V Alarm), spawning continuous enemies via Error Alarm (~6 enemies every 15 seconds). Collect 2 Power Cells from Zone 153.

3. **Third Sequence (Zones 154-158):** Room Scan initiated in Zone 155 with fog rising from floor then ceiling. Retrieve final 2 Power Cells from Zone 158.

After completing all 6 Power Cell insertions, execute `COPY_GEN_INDEX_EXTENDED_DB` command in Alpha Six, then extract from Zone 144.

### Secondary Objective

**Terminal Command:** Download Extended Genome Database in Alpha Six. Accessible after completing the main objective -- Zone 144B (Blood Door) opens automatically post-main objective. Access Zones 290/291, retrieve a Matter Wave Projector from Zone 291 (contains Baby Strikers and a Mother boss). Insert the projector to return to Alpha Six. Complete a Team Scan that triggers a Class VI Alarm with Flyers.

### Overload Objective

**Gather Items:** Collect 3 OSIP Containers from Zone 303 (5 available, only 3 required). Obtain the Bulkhead Key from Zone 150. Clear Zone 302 (Spitters, Chargers, Big Chargers, Charger Scouts). Power the door to Zone 303 with a Power Cell. Survive a Class VI Mixed Alarm (alternating Team/Cluster Scans with Flyer spawns). Retrieve a replacement Power Cell from Zone 303B.

## Enemy Types

**Main Sector:**

- Striker
- Shooter
- Scout
- Giant
- Hybrid
- Flyer
- Big Flyer

**Secondary Sector:**

- Striker
- Shooter
- Baby Striker
- Flyer
- Mother (boss)

**Overload Sector:**

- Charger
- Big Charger
- Charger Scout
- Flyer
- Big Flyer

## Level Description

R7C2 is a complex multi-sector expedition in the Mines environment featuring three sequential ventilation sequences as the core gameplay loop. Each sequence escalates in difficulty, with the third introducing fog mechanics that rise from floor to ceiling. The secondary objective features an encounter with a Mother boss and introduces Matter Wave Jumping without direct Data Cube interaction. The overload sector adds Charger variants for an additional challenge layer.

Notable firsts for this level include Big Flyers debuting within the Complex environment and the first example of Matter Wave Jumping without Data Cube interaction. The secondary objective is partially completed during main objective execution, and the overload objective is recommended to be performed in alternating steps alongside the main sequence.

## Notable Mechanics

- **Infectious Fog System:** Steadily rises during alarms, descends post-completion; Fog Repellers recommended
- **Class V Cluster Alarm:** First ventilation sequence
- **Class V Alarm:** Second ventilation sequence with Error Alarm overlay
- **Room Scan with Bidirectional Fog:** Third ventilation sequence -- fog rises from floor then ceiling
- **Class VI Full Team Alarm:** Secondary objective completion
- **Class VI Mixed Alarm:** Overload objective -- alternating Team/Cluster Scans with Flyer spawns
- **Error Alarm:** Spawns 6 enemies every ~15 seconds during second sequence
- **Power Cell Management:** 6 cells mandatory for main objective; additional cells needed for overload access
- **Checkpoint Mechanic:** Using checkpoints causes fog to lower at 5 rather than 6 cells inserted, significantly altering level progression
- **Matter Wave Projector:** Used in secondary objective for dimensional transport
- **Mother Boss:** Encountered in Zone 291 during secondary objective
