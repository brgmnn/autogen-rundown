# Terminal Uplink Objective

## Overview

**Terminal Uplink** (also known as **Establish Uplink**) is an objective in GTFO that requires players to locate specific terminals and establish network uplinks to designated IP addresses using verification codes. Players must navigate through multiple verification stages, entering correct codes within time limits while managing alarm sequences and enemy waves. This objective combines terminal proficiency, team coordination, and combat capability.

## What is Terminal Uplink?

Terminal Uplink involves connecting to remote network systems by entering verification credentials at designated terminals within the Complex. Players must use special UPLINK commands to initiate connections and verify their access through multiple authentication stages. Each uplink requires precise code entry while managing the security consequences of unauthorized network access.

## Objective Flow

### Phase 1: Locate Uplink Terminals

1. **Check Warden Objective**: Review which IP address(es) require uplink connection
2. **Terminal Navigation**: Use standard terminal commands to locate uplink terminals
3. **Zone Reconnaissance**: Scout zones containing required terminals
4. **Resource Gathering**: Collect ammunition and supplies along the path
5. **Clear Terminal Area**: Eliminate enemies around target terminals

### Phase 2: Initiate Uplink Connection

1. **Access Terminal**: Reach the designated uplink terminal
2. **Note IP Address**: Verify IP address shown on HUD (format: **.**.**.**)
3. **Prepare for Alarm**: Position team for alarm triggered by connection
4. **Execute UPLINK_CONNECT**: Enter command `UPLINK_CONNECT [IP_ADDRESS]`
5. **Alarm Triggers**: Connection initiates alarm sequence

### Phase 3: Verification Stages

1. **Receive Verification Code**: Terminal displays 3-letter code (e.g., X04, Y03, Z12)
2. **Match to HUD Password**: Find corresponding 4-letter word on HUD
3. **Enter Verification**: Use `UPLINK_VERIFY [PASSWORD]` command
4. **Multiple Stages**: Complete verification for each stage (typically 3-5 stages)
5. **Wrong Code Penalty**: Incorrect verification resets current stage

### Phase 4: Code Retrieval Variant (Advanced)

**Standard Variant:**

- Verification codes displayed on HUD for whole team
- Straightforward code matching and entry

**Advanced Variant (Log File Codes):**

- Codes sent to log files in other terminals
- Must navigate to different zones to retrieve codes
- Use `LOGS` and `READ [FILENAME]` commands to access codes
- Introduced in Rundown 5 (005://EXT)

### Phase 5: Complete Uplink and Extract

1. **Final Verification**: Complete last verification stage
2. **Uplink Established**: Network connection successfully established
3. **Alarm Management**: Survive ongoing alarm sequences
4. **Continue Mission**: Proceed to next objective or extraction
5. **Final Extraction**: Navigate to extraction point

## Core Mechanics

### UPLINK Commands

**UPLINK_CONNECT [IP_ADDRESS]:**

- Initiates uplink to specified IP address
- Format: `UPLINK_CONNECT 127.0.0.1` (example)
- Can only be used on IP address provided in objective
- Triggers alarm sequence immediately
- Begins verification stage sequence

**UPLINK_VERIFY [PASSWORD]:**

- Enters 4-letter verification password
- Must match code shown on terminal
- Format: `UPLINK_VERIFY DART` (example)
- Incorrect password resets current stage
- Tab completion available

### Verification System

**Code Matching:**

- Terminal displays 3-letter codes (X04, Y03, Z12, etc.)
- HUD displays corresponding 4-letter passwords
- Player must match code to password and verify

**Example:**

```
Terminal shows: X04
HUD shows: X04 - DART
Command: UPLINK_VERIFY DART
```

**Multiple Stages:**

- Each uplink requires multiple verification stages (typically 3-5)
- Each stage presents new code
- Must complete all stages to establish uplink
- Progress tracked on HUD

### Alarm Mechanics

**Initial Connection Alarm:**

- UPLINK_CONNECT triggers alarm immediately
- Enemy waves spawn during verification process
- Team must defend terminal area while verifying

**Continuous Pressure:**

- Alarms may persist through entire uplink process
- Error alarms cannot be deactivated
- Must balance combat with code entry

### Code Retrieval Variants

**Standard (HUD Display):**

- All codes visible on team HUD
- Immediate access to passwords
- Focus remains on defense and code entry
- Lower complexity, higher combat focus

**Advanced (Log File Retrieval):**

- Codes stored in terminal log files in other zones
- Must use `LOGS` command to list available logs
- Use `READ [FILENAME]` to access code files
- Requires zone navigation during uplink sequence
- May require team splitting or solo code runs
- Adds time pressure and navigation risk

## Recommended Strategy

### Preparation Phase

1. **Terminal Scouting**: Use PING to locate uplink terminal zones
2. **Clear Terminal Area**: Eliminate all enemies around uplink terminal before starting
3. **Resource Check**: Ensure ammunition and consumables topped off
4. **Sentry Placement**: Position 2-4 sentries covering terminal and approaches
5. **Defensive Setup**: Establish defensive positions around terminal
6. **C-Foam Doors**: Reinforce entry points to terminal area
7. **Code Zone Scouting** (Advanced Variant): Scout log file terminal locations beforehand

### Uplink Initiation

1. **Team Ready Check**: Confirm all players positioned and ready
2. **Pre-type Command**: Have `UPLINK_CONNECT ` ready before entering IP
3. **Execute Connection**: Enter full command when team ready
4. **Immediate Defense**: Engage spawning enemies immediately
5. **Terminal Access**: Designate player responsible for code entry

### Verification Process

**Standard Variant:**

1. **Read Terminal Code**: Note 3-letter code on terminal (e.g., X04)
2. **Match to HUD**: Find corresponding 4-letter password on HUD
3. **Pre-type Verify**: Have `UPLINK_VERIFY ` ready
4. **Enter Password**: Type complete command quickly
5. **Repeat**: Continue for all verification stages

**Advanced Variant (Log Files):**

1. **Check Terminal**: Use `LOGS` command to list log files
2. **Read Code File**: Use `READ [FILENAME]` to view code
3. **Code Runner**: Designate fast player for code retrieval if in other zones
4. **Navigate to Zone**: Runner travels to log file terminal location
5. **Retrieve Code**: Access terminal, read log file, note code
6. **Return or Relay**: Communicate code to team or return to uplink terminal
7. **Enter Verification**: Input code at uplink terminal
8. **Repeat**: Continue for all stages

### Combat Management

1. **Defender Roles**: 2-3 players focus exclusively on defense
2. **Terminal Operator**: 1 player handles code entry (protected position)
3. **Code Runner** (if needed): 1 player retrieves log file codes
4. **Priority Targets**: Focus Giants, Chargers, Shooters first
5. **Sentry Maintenance**: Protect and repair sentries during sequence
6. **Bio Tracker**: Tag enemies constantly for sentry targeting

### Advanced Variant Strategy

1. **Pre-Scout Zones**: Identify log file terminal locations before starting uplink
2. **Clear Code Paths**: Pre-clear routes to log file terminals
3. **Fast Runner**: Designate fastest/most skilled player for retrieval
4. **Defensive Hold**: Remaining team holds uplink terminal position
5. **Communication**: Constant callouts of codes, threats, and runner status
6. **Terminal Memory**: Memorize which terminals contain which codes

## Terminal Commands

### Uplink-Specific Commands

- `UPLINK_CONNECT [IP_ADDRESS]` - Initiate uplink to specified IP (e.g., `UPLINK_CONNECT 127.0.0.1`)
- `UPLINK_VERIFY [PASSWORD]` - Enter 4-letter verification password (e.g., `UPLINK_VERIFY DART`)

### Log File Commands (Advanced Variant)

- `LOGS` - List all available log files on current terminal
- `READ [FILENAME]` - Read specific log file (e.g., `READ verification.log`)

### Standard Navigation Commands

- `PING` - Locate terminal zones
- `LIST` - Show items and objectives
- `QUERY` - Find specific item or objective locations

## Notable Expeditions

Terminal Uplink appears in various expeditions across multiple rundowns:

### Featured Expeditions

- Rundown 5 (005://EXT) introduced log file code variant
- Many expeditions feature uplink objectives with varying difficulty

See [Establish Uplink - Official GTFO Wiki](https://gtfo.fandom.com/wiki/Establish_Uplink) for complete list.

## Comparison to Related Objectives

| Aspect                  | Terminal Uplink                    | Reactor Startup                    | Special Terminal Command |
| ----------------------- | ---------------------------------- | ---------------------------------- | ------------------------ |
| **Primary Action**      | Establish network uplink           | Defend reactor waves               | Execute special command  |
| **Terminal Usage**      | Multiple verification commands     | Multiple verification codes        | Single command execution |
| **Verification Stages** | 3-5 stages per uplink              | 4-10 waves                         | None (single execution)  |
| **Code Source**         | HUD or log files                   | HUD or terminals                   | N/A                      |
| **Combat Intensity**    | Moderate to high                   | Very high                          | Varies                   |
| **Alarm Pattern**       | Continuous during uplink           | Wave-based                         | Often bioscan-triggered  |
| **Time Pressure**       | Moderate                           | Very high (strict timers)          | Low                      |
| **Complexity**          | Moderate to high                   | High                               | Low to moderate          |
| **Code Retrieval**      | Sometimes requires zone navigation | Sometimes requires zone navigation | N/A                      |

## Common Challenges

### Code Matching Errors

- **Challenge**: Mismatching 3-letter codes to 4-letter passwords, causing stage resets
- **Solution**: Double-check code on terminal before entering, communicate codes verbally

### Combat During Code Entry

- **Challenge**: Being attacked while trying to read and enter codes at terminal
- **Solution**: Designate protected terminal operator, team provides defense screen

### Log File Code Retrieval (Advanced)

- **Challenge**: Code runner gets killed or lost while retrieving codes from other zones
- **Solution**: Scout zones beforehand, clear paths, establish escape routes

### Wrong Verification Resets

- **Challenge**: Entering incorrect password resets current stage, wasting time and resources
- **Solution**: Verify code carefully before entry, use tab completion, communicate clearly

### Alarm Overwhelm

- **Challenge**: Continuous enemy spawns overwhelm defenses during verification sequence
- **Solution**: Strong sentry setup, C-Foam reinforcement, focus fire on priorities

### Time Pressure (Advanced Variant)

- **Challenge**: Taking too long to retrieve log codes allows enemy buildup
- **Solution**: Fast code runner, pre-cleared paths, efficient terminal navigation

### Multiple Uplinks

- **Challenge**: Some expeditions require establishing uplinks to multiple IP addresses
- **Solution**: Treat each uplink as separate objective, prepare fully for each

## Tips

- **Pre-type Commands**: Have `UPLINK_VERIFY ` ready before codes appear
- **Clear First**: Always clear terminal area thoroughly before initiating uplink
- **Sentry Coverage**: Position sentries covering all approach angles to terminal
- **Double-Check Codes**: Verify code match before entering to avoid resets
- **Tab Completion**: Use TAB key to auto-complete commands quickly
- **Communication**: Call out codes verbally to ensure whole team knows them
- **Terminal Operator**: Designate one player for code entry (protected position)
- **Defensive Priority**: Defense is more important than speed - take time to be accurate
- **Scout Logs First** (Advanced): Check log file locations before starting uplink
- **Code Runner Speed**: If retrieving codes, move quickly but carefully
- **Bio Tracker Essential**: Tag enemies for sentry targeting and team awareness
- **C-Foam Doors**: Reinforce terminal area entrances before starting
- **Ammo Check**: Top off ammunition before initiating uplink
- **Count Stages**: Track verification stage progress to anticipate completion
- **Error Recovery**: If stage resets, regroup and try again methodically
- **Terminal Position**: Stand where you can see both terminal and approaches
- **Multiple IP Addresses**: Confirm which IP you're connecting to before starting
- **Log File Names**: Note which log files contain codes (may be labeled clearly)
- **READ Command**: Practice using LOGS and READ commands before mission
- **Team Splitting** (Advanced): Only split for code retrieval if paths are pre-cleared

## Sources

- [Establish Uplink - Official GTFO Wiki](https://gtfo.fandom.com/wiki/Establish_Uplink)
- [Terminal - Official GTFO Wiki](https://gtfo.fandom.com/wiki/Terminal)
- [Terminal - Official GTFO Wiki (wiki.gg)](https://gtfo.wiki.gg/wiki/Terminal)
- [GTFO: Using Terminals (Tips & Tricks) - Screen Rant](https://screenrant.com/gtfo-using-terminals-tips-tricks/)
- [The ULTIMATE GTFO Guide - Steam Community](https://steamcommunity.com/sharedfiles/filedetails/?id=2359433203)
- [My Personal Terminal Guide - Steam Community](https://steamcommunity.com/sharedfiles/filedetails/?id=2134930681)
