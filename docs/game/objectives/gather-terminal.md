# Gather Terminal Objective

## Overview

**Gather Terminal** (also known as **Gather Terminal Data** or **Collect Terminal Logs**) is an information-gathering objective in GTFO that requires players to access specific terminals throughout the Complex and collect data from terminal logs. Unlike physical item collection, this objective involves navigating to designated terminals, reading log files, and extracting critical information. This objective emphasizes terminal proficiency, zone navigation, and systematic exploration.

## What is Terminal Data Collection?

Terminal data collection involves accessing computer terminals scattered throughout the Complex and reading log files that contain mission-critical information. These logs may contain verification codes, zone information, equipment locations, or story/lore content. Players must use terminal commands to list available logs, read specific files, and gather required information to progress the mission.

## Objective Flow

### Phase 1: Identify Required Terminals

1. **Check Warden Objective**: Review which terminal data must be collected
2. **Terminal Count**: Note how many terminals require access
3. **Zone Identification**: Determine which zones contain required terminals
4. **Resource Gathering**: Collect supplies for zone navigation
5. **Route Planning**: Plan efficient path through required zones

### Phase 2: Navigate to Terminals

1. **Zone Progression**: Travel through Complex to terminal zones
2. **Terminal Location**: Find terminals within zones (use PING if needed)
3. **Clear Area**: Eliminate enemies around terminals before accessing
4. **Secure Position**: Ensure safe access to terminal
5. **Access Terminal**: Approach and interact with terminal

### Phase 3: Access Terminal Logs

1. **List Logs**: Use `LOGS` command to display available log files
2. **Identify Required Logs**: Determine which logs contain needed information
3. **Read Logs**: Use `READ [FILENAME]` to view log contents
4. **Extract Information**: Note verification codes, locations, or other data
5. **Verify Collection**: Confirm information gathered correctly

### Phase 4: Complete Collection and Extract

1. **Visit All Required Terminals**: Access each designated terminal
2. **Gather All Data**: Collect all required information from logs
3. **Verify Completion**: Confirm all terminal data collected
4. **Continue Mission**: Proceed to next objective or extraction
5. **Final Extraction**: Navigate to extraction point

## Core Mechanics

### Terminal Log System

**LOGS Command:**
- Lists all available log files on current terminal
- Shows filenames and extensions
- Every terminal contains at least `auto_gen_status.log` (non-useful generic log)
- Mission-critical logs have descriptive filenames

**READ Command:**
- Displays contents of specific log file
- Format: `READ [FILENAME.EXT]` (e.g., `READ status.log`)
- Log contents appear on terminal screen
- May contain codes, locations, lore, or instructions

### Log File Types

**Mission-Critical Logs:**
- Verification codes for uplinks or reactors
- Equipment locations and zone information
- Access codes for locked systems
- Progression-critical data

**Lore Logs:**
- Story and world-building content
- Character communications and reports
- Historical information about the Complex
- Optional but provides context

**Generic Logs:**
- `auto_gen_status.log` - Default log with no useful information
- System status logs with standard information

### Information Gathering

**Code Extraction:**
- Verification codes for Terminal Uplink objectives
- Access codes for locked terminals or doors
- Reactor verification passwords
- Equipment activation codes

**Location Data:**
- Zone information for objectives
- Equipment locations
- Key locations
- Resource positions

**Instructions:**
- Procedural steps for objectives
- Equipment usage instructions
- Security protocols
- Mission parameters

## Recommended Strategy

### Preparation Phase

1. **Terminal Survey**: Use PING to locate terminals if needed
2. **Map Terminal Zones**: Identify which zones contain required terminals
3. **Route Optimization**: Plan path visiting all terminals efficiently
4. **Resource Check**: Ensure ammunition and supplies adequate
5. **Clear Strategy**: Decide whether to clear zones before terminal access

### Zone Navigation

1. **Systematic Approach**: Visit terminals in logical order
2. **Zone Clearing**: Clear enemies before accessing terminals (preferred)
3. **Stealth Option**: In some cases, stealth to terminal possible
4. **Team Coordination**: Designate reader if team splitting
5. **Terminal Security**: Ensure area safe before reading logs

### Terminal Access

1. **Approach Terminal**: Navigate to terminal location
2. **Secure Position**: Clear immediate threats
3. **List Logs**: Type `LOGS` to see available files
4. **Identify Targets**: Find mission-relevant log files
5. **Read Files**: Use `READ [FILENAME]` for each required log
6. **Extract Data**: Note codes, locations, or information
7. **Verify Reading**: Confirm all required logs accessed

### Information Management

1. **Communication**: Share discovered information with team verbally
2. **Code Recording**: Memorize or communicate codes immediately
3. **Location Notes**: Note important location data
4. **Cross-Reference**: Verify information matches objective requirements
5. **Backup Reader**: Have second player verify critical information

### Collection Completion

1. **Terminal Checklist**: Track which terminals have been accessed
2. **Data Verification**: Confirm all required information gathered
3. **Mission Progression**: Use gathered data for subsequent objectives
4. **Proceed to Next Phase**: Continue mission with collected information

## Terminal Commands

### Log Access Commands
- `LOGS` - List all available log files on current terminal
- `READ [FILENAME]` - Read specific log file (e.g., `READ verification.log`)

### Navigation Commands
- `PING` - Locate terminal zones or objectives
- `LIST` - Show items and objectives
- `QUERY` - Find specific item or objective locations

### Related Commands
- `UPLINK_VERIFY` - Uses codes often found in terminal logs
- `REACTOR_VERIFY` - Uses codes sometimes retrieved from logs

## Notable Expeditions

Terminal log gathering is often integrated into other objectives:

### Terminal Uplink (Advanced Variant)
- Verification codes stored in terminal logs in other zones
- Requires accessing multiple terminals to gather codes

### Reactor Startup/Shutdown (Variant)
- Some expeditions require log file code retrieval
- Codes in terminals rather than HUD display

### Lore Collection
- D-Lock Block Decipherer Steam Achievement for collecting all lore logs
- Optional exploration and story immersion

See [Logs - Official GTFO Wiki](https://gtfo.fandom.com/wiki/Logs) for comprehensive log information.

## Comparison to Related Objectives

| Aspect | Gather Terminal Data | Gather Small Items | Terminal Uplink |
|--------|---------------------|-------------------|-----------------|
| **Collection Type** | Virtual data/information | Physical items | Network verification |
| **Pickup Method** | Read terminal logs | Pick up from containers | Enter codes at terminal |
| **Movement Impact** | None | None | None |
| **Location Method** | Navigate to terminals | LIST/QUERY/PING | Find uplink terminals |
| **Item Physicality** | Non-physical (data) | Physical containers | Non-physical (codes) |
| **Terminal Dependency** | High (primary mechanic) | Moderate (location aid) | High (command entry) |
| **Combat During Access** | Risk while reading | Risk while collecting | High (alarm during uplink) |
| **Quota System** | Specific terminals/logs | Item count quota | Verification stages |
| **Objective Integration** | Often part of larger objective | Standalone objective | Standalone objective |

## Common Challenges

### Terminal Location

- **Challenge**: Finding required terminals in large or complex zones
- **Solution**: Use PING, systematic zone exploration, map knowledge

### Reading Under Pressure

- **Challenge**: Trying to read logs while under attack
- **Solution**: Clear area before accessing terminal, designate protected reader

### Code Memory

- **Challenge**: Forgetting codes or information after reading logs
- **Solution**: Communicate codes immediately, have team memorize together, use in-game notes if available

### Multiple Terminals

- **Challenge**: Tracking which terminals accessed and which remain
- **Solution**: Mental checklist, team communication, systematic zone progression

### Log File Identification

- **Challenge**: Determining which log files contain required information
- **Solution**: Read filenames carefully, check all non-generic logs, use descriptive log names as hints

### Zone Navigation

- **Challenge**: Navigating dangerous zones just to access terminals
- **Solution**: Clear zones methodically, use stealth when possible, team provides cover

### Information Application

- **Challenge**: Not understanding how to use gathered information
- **Solution**: Review objective requirements, cross-reference with mission goals, communicate with team

## Tips

- **LOGS First**: Always type `LOGS` before trying to read files to see what's available
- **READ Syntax**: Remember format `READ [FILENAME.EXT]` including extension
- **Clear Before Read**: Always clear terminal area before accessing logs
- **Generic Ignore**: Skip `auto_gen_status.log` - it contains no useful information
- **Descriptive Names**: Log filenames often hint at contents (verification.log, codes.log, etc.)
- **Communicate Immediately**: Share discovered codes/info with team right away
- **Systematic Approach**: Visit terminals in logical order to avoid backtracking
- **Team Reader**: Designate one player for reading while others provide security
- **Zone Clearing**: Pre-clear zones before terminal access when possible
- **PING Terminals**: Use PING if terminals difficult to locate in zone
- **Verification Cross-Check**: Have second player verify critical codes/information
- **Read All Relevant**: Don't assume first log has all info - check all mission-relevant logs
- **Lore Optional**: Most lore logs are optional unless specifically required
- **Achievement Tracking**: Use external tools for D-Lock Block Decipherer achievement
- **Log File Persistence**: Log contents don't change during mission - no need to re-read
- **Terminal Safety**: Ensure terminal area secure before spending time reading
- **Code Application**: Know where/how to use codes before gathering them
- **Backup Memory**: Multiple players memorizing codes reduces error risk
- **Route Planning**: Plan terminal route to minimize dangerous zone exposure
- **Information Priority**: Focus on mission-critical data before optional lore

## Sources

- [Logs - Official GTFO Wiki](https://gtfo.fandom.com/wiki/Logs)
- [Terminal - Official GTFO Wiki](https://gtfo.fandom.com/wiki/Terminal)
- [Terminal - Official GTFO Wiki (wiki.gg)](https://gtfo.wiki.gg/wiki/Terminal)
- [Terminal Command - Official GTFO Wiki](https://gtfo.wiki.gg/wiki/Terminal_Command)
- [GTFO: Using Terminals (Tips & Tricks) - Screen Rant](https://screenrant.com/gtfo-using-terminals-tips-tricks/)
- [GTFO All Lore Logs - Steam Community](https://steamcommunity.com/sharedfiles/filedetails/?id=3111536188)
- [Timeline - Official GTFO Wiki](https://gtfo.wiki.gg/wiki/Timeline)
