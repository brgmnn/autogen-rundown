# AutogenRundown - GTFO Mod Project Context

## Project Overview
AutogenRundown is a C# mod for GTFO (a 4-player hardcore co-op action horror FPS) that procedurally generates rundowns (mission sets) using seed-based generation. The mod creates Daily, Weekly, Monthly, and Seasonal rundowns with various objectives, enemies, and challenges designed to match the spirit of vanilla GTFO.

## GTFO Core Concepts

### Game Structure
- **Rundown**: A collection of expeditions/levels, typically organized in tiers (A-E)
- **Expedition**: A single mission/level within a rundown
- **Complex**: The underground facility where missions take place (owned by Santonian Industries)
- **Prisoners**: The 4 playable characters (Woods, Dauda, Hackett, Bishop) controlled by players
- **Warden**: The AI entity that gives prisoners their objectives
- **Sleepers**: The main enemies - flesh-eating monsters that infest the Complex

### Mission Objectives
The mod supports all vanilla objectives plus custom variants:
- **HSU Find Sample**: Find a specific Hydro Stasis Unit and extract a tissue sample
- **Reactor Startup/Shutdown**: Fetch verification codes from terminals to operate reactors
- **Terminal Uplink**: Establish network connections with verification codes
- **Corrupted Terminal Uplink**: Uplink where codes are sent to log files
- **Timed Terminal Sequence**: Time-limited sequential terminal commands
- **Gather Items**: Collect small items (IDs, GLPs) or terminals
- **Retrieve Big Items**: Transport large items (Fog Turbines, Neonate HSUs)
- **Power Cell Distribution**: Deliver power cells to generators
- **Central Generator Cluster**: Bring cells to a central cluster
- **Survival**: Timed survival against enemy waves
- **Clear Path**: Navigate to extraction point
- **Special Terminal Command**: Execute specific terminal commands

### Zone Structure
- **Zones**: Separated by security doors, labeled with numbers (Zone 21, etc.)
- **Areas**: Separated by normal doors, labeled with letters
- **Geomorphs**: Tile sets that make up zones (mining, service, labs, etc.)
  - **Hubs**: Large central rooms
  - **I-tiles**: Corridors connecting hubs
  - **Exit tiles**: Special tiles for extraction points

### Alarms & Challenges
- **Class-based Alarms**: Different difficulty levels (Class I-X+)
- **Blood Doors**: Require killing enemies to open
- **Error Alarms**: Cannot be deactivated, must survive
- **Surge Alarms**: Rapid enemy waves
- **S-Class**: Special high-difficulty alarms
- **Fog**: Environmental hazard, can be infectious
- **Security Sensors**: Trigger enemy spawns when detected
- **Locked Terminals**: Require hacking minigame
- **Keys/Cells**: Physical items needed to unlock doors/generators

### Enemies
- **Strikers/Shooters**: Basic melee/ranged enemies
- **Giants**: Large tank-like enemies
- **Chargers**: Fast aggressive enemies
- **Shadows**: Teleporting enemies
- **Berserkers**: Extremely aggressive enemies
- **Hybrids**: Blood door spawns
- **Scouts**: Patrolling enemies (various types: regular, zoomer, shadow, charger, berserker)
- **Mothers**: Boss-type enemies that spawn children
- **Tanks**: Heavily armored boss enemies
- **Snatchers (Pouncers)**: Enemies that grab and drag players

## Technical Architecture

### Framework & Dependencies
- **BepInEx**: Unity plugin framework for game modding
- **IL2CPP**: Unity's intermediate language compilation
- **Harmony**: Runtime method patching library
- **MTFO**: Dependency for custom content injection
- **LocalProgression**: Custom progression tracking system
- **ExtraObjectiveSetup (EOS)**: Advanced objective configuration

### Project Structure
```
AutogenRundown/
├── src/
│   ├── Components/       # Game component classes
│   ├── DataBlocks/       # Game data configuration (70+ files)
│   ├── Events/           # Game event handlers
│   ├── Extensions/       # C# extension methods
│   ├── GeneratorData/    # Seed generation data
│   ├── Json/             # JSON serialization
│   ├── Managers/         # Core system managers
│   ├── Patches/          # Harmony patches
│   ├── PeerMods/         # 3rd party mod support
│   ├── Serialization/    # Data serialization
│   ├── Utils/            # Utility functions
│   ├── BuildDirector.cs  # Build orchestration
│   ├── Generator.cs      # Main generation logic (28KB)
│   └── Plugin.cs         # BepInEx plugin entry point
├── kb/                   # Knowledge base files
│   └── base-game.md      # Comprehensive GTFO game information
├── build/                # Generated datablocks output
└── AutogenRundown.sln    # Visual Studio solution
```

### Key Files
- `Plugin.cs`: Entry point, config management, plugin initialization
- `Generator.cs`: Core procedural generation logic
- `BuildDirector.cs`: Orchestrates datablock generation
- `DataBlocks/*.cs`: Game data configurations for enemies, weapons, zones, objectives, etc.
- `kb/base-game.md`: Comprehensive reference for GTFO game mechanics and lore

### DataBlock System
DataBlocks are JSON configuration files that define game content:
- LevelLayout: Zone structure and connections
- ExpeditionBalance: Enemy spawning configuration
- ChainedPuzzle: Door puzzles and alarms
- WardenObjective: Mission objectives and progression
- Rundown: Overall rundown structure
- FogSettings, LightSettings, EnemyPopulation, etc.

Generated datablocks are output to `build/{GameRevision}/GameData_*_bin.json`

### Seed-Based Generation
- **Daily**: Regenerates each day (format: `YYYY_MM_DD`)
- **Weekly**: Regenerates each week (format: `YYYY_MM_DD`)
- **Monthly**: Regenerates each month (format: `YYYY_MM`)
- **Seasonal**: Regenerates each season (format: `SEASON_YYYY` e.g., `FALL_2025`)
- Seeds can be overridden via BepInEx config for reproducible generation

## Development Workflow

### Build Commands
```bash
# Build the solution
dotnet build AutogenRundown.sln

# Release build
dotnet build -c Release AutogenRundown.sln
```

### Configuration
Configuration is managed via BepInEx config system in `Plugin.cs`:
- Seeds for each rundown type
- RegenerateOnStartup: Controls datablock regeneration
- UsePlayerColorGlowsticks: Client-side cosmetic option

Config file location: `BepInEx/config/000-the_tavern-AutogenRundown.cfg`

### Testing & Debugging
- Game must be run through Steam
- Logs are in `BepInEx/LogOutput.log`
- Generated datablocks in `BepInEx/GameData/{GameRevision}/`
- Watermarks in-game identify mod version and seed info

### Common Tasks
1. **Adding New Objectives**: Extend classes in `DataBlocks/Objectives/`
2. **Modifying Enemy Spawns**: Edit `DataBlocks/ExpeditionBalance.cs`
3. **Adding Geomorphs**: Update geomorph lists in `DataBlocks/LevelLayout.cs`
4. **Adjusting Difficulty**: Modify tier-specific generation in `Generator.cs`
5. **Lore/Intel**: Edit terminal logs in `DataBlocks/` files

## Peer Mod Support
AutogenRundown is compatible with several other mods:
- **Arsenality**: Additional weapons
- **ArsenalityRebalance**: Weapon rebalancing
- **VanillaReloaded**: Enhanced base game
- **GTFriendlyO**: Friendly fire modifications
- **OmegaWeapons**: Extra weapons

Compatibility is managed in `PeerMods/` directory.

## Important Notes

### Plugin Loading Order
Uses GUID `000-the_tavern-AutogenRundown` (prefixed with zeros) to ensure loading before:
- MTFO
- ExtraObjectiveSetup
- EOSExt.Reactor

### Game Version
- Game revision is detected at runtime via `CellBuildData.GetRevision()`
- Datablocks are specific to game revision
- Current version: 0.80.0

### Known Issues & TODOs
Check README.md comments section for current development notes:
- Balance adjustments for specific objectives
- Missing geomorph tiles
- Zone count optimization
- Fog/lighting improvements

## Code Style
- C# 10+ features
- Null-safety annotations
- Extensive use of LINQ
- Procedural generation with deterministic randomness (seeded)
- Datablock generation is idempotent per seed

## Terminology Quick Reference
- **HSU**: Hydro Stasis Unit (containment pod)
- **KDS**: Kovac Defense Services (security contractor)
- **KSO**: Kovac Security Operator (player character type)
- **NAM-V**: The virus that created Sleepers
- **GLP**: Data log items to gather
- **Neonate**: Small HSU pickup item
- **Bioscan**: Scan requiring players to stand in area
- **C-Foam**: Foam used to reinforce doors
- **Matter Wave Projector**: Dimensional travel device
- **The Inner/Fossil**: Ancient alien spacecraft in the crater
- **Collectors**: The alien race who created the Inner

## Resources
- Game Lore: See `kb/base-game.md`
- GTFO Wiki: https://gtfo.fandom.com
- Thunderstore: https://gtfo.thunderstore.io
- GitHub: https://github.com/brgmnn/autogen-rundown
