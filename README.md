# Autogen Rundown 🎲

Automatic Rundown generation, using procedural seed based generation.

## Installation

### With Mod Manager

Go to the Autogen Rundown Thunderstore mod page (https://gtfo.thunderstore.io/package/the_tavern/AutogenRundown/) and install via your mod manager.

### Manual Installation 📦
Download the latest version of the mods manual install zip from the [releases page](https://github.com/brgmnn/autogen-rundown/releases/latest).

> [!IMPORTANT]
> Backup your GTFO game folder before copying any mod files in so you can easily restore it

1. Back up your GTFO game folder (`<Steam Location>\steamapps\common\GTFO`)
  * Right click on GTFO in steam, select "properties", select "Installed Files"
  * Click "Browse..." at the top
  * Windows File Explorer will open inside the GTFO game folder. Navigate up one level and copy/paste the folder. Name it appropriately for your backup (eg. `GTFO_backup`).
1. Extract the `Autogen_manual_install_<Version>.zip` archive into a folder. *Do not extract into the game folder*.
2. Move the contents of `AutogenRundown_GTFO` into the GTFO game folder: `<Steam Location>\steamapps\common\GTFO`.
3. Run the game.

If done correctly, after the game launches you should see a watermark in the bottom right corner of the game identifying the mod version and current seed.

![GTFO Watermark](docs/watermark.jpg)

#### Uninstalling the mod

To uninstall the mod, open `<Steam Location>\steamapps\common` and delete the GTFO game folder `GTFO`. Then rename your backup GTFO game folder (eg. `GTFO_backup`) to `GTFO`.

If you did not make a backup of the game: delete the GTFO game folder and run "Verify integrity of game files" from the Steam "Installed Files" properties menu. This will redownload the GTFO game folder.

#### Updating to the latest mod version

Follow the installation instructions again, check yes to overwrite all files.

## Features progress

* Levels
  * [x] A Tier
  * [x] B Tier
  * [x] C Tier
  * [x] D Tier
  * [x] E Tier
  * Additional objectives
    * [x] Secondary
    * [x] Overload
    * [x] Simple Bulkhead key placement
    * [ ] Chained Bulkhead key placement
  * Lights
    * [x] Random lights selection
    * [x] Reactor specific lights
    * [ ] Fog specific lights
    * [ ] Custom lights(?)
  * Zones
    * Custom Geomorphs
      * [x] Hubs
      * [x] I's (corridors)
      * [ ] Warp portals
    * Curated event layouts
      * [ ] Class 10+ alarm room
      * [ ] King of the hill room
    * [ ] Specialized bulkhead geomorphs and layouts
    * [ ] Bulkheads behind bulkheads
  * Challenges
    * [x] Key to unlock doors
    * [ ] Cell to power generator to unlock doors
    * [ ] Fog
    * [ ] Infectious fog
  * [ ] Dimensions
* Objectives
  * [x] Clear Path — *Navigate through the zones to a separate extraction elevator*
  * [x] Gather Small Items — *IDs, GLPs etc.*
  * [x] HSU Find Sample
  * [ ] Reactor Startup
  * [x] Reactor Shutdown
  * [x] Input Special Terminal Command
  * [x] Retrieve Big Items — *Fog Turbine etc.*
  * [x] Power Cell Distribution — *Distributing cells to individual generators*
  * [x] Terminal Uplink
  * [x] ~~Central Generator Cluster~~ — *Fetching cells for a central generator cluster*. **Unreliable cluster spawns**
  * [ ] HSU Activate Small - *Bring Neonate to depressurizer*
  * [ ] Survival
  * [ ] Gather Terminal
  * [ ] Corrupted Terminal Uplink
  * [ ] Timed Terminal Sequence
* Enemies
  * [x] Basic hybernation
  * [x] Event based activation
  * Types of enemies present
    * [x] Strikers / Shooters
    * [x] Giants
    * [x] Scouts
    * [x] Chargers
    * [x] Shadows
    * [x] Hybrids
    * [x] Mothers
    * [x] Tanks
    * [ ] Pouncers (Snatchers)
  * Custom enemy spawning
    * [ ] Balanced default spawns (in progress)
    * [x] Charger only zones
    * [x] Shadow only zones
* Alarms
  * [x] Basic alarms
  * [x] Blood doors
  * [x] Error alarms
  * [x] S-Class alarms
  * [x] Surge alarms
  * [x] High class alarms (> Class V)
* Glowsticks — *All colors available to spawn*
  * [x] Green (normal)
  * [x] Yellow
  * [x] Halloween (orange)
  * [x] Christmas (red)
