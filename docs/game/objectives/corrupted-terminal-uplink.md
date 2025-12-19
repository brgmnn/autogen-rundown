# Corrupted Terminal Uplink Objective

## Overview

**Corrupted Terminal Uplink** is an advanced variant of the Terminal Uplink objective in GTFO that requires players to coordinate across multiple terminals to establish network connections. Unlike standard uplinks where verification codes appear on the HUD, corrupted uplinks send codes to log files on separate terminals, requiring team splitting and coordination. One player must read codes from log files while another inputs them at the uplink terminal, adding significant complexity and communication demands to the objective.

## What is Corrupted Terminal Uplink?

Corrupted Terminal Uplink represents a damaged or compromised network connection system where the verification infrastructure has been corrupted. Instead of displaying codes directly to prisoners, the system writes them to log files on terminals scattered throughout the Complex. Players must locate these log terminals, read the verification codes, and relay them to teammates at the uplink terminal for entry. This requires precise communication, timing, and team coordination under combat pressure.

## Objective Flow

### Phase 1: Locate Uplink and Log Terminals

1. **Check Warden Objective**: Review which IP address requires corrupted uplink
2. **Find Central Uplink Terminal**: Locate primary uplink terminal
3. **Identify Log Terminals**: Determine which terminals contain verification codes
4. **Zone Reconnaissance**: Scout zones containing both terminal types
5. **Resource Gathering**: Collect ammunition and supplies
6. **Plan Terminal Route**: Map path between uplink and log terminals

### Phase 2: Initiate Corrupted Uplink

1. **Access Uplink Terminal**: Reach primary uplink terminal
2. **Prepare Team Split**: Position players at uplink and log terminals
3. **Execute UPLINK_CONNECT**: Enter `UPLINK_CONNECT [IP_ADDRESS]`
4. **Alarm Triggers**: Connection initiates alarm sequence
5. **Begin Verification Stages**: Multiple stages requiring code retrieval

### Phase 3: Code Retrieval and Verification

1. **Code Reader at Log Terminal**:
   - Access terminal containing verification logs
   - Use `LOGS` command to list log files
   - Use `READ [FILENAME]` to view verification code
   - Note 3-letter code (e.g., X04, Y03, Z12)
   - Communicate code to uplink operator

2. **Uplink Operator at Central Terminal**:
   - Receive code from log reader via communication
   - Match 3-letter code to 4-letter password on HUD
   - Enter `UPLINK_VERIFY [PASSWORD]`
   - Defend terminal position during verification

3. **Multiple Stages**:
   - Repeat process for each verification stage (typically 3-5)
   - Each stage requires new code from log files
   - Wrong verification resets current stage

### Phase 4: Uplink Confirmation

1. **Final Verification Stage**: Complete all standard verification stages
2. **Navigate to Confirmation Terminal**: Move to second designated terminal
3. **Execute UPLINK_CONFIRM**: Enter confirmation command at second terminal
4. **Uplink Established**: Network connection successfully established

### Phase 5: Complete and Extract

1. **Alarm Management**: Survive ongoing alarm sequences
2. **Regroup Team**: Reunite split team members
3. **Continue Mission**: Proceed to next objective or extraction
4. **Final Extraction**: Navigate to extraction point

## Core Mechanics

### Multi-Terminal Coordination

**Terminal Roles:**

- **Central Uplink Terminal**: Primary terminal for UPLINK_CONNECT and UPLINK_VERIFY
- **Log Terminals**: Contain verification codes in log files (in other zones)
- **Confirmation Terminal**: Secondary terminal for UPLINK_CONFIRM command

**Team Splitting Required:**

- Code Reader navigates to log terminals
- Uplink Operator remains at central terminal
- Defenders protect both positions
- Communication critical between split team

### Corrupted Verification System

**Code Location:**

- Verification codes NOT displayed on HUD
- Codes written to terminal log files instead
- Log files located on terminals in other zones
- Requires physical navigation to retrieve codes

**Log File Access:**

- Use `LOGS` command to list files on log terminal
- Use `READ [FILENAME]` to view verification codes
- Log files typically have descriptive names (verification.log, codes.log, etc.)
- Codes presented in 3-letter format (X04, Y03, Z12)

### Verification Commands

**UPLINK_CONNECT [IP_ADDRESS]:**

- Initiates corrupted uplink to specified IP
- Triggers alarm immediately
- Begins multi-stage verification process

**UPLINK_VERIFY [PASSWORD]:**

- Enters 4-letter verification password at central terminal
- Code must match log file code
- Incorrect password resets current stage

**UPLINK_CONFIRM:**

- Final confirmation command at second terminal
- Completes uplink establishment
- Only used after all verification stages complete

### Communication Requirements

**Critical Information Flow:**

- Log reader finds and reads codes
- Codes relayed verbally to uplink operator
- Uplink operator confirms receipt and enters verification
- Status updates on verification success/failure
- Position and threat callouts for split team

## Recommended Strategy

### Preparation Phase

1. **Scout Both Terminal Types**:
   - Locate central uplink terminal
   - Identify log terminal locations (may be multiple)
   - Map confirmation terminal location
2. **Clear Log Terminal Zones**: Pre-clear routes to log terminals
3. **Team Role Assignment**:
   - **Code Runner**: Fast, skilled player for log retrieval
   - **Uplink Operator**: Player with good terminal speed and accuracy
   - **Defenders**: 1-2 players protecting uplink terminal
4. **Communication Check**: Ensure voice communication working properly
5. **Resource Stockpile**: Top off ammunition before initiation

### Team Positioning

**Initial Setup:**

- Uplink operator at central terminal
- Code runner ready to navigate to first log terminal
- Defenders positioned at uplink terminal
- All players confirm readiness

**During Uplink:**

- Uplink operator and defenders hold central terminal
- Code runner navigates between log terminals
- Maintain constant communication
- Adapt positioning as needed

### Code Retrieval Process

**Code Runner Workflow:**

1. Navigate to log terminal zone quickly but safely
2. Access log terminal
3. Type `LOGS` to list files
4. Type `READ [VERIFICATION_FILE]` to view code
5. Note 3-letter code clearly
6. Communicate code verbally to uplink operator
7. Confirm operator received code correctly
8. Prepare to move to next log terminal for next stage

**Communication Protocol:**

- Code runner: "Code is X04, repeat X-zero-four"
- Uplink operator: "Confirm X04, entering verification"
- Code runner: "Verified, moving to next terminal"

### Uplink Operator Workflow

1. Stay at central uplink terminal
2. Defend position from spawning enemies
3. Listen for code from runner
4. Match 3-letter code to 4-letter password on HUD
5. Pre-type `UPLINK_VERIFY `
6. Enter complete command quickly
7. Confirm verification success
8. Prepare for next code

### Defensive Strategy

**Central Terminal Defense:**

- 1-2 defenders focus on protecting uplink operator
- Position sentries covering main approaches
- C-Foam door reinforcement
- Bio Tracker for enemy tagging

**Code Runner Protection:**

- Runner must be self-sufficient during retrieval
- Pre-cleared zones critical for runner safety
- Defenders cannot protect runner due to distance
- Runner drops back to regroup if overwhelmed

### Advanced Coordination

1. **Pre-Scout Log Terminals**: Identify which terminals have verification logs before starting
2. **Clear All Paths**: Pre-clear entire route from uplink to all log terminals
3. **Timing Coordination**: Code runner times arrivals with verification stages
4. **Backup Reader**: Designate backup in case primary runner incapacitated
5. **Log Terminal Memory**: Remember which terminals contain which codes (if multiple)

## Terminal Commands

### Uplink Commands

- `UPLINK_CONNECT [IP_ADDRESS]` - Initiate corrupted uplink (e.g., `UPLINK_CONNECT 127.0.0.1`)
- `UPLINK_VERIFY [PASSWORD]` - Enter 4-letter verification password (e.g., `UPLINK_VERIFY DART`)
- `UPLINK_CONFIRM` - Final confirmation at second terminal

### Log File Commands

- `LOGS` - List all available log files on current terminal
- `READ [FILENAME]` - Read specific log file containing codes (e.g., `READ verification.log`)

### Navigation Commands

- `PING` - Locate terminal zones
- `LIST` - Show items and objectives
- `QUERY` - Find specific objectives

## Notable Expeditions

Corrupted Terminal Uplink appears in various expeditions, particularly in higher-difficulty tiers.

### Introduction

- Corrupted uplink variant with log files introduced in Rundown 5 (005://EXT)

### Difficulty Scaling

- AutogenRundown mod features longer verification codes at higher difficulties for both standard and corrupted uplinks

See [Establish Uplink - Official GTFO Wiki](https://gtfo.fandom.com/wiki/Establish_Uplink) for information about uplink objectives.

## Comparison to Related Objectives

| Aspect                  | Corrupted Terminal Uplink    | Terminal Uplink (Standard) | Timed Terminal Sequence   |
| ----------------------- | ---------------------------- | -------------------------- | ------------------------- |
| **Code Source**         | Log files in other terminals | HUD display                | N/A                       |
| **Team Splitting**      | Required                     | Not required               | Required                  |
| **Terminal Count**      | 3+ (uplink, logs, confirm)   | 1 (uplink only)            | 2-3 (central, parallel)   |
| **Communication Needs** | Critical (code relay)        | Low                        | Critical (coordination)   |
| **Time Pressure**       | Moderate                     | Low                        | Very high (strict timers) |
| **Zone Navigation**     | Extensive                    | Minimal                    | Moderate                  |
| **Complexity**          | Very high                    | Moderate                   | Very high                 |
| **Code Entry**          | Relayed from reader          | Direct from HUD            | Command sequences         |
| **Confirmation Step**   | UPLINK_CONFIRM required      | None                       | Multiple confirms         |
| **Combat Intensity**    | High (split defense)         | Moderate to high           | Very high (continuous)    |

## Common Challenges

### Code Communication Errors

- **Challenge**: Miscommunication of codes between runner and operator causes verification failures
- **Solution**: Clear pronunciation, phonetic alphabet (X-ray, Zero, Four), confirmation repeats

### Code Runner Death

- **Challenge**: Code runner killed while retrieving codes from log terminals
- **Solution**: Pre-clear zones thoroughly, designate backup runner, runner uses stealth when possible

### Split Team Vulnerability

- **Challenge**: Divided team makes both positions weaker against enemy spawns
- **Solution**: Strong sentry setup at uplink terminal, code runner self-sufficiency, pre-cleared paths

### Log Terminal Location

- **Challenge**: Finding which terminals contain verification logs under time pressure
- **Solution**: Pre-scout log terminal locations before initiating uplink, use LOGS command to check

### Verification Reset Frustration

- **Challenge**: Wrong code entry resets stage, forcing code re-retrieval
- **Solution**: Double-check code clarity, operator confirms code before entering, careful typing

### Alarm Overwhelm During Split

- **Challenge**: Continuous spawns overwhelm uplink defenders while runner away
- **Solution**: Maximum sentry deployment, C-Foam reinforcement, defenders prioritize survival over kills

### Confirmation Terminal Access

- **Challenge**: Reaching confirmation terminal after all verifications complete
- **Solution**: Scout confirmation terminal location beforehand, clear path in advance

### Multiple Log Terminals

- **Challenge**: Different codes in different terminals, confusion about which to read
- **Solution**: Systematic approach, communicate which terminal contains which stage codes

## Tips

- **Pre-Scout Logs**: Identify log terminal locations before starting uplink
- **Clear Everything First**: Pre-clear all zones between uplink and log terminals
- **Phonetic Alphabet**: Use clear pronunciation for code communication (X-ray, Yankee, Zulu)
- **Confirmation Repeats**: Always confirm code receipt ("Confirm X04")
- **Code Runner Speed**: Fastest player should be code runner
- **Maximum Sentries**: Deploy all available sentries at uplink terminal
- **C-Foam Uplink**: Heavily reinforce uplink terminal area
- **Backup Runner Ready**: Designate backup in case primary dies
- **LOGS First**: Code runner always types LOGS before READ to see available files
- **Log File Names**: Verification logs typically have descriptive names
- **Communication Discipline**: Clear, concise communication essential
- **Tab Completion**: Use TAB key for faster command entry
- **Uplink Operator Focus**: Operator focuses on terminal and defense, not full combat
- **Code Runner Self-Sufficiency**: Runner must handle solo combat during retrieval
- **Pre-Type Commands**: Have UPLINK_VERIFY ready before code arrives
- **Zone Clearing Order**: Clear log terminal zones in order of verification stages
- **Terminal Memory**: Note which terminals have codes to avoid rechecking
- **UPLINK_CONFIRM Location**: Know where confirmation terminal is before final stage
- **Team Regroup**: Reunite team after uplink completion before proceeding
- **Communication Test**: Test voice comms before starting objective

## Sources

- [Establish Uplink - Official GTFO Wiki](https://gtfo.fandom.com/wiki/Establish_Uplink)
- [Terminal - Official GTFO Wiki](https://gtfo.fandom.com/wiki/Terminal)
- [Logs - Official GTFO Wiki](https://gtfo.fandom.com/wiki/Logs)
- [GTFO: Using Terminals (Tips & Tricks) - Screen Rant](https://screenrant.com/gtfo-using-terminals-tips-tricks/)
- [The ULTIMATE GTFO Guide - Steam Community](https://steamcommunity.com/sharedfiles/filedetails/?id=2359433203)
- [My Personal Terminal Guide - Steam Community](https://steamcommunity.com/sharedfiles/filedetails/?id=2134930681)
- [AutogenRundown Changelog - Thunderstore](https://thunderstore.io/c/gtfo/p/the_tavern/AutogenRundown/changelog/)
