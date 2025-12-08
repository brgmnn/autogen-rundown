# Power Cell Distribution Objective

## Overview

**Power Cell Distribution** (also known as **Distribute Powercells**) is a carry-and-deliver objective in GTFO that requires players to locate power cells and transport them to multiple generators distributed throughout the Complex. Power cells are heavy items that severely reduce movement speed, making carriers vulnerable during transport. This objective demands efficient route planning, team coordination, and resource management as players make multiple trips to power up generators and progress through the mission.

## What Are Power Cells?

Power cells are large, heavy energy storage units used to activate generators within the Complex. Like other big items, power cells must be physically carried by players and impose significant movement penalties. Generators require power cells to function, often controlling critical systems like door locks, zone power, or progression gates.

### Power Cell Sources

Power cells can be found in two ways:
- **Starting Cargo Container**: Given to players at mission start in a crate
- **Scattered in Zones**: Located throughout the expedition in various zones

## Objective Flow

### Phase 1: Locate Power Cells and Generators

1. **Check Warden Objective**: Review how many cells must be distributed
2. **Terminal Search**: Use `LIST CELL` to find all power cell locations
3. **Locate Generators**: Use terminals to identify generator locations
4. **Map the Layout**: Understand spatial relationship between cells and generators
5. **Plan Distribution Route**: Determine optimal order for cell delivery

### Phase 2: Prepare for Distribution

1. **Resource Gathering**: Stock up on ammunition and consumables
2. **Route Planning**: Plan efficient path between cells and generators
3. **Clear Paths**: Pre-clear enemies along distribution routes when possible
4. **Team Role Assignment**: Designate carriers and guards
5. **Progressive Strategy**: Decide between "carry as you go" vs "dedicated trips"

### Phase 3: Power Cell Transport

1. **Pick Up Cell**: Designated carrier picks up power cell
2. **Movement Speed Reduced**: Carrier moves at significantly reduced speed
3. **Team Protection**: Guards escort carrier to generator
4. **Navigate to Generator**: Travel through zones to target generator
5. **Insert Cell**: Deliver power cell to generator to activate it
6. **Repeat**: Return for next cell and continue until all generators powered

### Phase 4: Complete Objective and Extract

1. **Verify All Generators**: Confirm all required generators are powered
2. **Progression Unlock**: Powered generators enable mission progression
3. **Continue Mission**: Proceed to next objective or extraction
4. **Final Extraction**: Navigate to extraction point

## Core Mechanics

### Power Cell Carry Restrictions

When carrying a power cell, players experience:
- **Severe Movement Speed Reduction**: Cannot sprint, drastically reduced walk speed
- **Can Still Use Weapons**: Unlike some carry items, weapons remain usable
- **Can Drop Cell**: Ability to drop cell when needed for combat or repositioning
- **Door Opening**: Can open doors while carrying
- **Flashlight Use**: Can use flashlight while carrying

### Generator Mechanics

**Generator Activation:**
- Requires one power cell per generator
- Insert cell at generator to activate
- Activation may trigger enemy spawns or alarms
- Powers systems (doors, lights, progression gates)

**Generator Distribution Patterns:**
- Multiple generators scattered across zones
- Each requires individual power cell delivery
- Generators may be in dangerous or distant zones

### Two Objective Variants

**Distribute Power Cells (Standard):**
- Cells found centrally or at start
- Generators distributed throughout zones
- Carry cells TO generators

**Central Generator Cluster (Inverse):**
- Cells scattered throughout zones
- Generators in central location
- Retrieve cells FROM zones to central cluster
- Essentially the inverse of Distribute Powercells

## Recommended Strategy

### Planning Phase

1. **Terminal Reconnaissance**:
   - `LIST CELL` - Show all power cell locations
   - `PING CELL_###` - Locate specific cells
   - Map generator locations (use PING or exploration)
2. **Count Resources**: Verify cell count matches generator count
3. **Route Optimization**: Plan route minimizing backtracking
4. **Threat Assessment**: Identify dangerous zones on distribution route

### Progressive Carry Strategy

**"Carry As You Go" Method (Recommended):**
1. Pick up cell at mission start or early zone
2. Carry cell through zones as team clears them
3. Deliver to generator when encountered
4. Pick up next cell in subsequent zones
5. Continue pattern throughout mission

**Advantages:**
- No backtracking required
- Integrates naturally with zone clearing
- Reduces total transport time
- Less vulnerable to ambush

### Dedicated Trip Strategy

**"Make Special Trips" Method:**
1. Clear all zones first without carrying cells
2. Return to cell locations after clearing
3. Make dedicated delivery runs
4. Repeat for each generator

**Advantages:**
- Full combat capability during initial clear
- Can optimize routes after seeing full layout
- Less risk of dropping cells in dangerous situations

**Disadvantages:**
- Significant backtracking
- Time-consuming
- Vulnerable during dedicated delivery runs

### Transport Execution

1. **Carrier Selection**: Choose most survivable player for each trip
2. **Protective Formation**: Team escorts carrier similar to big item retrieval
3. **Clear Before Carry**: Clear zones before entering with cell when possible
4. **Drop for Combat**: Carrier drops cell to fight if overwhelmed
5. **Communication**: Constant callouts of threats and carrier status
6. **Efficient Pathing**: Take most direct safe route to generators

### Generator Activation

1. **Clear Generator Area**: Eliminate enemies around generator first
2. **Prepare for Activation**: Expect alarm or spawns on cell insertion
3. **Insert Cell**: Deliver cell to generator
4. **Manage Consequences**: Survive any triggered alarms or spawns
5. **Secure Area**: Clear spawned enemies before proceeding
6. **Continue Distribution**: Move to next cell/generator pair

## Terminal Commands

- `LIST CELL` - Show all power cell locations in Complex
- `QUERY CELL_###` - Find specific power cell zone location
- `PING CELL_###` - Directional audio ping for cell in current zone
- `LIST RESOURCES` - Find ammo and health stations along routes
- `LIST ZONE_##` - View items in specific zone

## Notable Expeditions

### Distribute Powercells
- **[ALT://R2D1](https://gtfo.fandom.com/wiki/ALT://R2D1)**: Power Cell Distribution objective
- **[ALT://R2B2](https://gtfo.fandom.com/wiki/ALT://R2B2)**: Power Cell Distribution objective
- Various other expeditions feature power cell distribution

### Central Generator Cluster (Inverse Variant)
See [Central Generator Cluster documentation](./central-generator-cluster.md) for the inverse objective variant.

See [Distribute Powercells - Official GTFO Wiki](https://gtfo.fandom.com/wiki/Distribute_Powercells) for complete list.

## Comparison to Related Objectives

| Aspect | Distribute Power Cells | Central Generator Cluster | Retrieve Big Item |
|--------|------------------------|--------------------------|------------------|
| **Item Source** | Central/starting location | Scattered in zones | Single location |
| **Destination** | Generators in zones | Central cluster | Extraction |
| **Number of Deliveries** | Multiple (one per generator) | Multiple (one per cell) | Single item |
| **Movement Speed** | Greatly reduced | Greatly reduced | Severely reduced |
| **Weapon Use** | Available | Available | Prohibited |
| **Distribution Pattern** | Cells to generators | Cells to cluster | Item to extraction |
| **Backtracking** | Varies by strategy | High (retrieval missions) | Low (one-way trip) |
| **Drop Mechanic** | Yes | Yes | Yes |
| **Objective Relationship** | Standard variant | Inverse variant | Different objective |

## Common Challenges

### Movement Speed Penalty

- **Challenge**: Drastically reduced movement speed makes carrier vulnerable
- **Solution**: Clear paths before carrying, maintain protective team formation

### Route Inefficiency

- **Challenge**: Poor route planning leads to excessive backtracking
- **Solution**: Use "carry as you go" strategy, plan optimal sequence beforehand

### Multiple Deliveries

- **Challenge**: Fatigue from making multiple trips with reduced speed
- **Solution**: Split carrier duties among team, take breaks to recover resources

### Generator Activation Alarms

- **Challenge**: Activating generator triggers enemy spawns while vulnerable
- **Solution**: Clear generator area first, prepare defensive positions before insertion

### Cell Management

- **Challenge**: Losing track of which generators are powered and which cells are delivered
- **Solution**: Communicate deliveries, mentally note powered generators, use terminals to verify

### Team Separation

- **Challenge**: Carrier falls behind team due to slow movement
- **Solution**: Team matches pace to carrier, maintains formation discipline

### Dropped Cell Recovery

- **Challenge**: Cell dropped in combat gets lost or surrounded by enemies
- **Solution**: Drop cells in safe positions, prioritize recovery after combat

## Tips

- **Carry As You Go**: Integrate cell delivery into normal zone progression
- **LIST CELL Early**: Use terminal to map all cells before starting distribution
- **Team Escorts Carrier**: Never let carrier travel alone
- **Drop for Combat**: Don't hesitate to drop cell to fight effectively
- **Clear Generator Areas**: Always clear generator surroundings before insertion
- **Count Generators**: Track how many remain to avoid missing any
- **Distribute Carrier Duty**: Share carrying responsibility among team
- **Route Planning**: Plan logical sequence to minimize total distance
- **Expect Alarms**: Anticipate enemy spawns on generator activation
- **Cell Security**: Don't leave dropped cells unattended
- **Communication**: Call out cell pickups, drops, and deliveries
- **Resource Check**: Top off ammo before starting distribution runs
- **Progressive Approach**: Don't try to rush - methodical distribution is safer
- **Terminal Verification**: Use terminals to confirm all cells located and delivered
- **Central Cluster Awareness**: If objective is Central Generator Cluster, expect retrieval pattern
- **Sentry Support**: Place sentries along frequent transport routes
- **C-Foam Paths**: Reinforce doors on transport routes for emergency retreat
- **Match Carrier Pace**: Team moves at carrier's reduced speed
- **Weapon Advantage**: Unlike some carry items, you CAN fight while carrying cells
- **Generator Priority**: If generators in dangerous zones, consider delivering those first while at full resources

## Sources

- [Distribute Powercells - Official GTFO Wiki](https://gtfo.fandom.com/wiki/Distribute_Powercells)
- [Power Cell - Official GTFO Wiki](https://gtfo.fandom.com/wiki/Power_Cell)
- [Generators - Official GTFO Wiki](https://gtfo.fandom.com/wiki/Generators)
- [ALT://R2D1 - Official GTFO Wiki](https://gtfo.fandom.com/wiki/ALT://R2D1)
- [ALT://R2B2 - Official GTFO Wiki](https://gtfo.fandom.com/wiki/ALT://R2B2)
- [ALT://R2D1 - Official GTFO Wiki (wiki.gg)](https://gtfo.wiki.gg/wiki/ALT://R2D1)
- [ALT://R2B2 - Official GTFO Wiki (wiki.gg)](https://gtfo.wiki.gg/wiki/ALT://R2B2)
