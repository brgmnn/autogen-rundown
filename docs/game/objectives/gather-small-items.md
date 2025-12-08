# Gather Small Items Objective

## Overview

**Gather Small Items** (also known as **Gather Items**) is a collection-based objective in GTFO that requires players to explore zones throughout the Complex and collect a specific quota of small items from containers. Players must use terminal commands to locate items, navigate through dangerous zones, and retrieve them before extraction becomes available. This objective emphasizes exploration, terminal proficiency, and efficient zone navigation.

## What Are Small Items?

Small items are collectible objective items scattered throughout the Complex in containers. Unlike large items (Fog Turbines, Neonate HSUs), these items are picked up automatically when interacting with their containers and do not slow player movement. The most common types are:

- **Personnel IDs**: Identification cards from Complex personnel
- **GLP-1 Canisters**: Data storage canisters (GLP = "Gather Lockdown Protocols")
- **GLP-2 Canisters**: Advanced data storage canisters

These items are critical to the mission's research objectives, containing valuable data or biological samples needed by those who sent the prisoners into the Complex.

## Objective Flow

### Phase 1: Understand Requirements

1. **Check Warden Objective**: Review the objective to see required quota (e.g., "Collect 6 Personnel IDs")
2. **Identify Item Type**: Determine which type of item you need to collect
3. **Note Quota**: Remember the exact number required before extraction is available

### Phase 2: Locate Items Using Terminals

1. **Find a Terminal**: Locate a terminal in the starting zone or current zone
2. **List Items**: Use `LIST` command to see what items are available
3. **Filter by Type**: Use `LIST ITEM_TYPE` to see specific items across all zones
4. **Filter by Zone**: Use `LIST ZONE_##` to see items in specific zones
5. **Query Specific Items**: Use `QUERY [ITEM_NAME]` to find exact location of an item
6. **Plan Route**: Determine which zones to visit based on item locations

### Phase 3: Navigate and Collect

1. **Prioritize Zones**: If more items exist than needed, choose safest/closest zones
2. **Clear Threats**: Eliminate or stealth past sleepers in each zone
3. **Use PING**: At zone terminals, use `PING [ITEM_NAME]` to get directional audio
4. **Follow Audio Cues**: Ping sound gets louder as you approach the item
5. **Locate Container**: Find the container holding the item
6. **Collect Item**: Interact with container to collect (automatic pickup)
7. **Track Progress**: Monitor HUD to see current collection count vs quota

### Phase 4: Extraction

1. **Verify Quota**: Ensure required number of items has been collected
2. **Return to Start**: Navigate back to extraction point
3. **Extraction Available**: Once quota is met, extraction becomes available
4. **Complete Mission**: Perform final extraction scan to complete

## Core Mechanics

### Item Containers

- **Spawning**: Items spawn in containers scattered throughout zones
- **Fixed Locations**: Container locations are fixed per expedition (consistent)
- **Automatic Pickup**: Items are collected instantly upon container interaction
- **No Carry Penalty**: Small items don't slow movement or occupy hands
- **Visibility**: Containers may require thorough zone exploration to find

### Terminal Location System

**Three-Command System** for finding items:

1. **LIST**: Shows which items have spawned in the Complex
   - `LIST` - Shows all items on entire map (overwhelming)
   - `LIST RESOURCES` - Filter to resource boxes only
   - `LIST ITEM_TYPE` - Filter by item type (e.g., Personnel IDs)
   - `LIST ZONE_##` - Filter to specific zone
   - `LIST RESOURCES ZONE_14` - Combined filters

2. **QUERY**: Finds location of specific item
   - `QUERY [ITEM_NAME]` - Returns zone location
   - Example: `QUERY ID_018` might return "ID_018 is in ZONE 21"

3. **PING**: Provides directional audio for items in current zone
   - `PING [ITEM_NAME]` - Plays directional sound that gets louder when closer
   - `PING -t [ITEM_NAME]` - Continuous ping until Ctrl+C is pressed
   - Must be used from a terminal in the same zone as the item

### Quota and Surplus

- **Fixed Quota**: Each expedition has a specific required number
- **Surplus Items**: Often more items spawn than required
- **Prioritization**: Players can choose which zones to visit
- **Strategic Choice**: Avoid dangerous zones if enough items exist elsewhere
- **No Extraction Lock**: Cannot extract until quota is fully met

## Recommended Strategy

### Preparation Phase

1. **Terminal Survey**: Before moving out, use `LIST ITEM_TYPE` to see all item locations
2. **Map Planning**: Identify which zones contain items
3. **Route Optimization**: Plan most efficient route that visits necessary zones
4. **Surplus Analysis**: Count total items vs required quota
5. **Threat Assessment**: Consider which zones may be safer to visit

### Exploration Phase

1. **Zone Terminal First**: Upon entering new zone, locate terminal immediately
2. **Use PING**: From zone terminal, ping target items to get directional cues
3. **Follow Sound**: Move toward louder ping sounds
4. **Systematic Search**: Search zone methodically to avoid missing containers
5. **Clear First, Search After**: Eliminate threats before thorough searching
6. **Mark Mental Notes**: Remember which zones you've already cleared

### Collection Efficiency

1. **Batch Collection**: Collect all items in a zone before moving to next
2. **Terminal Confirmation**: Periodically check terminals to confirm remaining items
3. **Team Splitting**: If safe, split team to cover multiple zones simultaneously
4. **Communication**: Call out item locations and collection progress
5. **Count Tracking**: Monitor HUD count constantly to avoid over-collecting

### Common Terminal Workflows

**Initial Survey:**
```
LIST ITEM_ID           // See all Personnel IDs
```

**Zone-Specific Search:**
```
LIST ZONE_21           // See all items in Zone 21
PING ID_018            // Get directional ping for specific ID
```

**Continuous Tracking:**
```
PING -t GLP_042        // Continuous ping (stop with Ctrl+C)
```

## Terminal Commands

- `LIST` - Shows all items on the entire map
- `LIST ITEM_TYPE` - Filter by item type (e.g., `LIST ITEM_ID`, `LIST ITEM_GLP`)
- `LIST ZONE_##` - Show items in specific zone (e.g., `LIST ZONE_21`)
- `LIST RESOURCES ZONE_##` - Combined filter (e.g., `LIST ITEM_ID ZONE_21`)
- `QUERY [ITEM_NAME]` - Find zone location of specific item (e.g., `QUERY ID_018`)
- `PING [ITEM_NAME]` - Directional audio ping for item in current zone
- `PING -t [ITEM_NAME]` - Continuous ping until Ctrl+C

## Notable Expeditions

Gather Small Items appears in many expeditions:

### Personnel IDs
- **[ALT://R1B1](https://gtfo.wiki.gg/wiki/ALT://R1B1)**: Collect 6 Personnel IDs
- Many other expeditions feature ID collection

### GLP Canisters
- Various expeditions require collecting GLP-1 or GLP-2 Canisters

See [Gather Items - Official GTFO Wiki](https://gtfo.wiki.gg/wiki/Gather_Items) for complete list.

## Comparison to Related Objectives

| Aspect | Gather Small Items | Retrieve Big Items | Gather Terminal Items |
|--------|-------------------|-------------------|----------------------|
| **Item Size** | Small (IDs, GLPs) | Large (Turbines, HSUs) | Virtual (terminal data) |
| **Pickup Method** | Automatic from container | Pick up and carry | Terminal interaction |
| **Movement Impact** | No slowdown | Slowed movement | No movement needed |
| **Location Method** | LIST/QUERY/PING | QUERY/PING | Terminal logs |
| **Quantity** | Multiple items (quota) | Typically 1-2 items | Variable |
| **Container Source** | Physical containers | Items lying in zones | Terminal logs/data |
| **Hands Occupied** | No | Yes (can't use weapons) | No |
| **Team Distribution** | Can split to collect | One carrier + support | Team can split |

## Common Challenges

### Finding Containers

- **Challenge**: Containers can be well-hidden or in obscure corners of zones
- **Solution**: Use PING directional audio, search systematically, check all rooms and alcoves

### Terminal Inefficiency

- **Challenge**: Not using terminals effectively, wandering aimlessly
- **Solution**: Master LIST/QUERY/PING commands, always check terminal before exploring zone

### Dangerous Zone Navigation

- **Challenge**: Required items are in zones with heavy enemy presence
- **Solution**: Check if surplus items exist in safer zones, or clear threats before searching

### Time Wasting

- **Challenge**: Visiting too many zones when surplus items exist
- **Solution**: Count total items vs quota, plan minimal route using LIST commands

### Getting Lost

- **Challenge**: Forgetting which zones have been cleared or collected from
- **Solution**: Use terminal to verify uncollected items, communicate with team about progress

### PING Misunderstanding

- **Challenge**: Not understanding directional audio or distance indication
- **Solution**: Practice with PING, remember louder = closer, turn to locate direction

## Tips

- **LIST Before You Quest**: Always use `LIST ITEM_TYPE` before leaving start to see total available items
- **Zone Terminal Priority**: First action in new zone should be finding terminal for PING
- **Surplus is Your Friend**: If 8 items spawn but you only need 6, skip the dangerous zones
- **PING -t for Hunting**: Use continuous ping mode to hunt items hands-free
- **Systematic Searching**: Clear zones section by section, don't randomly wander
- **Team Communication**: Call out items found and zones cleared to avoid redundant searching
- **Collect All in Zone**: Get all items in a zone before moving to next (minimize backtracking)
- **Write Down Locations**: For complex expeditions, note which zones have items
- **QUERY is Your Map**: Use QUERY on each item from start terminal to plan entire route
- **Sound Direction**: Turn in place while PING sounds to pinpoint direction
- **Check Corners**: Containers often spawn in room corners or behind objects
- **Terminal Practice**: Master terminal commands in low-pressure situations first
- **Count Management**: Track your count vs quota constantly on HUD
- **Stealth Option**: If possible, sneak past sleepers to collect items without fighting
- **Extraction Locked**: Remember you cannot extract until quota is met - no early leaving

## Sources

- [Gather Items - Official GTFO Wiki](https://gtfo.wiki.gg/wiki/Gather_Items)
- [Gather Items - Official GTFO Wiki (Fandom)](https://gtfo.fandom.com/wiki/Gather_Items)
- [Terminal - Official GTFO Wiki](https://gtfo.fandom.com/wiki/Terminal)
- [Terminal Command - Official GTFO Wiki](https://gtfo.wiki.gg/wiki/Terminal_Command)
- [Objectives - Official GTFO Wiki](https://gtfo.wiki.gg/wiki/Objectives)
- [GTFO: Using Terminals (Tips & Tricks) - Screen Rant](https://screenrant.com/gtfo-using-terminals-tips-tricks/)
- [GTFO - Terminal Guide (Useful Codes) - IndieFAQ](https://indiefaq.com/guides/956-gtfo.html)
- [Terminal Commands and Floor Inventory - Steam Community](https://steamcommunity.com/sharedfiles/filedetails/?id=2761182459)
- [ALT://R1B1 - Official GTFO Wiki](https://gtfo.wiki.gg/wiki/ALT://R1B1)
