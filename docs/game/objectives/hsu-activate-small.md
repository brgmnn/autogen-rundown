# HSU Activate Small Objective

## Overview

**HSU Activate Small** (also known as **Process Neonate** or **Activate Neonate HSU**) is an objective in GTFO that requires players to locate a Neonate HSU (small Hydro Stasis Unit containing an infant or child specimen) and activate or resuscitate it using specialized equipment. Players must use terminals to locate both the neonate HSU and the required activation device, then successfully complete the activation sequence while managing any alarm consequences.

## What is a Neonate HSU?

A **Neonate HSU** is a smaller version of a regular HSU that contains an infant or child specimen preserved in amniotic fluid. Unlike standard HSUs that may simply require tissue sample extraction, Neonate HSUs in activation objectives must be processed or resuscitated using specialized equipment found elsewhere in the Complex.

### Key Characteristics

- **Size**: Smaller than standard HSUs
- **Contents**: Infant or child specimen
- **Objective Type**: Activation/processing rather than retrieval or sampling
- **Equipment Required**: Specialized devices (NCR, HydroStasis Depressurizer, etc.)

## Objective Flow

### Phase 1: Locate the Neonate HSU

1. **Check Warden Objective**: Review which neonate HSU must be activated
2. **Terminal Query**: Use `QUERY [HSU_NAME]` to find HSU location
3. **Navigate to Zone**: Travel through Complex to HSU's zone
4. **PING for Precision**: Use `PING [HSU_NAME]` from zone terminal for exact location
5. **Locate HSU**: Find the specific neonate HSU unit

### Phase 2: Locate Activation Equipment

1. **Terminal Search**: Use terminals to find activation device location
2. **Common Equipment Types**:
   - **NCR (Neurogenic Cardiac Resuscitator)**: Device for resuscitating neonate
   - **HydroStasis Depressurizer**: Device for depressurizing stasis units
   - Other specialized processing equipment
3. **Query Equipment**: Use `QUERY [DEVICE_NAME]` to find device zone
4. **Navigate to Equipment**: Travel to device location
5. **Retrieve Equipment**: Pick up or note equipment location

### Phase 3: Prepare for Activation

1. **Resource Check**: Ensure team has ammunition and consumables
2. **Clear HSU Area**: Eliminate enemies around neonate HSU
3. **Defensive Setup**: Position sentries and establish defensive positions
4. **Prepare for Alarm**: Expect activation to trigger enemy spawns
5. **Coordinate Team**: Assign roles for activation sequence

### Phase 4: Activate/Process Neonate HSU

1. **Bring Equipment to HSU**: Transport activation device to neonate location (if required)
2. **Initiate Activation**: Interact with HSU using specialized equipment
3. **Activation Sequence**: Complete required activation steps
4. **Bioscan (Possible)**: May require bioscan during activation
5. **Alarm Management**: Survive enemy waves triggered by activation
6. **Completion**: Successfully complete activation process

### Phase 5: Extraction

1. **Proceed to Next Objective**: Continue mission after activation complete
2. **Navigate to Extraction**: Travel to extraction point
3. **Final Extraction**: Complete final extraction scan

## Core Mechanics

### Neonate HSU Activation

**Activation Process:**
- Requires specific equipment (NCR, Depressurizer, etc.)
- Equipment location found via terminal commands
- May involve bioscan sequences during activation
- Commonly triggers alarm on activation

**Equipment Interaction:**
- Some equipment must be carried to HSU
- Other equipment may be used in place
- Activation typically irreversible once started

### Terminal Navigation

**Finding Neonate HSU:**
- Use `QUERY [HSU_NAME]` to locate zone
- HSU format: HSU_### (e.g., HSU_123)
- Use `PING` from zone terminal for directional guidance

**Finding Activation Equipment:**
- Equipment has specific names (NCR_A02, etc.)
- Use `QUERY [DEVICE_NAME]` to locate
- May be in different zone than neonate

### Alarm Mechanics

**Activation Alarms:**
- Activation commonly triggers error alarm
- Enemy waves spawn during or after activation
- Alarm type varies by expedition
- Must defend HSU area during activation

**Bioscan Defense:**
- Some activations require bioscan completion
- Team must hold position during scan
- Vulnerable to enemy attacks during process

## Recommended Strategy

### Preparation Phase

1. **Terminal Reconnaissance**:
   - `QUERY [HSU_NAME]` to find neonate location
   - `QUERY [DEVICE_NAME]` to find activation equipment
   - Map both locations and plan route
2. **Route Planning**: Determine most efficient path between equipment and HSU
3. **Clear Both Areas**: Pre-clear enemies from HSU and equipment zones
4. **Resource Stockpile**: Top off ammunition before activation
5. **Team Roles**:
   - **Equipment Carrier** (if needed): Transport device to HSU
   - **Activator**: Perform activation sequence
   - **Defenders**: Protect during activation

### Equipment Retrieval

1. **Locate Device**: Navigate to equipment zone using terminal guidance
2. **Clear Area**: Eliminate threats around equipment
3. **Retrieve Equipment**: Pick up or note equipment location
4. **Transport to HSU** (if required): Carry device to neonate location
5. **Maintain Protection**: Team escorts equipment carrier if needed

### Activation Phase

1. **Position Team**: Establish defensive formation around HSU
2. **Sentry Placement**: Position 2-4 sentries covering approach routes
3. **C-Foam Doors**: Reinforce entry points to HSU area
4. **Initiate Activation**: Begin activation sequence
5. **Defend Position**: Hold area during activation/bioscan
6. **Focus Fire**: Prioritize high-threat enemies (Giants, Chargers)
7. **Complete Sequence**: Finish activation process

### Post-Activation

1. **Clear Remaining Enemies**: Eliminate spawned enemies
2. **Resource Check**: Assess remaining ammunition and consumables
3. **Proceed to Next Objective**: Continue mission
4. **Prepare for Extraction**: Ready for final extraction sequence

## Terminal Commands

- `QUERY [HSU_NAME]` - Find zone location of neonate HSU (e.g., `QUERY HSU_123`)
- `QUERY [DEVICE_NAME]` - Find activation equipment location (e.g., `QUERY NCR_A02`)
- `PING [HSU_NAME]` - Directional audio for HSU in current zone
- `PING [DEVICE_NAME]` - Directional audio for equipment in current zone
- `LIST RESOURCES` - Find ammo and health stations

## Notable Expeditions

Various expeditions feature Neonate HSU activation objectives with different equipment and challenges.

See [Neonate HSU - Official GTFO Wiki](https://gtfo.wiki.gg/wiki/Neonate_HSU) for information about neonate-related objectives.

## Comparison to Related Objectives

| Aspect | HSU Activate Small | HSU Find Sample | Retrieve Big Item (Neonate) |
|--------|-------------------|-----------------|---------------------------|
| **Primary Action** | Activate/process neonate | Extract tissue sample | Transport to extraction |
| **HSU Type** | Neonate (small) | Standard (large) | Neonate (small) |
| **Equipment Required** | Activation device (NCR, etc.) | None | None |
| **Item Carry** | Possibly equipment | No | Yes (neonate itself) |
| **Terminal Usage** | Find HSU and equipment | Find target HSU | Find neonate location |
| **Activation Needed** | Yes (primary objective) | No | No |
| **Transport Needed** | Equipment to HSU (sometimes) | No | Neonate to extraction |
| **Alarm Timing** | At activation | At bioscan | At pickup/extraction |
| **Bioscan** | Often required | Required at HSU | Required at extraction |
| **Location Type** | Neonate in specific zone | Any HSU type | Neonate in specific zone |

## Common Challenges

### Equipment Location

- **Challenge**: Finding activation equipment in distant or dangerous zones
- **Solution**: Use terminal commands efficiently, plan route, clear path beforehand

### Two-Location Objective

- **Challenge**: Must navigate between equipment and HSU locations
- **Solution**: Clear both areas first, then perform activation with prepared defenses

### Activation Alarm

- **Challenge**: Activation triggers intense enemy spawns while team vulnerable
- **Solution**: Strong sentry setup, C-Foam reinforcement, prepare before activating

### Bioscan Vulnerability

- **Challenge**: Being stationary during activation bioscan with enemies spawning
- **Solution**: Defensive perimeter, focus fire on priorities, Bio Tracker tagging

### Equipment Transport

- **Challenge**: Carrying equipment to HSU while vulnerable (if required)
- **Solution**: Clear path first, team escorts carrier, drop equipment for combat if needed

### Zone Navigation

- **Challenge**: Equipment and HSU may be in different zones requiring extensive travel
- **Solution**: Plan efficient route, clear zones methodically, conserve resources

## Tips

- **Query Both First**: Use terminals to locate both HSU and equipment before moving
- **Clear Before Activate**: Never activate until area is fully cleared and defended
- **Sentry Setup**: Position sentries before activation, covering all approaches
- **Bioscan Ready**: Expect bioscan requirement, prepare defensive positions
- **Equipment Priority**: Locate equipment early, even if not needed until later
- **Communication**: Call out when activation begins so team can prepare
- **Resource Check**: Top off ammo before activation sequence
- **C-Foam Doors**: Reinforce HSU area entrances before activating
- **Bio Tracker Essential**: Tag spawning enemies for team and sentry awareness
- **Defensive Formation**: Establish perimeter around HSU before starting
- **PING for Precision**: Use PING to locate exact HSU and equipment positions
- **Clear Path Between**: If equipment must be transported, clear route first
- **Activation Timing**: Coordinate activation start with team readiness
- **Focus Fire**: Prioritize Giants, Chargers, Shooters during activation defense
- **Don't Rush**: Take time to prepare properly - activation can't be undone
- **Terminal Memory**: Note which terminals have information about HSU and equipment
- **Zone Clearing**: Clear surrounding zones to reduce reinforcement spawns
- **Extraction Prep**: Be ready to move to extraction quickly after activation
- **Equipment Names**: Remember or note exact equipment names for queries (NCR_A02, etc.)
- **Alarm Expected**: Assume activation will trigger alarm, prepare accordingly

## Sources

- [Neonate HSU - Official GTFO Wiki](https://gtfo.wiki.gg/wiki/Neonate_HSU)
- [Neonate HSU - Official GTFO Wiki (Fandom)](https://gtfo.fandom.com/wiki/Neonate_HSU)
- [HSU - Official GTFO Wiki](https://gtfo.wiki.gg/wiki/HSU)
- [HSU - Official GTFO Wiki (Fandom)](https://gtfo.fandom.com/wiki/HSU)
- [List of Expedition Objectives - Official GTFO Wiki](https://gtfo.fandom.com/wiki/List_of_Expedition_Objectives)
- [Retrieve HSU - Official GTFO Wiki](https://gtfo.fandom.com/wiki/Retrieve_HSU)
