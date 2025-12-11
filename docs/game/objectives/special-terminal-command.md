# Special Terminal Command Objective

## Overview

**Special Terminal Command** (also known as **Terminal Command**) is an objective in GTFO that requires players to locate specific terminals and execute special commands. These commands trigger significant environmental changes or events such as disabling lights, flooding zones with infectious fog, opening security doors, or spawning enemy waves. The objective combines terminal navigation, tactical decision-making, and combat readiness to handle the consequences of command execution.

## What is a Special Terminal Command?

Special Terminal Commands are executable programs that prisoners must run on designated terminals within the Complex. Unlike standard terminal queries (LIST, PING, QUERY), these special commands are mission-critical executables that alter the environment or unlock progression. Commands typically begin with `START` followed by the program name (e.g., `START LIGHT_TESTING.EXE`).

## Objective Flow

### Phase 1: Locate Required Terminals

1. **Check Warden Objective**: Review which terminal command(s) must be executed
2. **Terminal Navigation**: Use standard terminal commands to locate target terminals
3. **Zone Reconnaissance**: Scout the zones containing required terminals
4. **Resource Gathering**: Collect ammunition and supplies along the path
5. **Threat Assessment**: Clear or plan around enemies near target terminals

### Phase 2: Command Identification

1. **Access Terminal**: Reach the designated terminal
2. **List Available Commands**: Use `COMMANDS` command to view executable programs
3. **Identify Target Command**: Confirm which command matches objective requirements
4. **Read Command Details**: Review command description if available
5. **Prepare for Execution**: Set up defensive positions before running command

### Phase 3: Command Execution

1. **Team Positioning**: Ensure team is ready for consequences
2. **Execute Command**: Type `START [COMMAND_NAME].EXE` and press enter
3. **Immediate Effects**: Environmental changes occur immediately (lights out, fog, etc.)
4. **Bioscan (Sometimes)**: Some commands trigger bioscan requirements
5. **Enemy Spawns**: Many commands trigger alarm sequences or enemy waves

### Phase 4: Manage Consequences and Extract

1. **Adapt to Environment**: Adjust tactics for new conditions (darkness, fog, etc.)
2. **Complete Bioscan**: If triggered, complete required bioscan sequences
3. **Survive Waves**: Fight through any enemy spawns caused by command
4. **Continue Mission**: Proceed to next objective or extraction
5. **Final Extraction**: Navigate to extraction point under altered conditions

## Core Mechanics

### Command Types

**Environmental Modification Commands:**
- **LIGHT_TESTING.EXE**: Disables all lights in the expedition permanently
- **VENTILATION_OVERRIDE.EXE**: Floods zones with highly infectious fog
- **Power/Door Commands**: Open security doors or restore power systems

**Security/Access Commands:**
- Commands that unlock progression-critical doors
- Commands that disable security systems
- Commands that grant access to restricted zones

**Unknown/Classified Commands:**
- **DENOFWOLVES.EXE**: Special command with undisclosed effects
- **THREAT_LEVEL_M.EXE**: Likely increases threat or spawns enemies
- **PRIVATE_ENCRYPTION.EXE**: Encrypted command with special purpose

### Terminal Navigation

**Finding Command Terminals:**
- Use `PING` to locate terminal zones
- Some commands must be executed at specific terminals
- Terminal accessibility may require keys or security clearance

**Using COMMANDS Command:**
- Type `COMMANDS` at terminal to list available executable programs
- Includes Reactor Startup, Reactor Shutdown, Establish Uplink, and Terminal Command objectives
- Tab completion (TAB key) can auto-complete command names

### Environmental Consequences

**Permanent Darkness (LIGHT_TESTING.EXE):**
- All lights disabled for remainder of expedition
- Flashlights become critical survival tool
- Visibility drastically reduced
- Enemy detection becomes significantly harder

**Infectious Fog (VENTILATION_OVERRIDE.EXE):**
- Highly infectious fog fills affected zones
- Infection meter rises rapidly in fog
- May become permanent environmental hazard
- Requires careful navigation and fog repellers/turbines

**Enemy Waves:**
- Many commands trigger error alarms or surge alarms
- Continuous or wave-based enemy spawns
- Must survive while adapting to environmental changes

**Bioscan Requirements:**
- Some commands trigger bioscan sequences
- Team must hold position during scan
- Vulnerable to enemy attacks during scan

## Recommended Strategy

### Preparation Phase

1. **Scout Terminal Locations**: Use PING and zone navigation to find target terminals
2. **Clear the Area**: Eliminate enemies around terminal before execution
3. **Resource Stockpile**: Ensure ammunition and consumables are topped off
4. **Defensive Setup**: Position sentries covering terminal and bioscan areas
5. **Flashlight Check**: If LIGHT_TESTING expected, ensure flashlights functional
6. **Fog Preparation**: If VENTILATION_OVERRIDE expected, prepare fog repellers/plan route

### Command Execution Phase

1. **Team Communication**: Announce command execution countdown
2. **Defensive Positions**: Team positioned to handle spawns or bioscan
3. **Execute Swiftly**: Type command and execute when team ready
4. **Tab Completion**: Use TAB key to auto-complete long command names
5. **Immediate Adaptation**: Switch to flashlights, activate repellers, etc.

### Post-Execution Phase

1. **Complete Bioscans**: If triggered, complete scan sequences immediately
2. **Manage Alarms**: Survive wave spawns, focus fire on priorities
3. **Environmental Adaptation**:
   - **Darkness**: Use flashlights, move slowly, maintain visual contact
   - **Fog**: Navigate quickly through fog, use repellers, manage infection
4. **Resource Conservation**: Save resources for remaining objectives and extraction
5. **Progress to Next Objective**: Continue mission under new conditions

### Environmental Survival

**Darkness Survival (LIGHT_TESTING):**
- All players enable flashlights immediately
- Move more slowly to avoid alerting sleeping enemies
- Use Bio Tracker to tag enemies in darkness
- Maintain closer team formation
- Sentries become less effective - may need manual targeting

**Infectious Fog Survival (VENTILATION_OVERRIDE):**
- Deploy Fog Repellers if available
- Move through fog quickly to minimize infection
- Plan paths that avoid prolonged fog exposure
- Monitor infection levels constantly
- Disinfection stations become critical

## Terminal Commands

### Standard Navigation Commands
- `COMMANDS` - Lists all available executable commands at current terminal
- `PING` - Locate terminal zones
- `LIST` - Show items and objectives
- `QUERY` - Find specific item or objective locations

### Special Executable Commands
- `START LIGHT_TESTING.EXE` - Disable all lights in expedition
- `START VENTILATION_OVERRIDE.EXE` - Flood zones with infectious fog
- `START DENOFWOLVES.EXE` - Special classified command
- `START THREAT_LEVEL_M.EXE` - Special classified command
- `START PRIVATE_ENCRYPTION.EXE` - Special classified command
- Various mission-specific commands (varies by expedition)

### Command Syntax
- Use `START [COMMAND_NAME].EXE` format
- Tab completion available for faster input
- Case-insensitive (usually)
- Must be executed at correct terminal (some commands terminal-specific)

## Notable Expeditions

Various expeditions feature Terminal Command objectives with different environmental effects and challenges.

See [List of Terminal Commands - Official GTFO Wiki](https://gtfo.wiki.gg/wiki/List_of_Terminal_Commands) for complete list of commands and affected expeditions.

## Comparison to Related Objectives

| Aspect | Special Terminal Command | Reactor Startup | Terminal Uplink |
|--------|-------------------------|-----------------|-----------------|
| **Primary Action** | Execute special command | Defend wave sequences | Establish network connection |
| **Terminal Usage** | Single command execution | Multiple verification codes | Multiple uplink commands |
| **Environmental Impact** | High (lights, fog, etc.) | Moderate (power activation) | Low (network only) |
| **Combat Intensity** | Varies (often moderate) | Very high | Moderate to high |
| **Preparation Needed** | Moderate | Very high | Moderate |
| **Consequences** | Permanent environment change | Reactor online | Network established |
| **Complexity** | Low to moderate | High | Moderate |
| **Bioscan Requirement** | Sometimes | No (wave defense instead) | Sometimes |

## Common Challenges

### Unknown Command Effects

- **Challenge**: Not knowing what a command will do before executing it
- **Solution**: Scout expedition information beforehand, prepare for worst-case scenarios

### Darkness Navigation (LIGHT_TESTING)

- **Challenge**: Complete loss of visibility making navigation extremely difficult
- **Solution**: All players use flashlights, move slowly, maintain formation, use Bio Tracker

### Infectious Fog Management (VENTILATION_OVERRIDE)

- **Challenge**: Rapid infection buildup while navigating fog-filled zones
- **Solution**: Fog Repellers, quick navigation through fog, prioritize disinfection stations

### Command Execution Timing

- **Challenge**: Executing command at wrong time (team not ready, poor positioning)
- **Solution**: Coordinate team readiness, confirm all players positioned and prepared

### Bioscan Vulnerability

- **Challenge**: Being stationary during bioscan while environment hostile and visibility low
- **Solution**: Strong defensive setup before execution, sentry coverage, team coordination

### Terminal Location Exposure

- **Challenge**: Required terminal in dangerous or exposed location
- **Solution**: Clear area thoroughly before execution, establish defensive perimeter

### Permanent Environmental Changes

- **Challenge**: Dealing with darkness or fog for remainder of expedition
- **Solution**: Adapt tactics early, conserve resources for altered conditions

## Tips

- **COMMANDS First**: Always type `COMMANDS` at terminal to see available executables
- **Tab Completion**: Use TAB key to auto-complete long command names quickly
- **Clear Before Execute**: Always clear enemies around terminal before running command
- **Team Ready Check**: Confirm all players ready before executing command
- **Flashlight Prep**: If LIGHT_TESTING expected, test flashlights beforehand
- **Fog Repellers**: Bring Fog Repellers if VENTILATION_OVERRIDE is possible
- **Sentry Setup**: Position sentries before execution, facing expected spawn directions
- **Bio Tracker Essential**: Tag enemies in darkness or fog for team awareness
- **Ammo Check**: Top off ammunition before executing commands that trigger alarms
- **Bioscan Positions**: If bioscan likely, establish defensive positions first
- **Communicate**: Call out command execution countdown so team can prepare
- **Environmental Adaptation**: Switch tactics immediately after environment changes
- **Resource Conservation**: Save heavy ammo for post-command combat
- **Scout Ahead**: If possible, scout extraction path before triggering environmental changes
- **Infection Management**: Monitor infection levels constantly in fog environments
- **Stay Together**: Maintain tight formation in darkness to avoid separation
- **Slow and Steady**: Move deliberately in darkness to avoid alerting sleepers
- **Read Effects**: If command description available, read it carefully before executing
- **Backup Plans**: Have contingency for if command triggers unexpected consequences
- **Terminal Practice**: Practice typing commands quickly before critical execution

## Sources

- [List of Terminal Commands - Official GTFO Wiki](https://gtfo.fandom.com/wiki/List_of_Terminal_Commands)
- [Terminal Command - Official GTFO Wiki](https://gtfo.wiki.gg/wiki/Terminal_Command)
- [Terminal - Official GTFO Wiki](https://gtfo.fandom.com/wiki/Terminal)
- [GTFO: Using Terminals (Tips & Tricks) - Screen Rant](https://screenrant.com/gtfo-using-terminals-tips-tricks/)
- [Mastering Terminals in GTFO – Commands, Tips, and Strategies](https://mygamingtutorials.com/2025/06/01/mastering-terminals-in-gtfo-commands-tips-and-strategies/)
- [Terminal Commands and Floor Inventory - Steam Community](https://steamcommunity.com/sharedfiles/filedetails/?id=2761182459)
