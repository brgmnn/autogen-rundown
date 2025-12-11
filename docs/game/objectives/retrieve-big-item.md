# Retrieve Big Item Objective

## Overview

**Retrieve Big Item** (also known as **Retrieve Item**) is a carry-and-deliver objective in GTFO that requires players to locate a large item and transport it to an extraction zone. Big items severely restrict player capabilities while carried - prohibiting sprinting, weapon use, and most interactions. This objective demands careful planning, team coordination, and tactical positioning as one player becomes vulnerable while the rest provide protection during the dangerous journey to extraction.

## What Are Big Items?

Big items are large, heavy objective items that must be physically carried by a player through the Complex to extraction. Unlike small items (Personnel IDs, GLPs) that are automatically collected, big items occupy the player's hands and impose severe movement and combat restrictions. The most common big items are:

- **Fog Turbine**: Large device used to clear or manage fog (also used in Power Cell Distribution)
- **Neonate HSU**: Hydro Stasis Unit containing an infant or child specimen
- **Other Carry Items**: Various mission-specific large items

Big items represent high-value targets that justify the extreme risk and coordination required to extract them from deep within the Complex.

## Objective Flow

### Phase 1: Locate the Big Item

1. **Check Warden Objective**: Review which item type must be retrieved
2. **Terminal Search**: Use `QUERY [ITEM_NAME]` to find item location
3. **Navigate to Zone**: Travel through the Complex to item's zone
4. **PING for Precision**: Use `PING [ITEM_NAME]` from zone terminal for directional audio
5. **Clear the Area**: Eliminate enemies around item location before pickup

### Phase 2: Prepare for Transport

1. **Resource Check**: Ensure team has sufficient ammunition and consumables
2. **Route Planning**: Plan safest/fastest route back to extraction
3. **Designate Carrier**: Choose player to carry item (typically most survivable role)
4. **Sentry Placement**: Position sentries along extraction route if possible
5. **C-Foam Preparation**: Identify doors to reinforce during retreat
6. **Clear Path**: Pre-clear enemies along extraction route

### Phase 3: Item Pickup and Transport

1. **Pick Up Item**: Designated player interacts with item to carry it
2. **Carrier Restrictions Activate**:
   - Cannot sprint (walk speed only)
   - Cannot use weapons
   - Cannot use consumables
   - Cannot revive teammates
   - Cannot open lockers or boxes
   - CAN open doors and use flashlight
   - CAN drop item when needed
3. **Team Formation**: Establish protective formation around carrier
4. **Slow Movement**: Progress toward extraction at walking pace

### Phase 4: Extraction

1. **Reach Extraction Zone**: Navigate carrier to extraction point
2. **Error Alarm Trigger**: Pickup or extraction scan may trigger error alarm
3. **Position Item**: Carrier must bring item inside extraction scan circle
4. **Extraction Scan**: Complete final bioscan with item present in scan area
5. **Survive**: At least one alive prisoner plus item in circle completes mission

## Core Mechanics

### Carry Restrictions

When carrying a big item, the player **CANNOT**:
- Sprint (reduced to walk speed)
- Use weapons (completely defenseless)
- Use consumables (mines, C-Foam, med-kits, etc.)
- Revive downed teammates
- Open lockers or resource boxes
- Perform most interactions

When carrying a big item, the player **CAN**:
- Walk (at reduced speed)
- Open doors
- Use flashlight
- Drop the item (press drop key)
- Pick up item again after dropping

### Item Drop Mechanic

**Strategic Dropping:**
- Carrier can drop item to defend themselves
- Dropped items remain where placed
- Any player can pick up dropped item
- Useful for combat situations or reviving teammates
- Risky if enemies spawn near dropped item

### Error Alarm on Extraction

**Common Trigger Pattern:**
- Picking up the item may trigger error alarm immediately
- Alternatively, initiating extraction scan triggers alarm
- Error alarms spawn enemies continuously
- Cannot be deactivated - must survive until extraction completes

### Extraction Scan Requirements

**Mandatory Conditions:**
- Item must be inside extraction scan circle
- At least one living prisoner must complete scan
- Scan duration varies by expedition
- Enemies spawn continuously during scan

## Recommended Strategy

### Preparation Phase

1. **Terminal Reconnaissance**:
   - `QUERY [ITEM_NAME]` to find item zone
   - `LIST RESOURCES` to map ammo and health stations
   - `PING [ITEM_NAME]` for exact location in zone
2. **Route Clearing**: Clear ALL enemies from item location to extraction
   - Don't skip this step - it's critical
   - Mark or remember sleeping enemy locations
   - Clear scouts and patrols
3. **Resource Stockpile**: Top off ammunition before pickup
4. **Sentry Pre-Placement**: Position sentries along route and at extraction
5. **Team Role Assignment**:
   - **Carrier**: Survivable player with good positioning sense
   - **Point**: Lead player clearing immediate path
   - **Guards**: 2 players flanking and protecting carrier
   - **Rear**: Player watching back trail

### During Transport

1. **Tight Formation**: Maintain close protective formation around carrier
2. **Carrier Callouts**: Carrier communicates when stopping or dropping item
3. **Slow and Steady**: Move deliberately, don't rush into danger
4. **Drop for Combat**: If overwhelmed, carrier drops item to fight
5. **Drop for Revives**: Carrier drops item to revive teammates if needed
6. **Re-establish Formation**: After dropping, secure area before re-pickup
7. **Communication**: Constant callouts of threats and positioning

### Extraction Phase

1. **Scout Extraction First**: Locate exact scan circle position
2. **Sentry Coverage**: 2-4 sentries covering main enemy approach routes
3. **C-Foam Doors**: Reinforce doors leading to extraction area
4. **Position Item**: Carrier brings item to center of scan circle
5. **Defensive Setup**: Team forms perimeter around scan circle
6. **Drop Item**: Carrier drops item in circle to defend during scan
7. **Survive Scan**: Hold position and survive continuous enemy spawns
8. **Keep Item in Circle**: If item gets knocked out of circle, retrieve it immediately

### Team Coordination

**Communication Essential:**
- Carrier calls out movement ("stopping," "moving," "dropping item")
- Guards call out threats and their positions
- Coordinate item drops and pickups
- Call out when carrier is vulnerable

**Defensive Formation:**
- Point player leads, clearing immediate path
- Carrier follows in center
- Guards flank left and right
- Rear player watches back trail
- Maintain visual contact always

**Combat Protocol:**
- If attacked: Carrier drops item, all players engage
- After combat: Carrier picks up item, formation re-established
- Don't leave dropped item unattended for long

## Terminal Commands

- `QUERY [ITEM_NAME]` - Find zone location of big item
- `PING [ITEM_NAME]` - Directional audio ping for item in current zone
- `LIST RESOURCES` - Map ammo and health stations along route
- `LIST ZONE_##` - View items and objectives in specific zone

## Notable Expeditions

### Fog Turbine Retrieval
- Various expeditions require retrieving Fog Turbines to extraction

### Neonate HSU Retrieval
- **[ALT://R5B1](https://gtfo.wiki.gg/wiki/ALT://R5B1)**: Features Neonate HSU retrieval
- **[ALT://R2E1](https://gtfo.wiki.gg/wiki/ALT://R2E1)**: Neonate HSU objective
- **[ALT://R3A1](https://gtfo.wiki.gg/wiki/R3A1)**: Neonate HSU retrieval

See [Retrieve Item - Official GTFO Wiki](https://gtfo.wiki.gg/wiki/Retrieve_Item) for complete list.

## Comparison to Related Objectives

| Aspect | Retrieve Big Item | Gather Small Items | HSU Find Sample |
|--------|------------------|-------------------|-----------------|
| **Item Type** | Large carry item | Small auto-pickup | Tissue sample (virtual) |
| **Carry Mechanic** | Must carry, restricts player | Automatic pickup | Bioscan only |
| **Movement Impact** | Severe (walk speed, no sprint) | None | None |
| **Weapon Use** | Prohibited while carrying | Full weapon access | Full weapon access |
| **Team Dependency** | Very high (carrier defenseless) | Low to moderate | Moderate |
| **Transport Required** | Yes (to extraction) | No | No |
| **Alarm Timing** | At pickup or extraction | Varies | At HSU bioscan |
| **Drop Mechanic** | Yes (strategic dropping) | N/A | N/A |
| **Extraction Requirement** | Item in scan circle | Just player presence | Player presence |
| **Difficulty** | High (coordination intensive) | Low to moderate | Moderate to high |

## Common Challenges

### Carrier Vulnerability

- **Challenge**: Carrier is completely defenseless while holding item
- **Solution**: Team must provide total protection, carrier drops item to defend if necessary

### Slow Movement Speed

- **Challenge**: Walking pace makes team vulnerable to prolonged combat
- **Solution**: Pre-clear path thoroughly, minimize combat during transport

### Error Alarm Overwhelm

- **Challenge**: Continuous enemy spawns during extraction scan
- **Solution**: Strong sentry setup, C-Foam reinforcement, focus fire priorities

### Item Drop in Combat

- **Challenge**: Dropped item gets surrounded by enemies or lost in chaos
- **Solution**: Drop item in safe position, clear enemies, retrieve immediately

### Team Separation

- **Challenge**: Carrier falls behind or guards move too fast
- **Solution**: Match pace to carrier, maintain formation discipline

### Revive Dilemma

- **Challenge**: Teammate downed but carrier can't revive while holding item
- **Solution**: Carrier drops item, assists revive, team secures area before re-pickup

### Long Distance Transport

- **Challenge**: Item location is very far from extraction
- **Solution**: Plan multiple rest/defensive points, pre-place sentries, clear methodically

## Tips

- **Clear First, Carry Second**: Always clear entire extraction route before picking up item
- **Designated Carrier**: Choose carrier before mission, ideally most survivable player
- **Drop Liberally**: Don't hesitate to drop item for combat or revives
- **Sentry Line**: Place sentries along route and at extraction for layered defense
- **C-Foam Retreat**: Reinforce doors behind you as you move toward extraction
- **Walk-Don't-Run Mentality**: Accept slow pace, don't rush into bad situations
- **Communication Constant**: Carrier and team communicate every action
- **Item in Circle**: Remember item MUST be in extraction scan circle
- **Carrier Drops to Fight**: During extraction scan, carrier drops item to defend
- **Bio Tracker Essential**: Tag enemies for team, especially for carrier awareness
- **One Hand Free**: Carrier can open doors and use flashlight - utilize this
- **Resource Pre-Position**: Drop extra ammo/health near extraction before pickup
- **Scout Extraction**: Know exact extraction circle location before transport begins
- **Formation Discipline**: Maintain protective formation throughout transport
- **Error Alarm Expected**: Assume error alarm will trigger, prepare accordingly
- **Time Management**: Prolonged transport drains resources, move efficiently
- **Backup Carrier**: Designate backup in case primary carrier goes down
- **Clear Sleeping Enemies**: Don't leave sleepers on route - they'll wake during alarm
- **Item Security**: Never leave dropped item unattended for extended time
- **Extraction Timing**: Coordinate scan start - ensure all players ready and positioned

## Sources

- [Retrieve Item - Official GTFO Wiki](https://gtfo.wiki.gg/wiki/Retrieve_Item)
- [Retrieve Item - Official GTFO Wiki (Fandom)](https://gtfo.fandom.com/wiki/Retrieve_Item)
- [Neonate HSU - Official GTFO Wiki](https://gtfo.wiki.gg/wiki/Neonate_HSU)
- [Neonate HSU - Official GTFO Wiki (Fandom)](https://gtfo.fandom.com/wiki/Neonate_HSU)
- [HSU - Official GTFO Wiki](https://gtfo.wiki.gg/wiki/HSU)
- [ALT://R5B1 - Official GTFO Wiki](https://gtfo.wiki.gg/wiki/ALT://R5B1)
- [ALT://R2E1 - Official GTFO Wiki](https://gtfo.wiki.gg/wiki/ALT://R2E1)
- [ALT://R3A1 - Official GTFO Wiki](https://gtfo.wiki.gg/wiki/R3A1)
