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

If done correctly, you should see a console appearing when the game launches.

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
  * [ ] D Tier
  * [ ] E Tier
  * Additional objectives
    * [x] Secondary
    * [x] Overload
    * [ ] Simple Bulkhead key placement
    * [ ] Chained Bulkhead key placement
  * Lights
    * [x] Random lights selection
    * [x] Reactor specific lights
    * [ ] Fog specific lights
    * [ ] Custom lights(?)
  * Zones
    * [ ] More custom geomorphs
    * [ ] Specialized bulkhead geomorphs and layouts
  * [ ] Dimensions
* Objectives
  * [x] Clear Path — *Navigate through the zones to a separate extraction elevator*
  * [x] Gather Small Items — *IDs, GLPs etc.*
  * [x] HSU Find Sample
  * [ ] Reactor Startup
  * [x] Reactor Shutdown
  * [ ] Input Special Terminal Command
  * [ ] Retrieve Big Items — *Fog Turbine etc.*
  * [ ] Power Cell Distribution — *Distributing cells to individual generators*
  * [ ] Terminal Uplink
  * [ ] Central Generator Cluster — *Fetching cells for a central generator cluster*
  * [ ] HSU Activate Small
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
    * [ ] Chargers
    * [ ] Shadows
    * [ ] Hybrids
    * [ ] Mothers
    * [ ] Tanks
    * [ ] Pouncers (Snatchers)
* Alarms
  * [x] Basic alarms
  * [x] Blood doors
  * [x] Error alarms
  * [ ] S-Class alarms
  * [ ] Surge alarms
  * [ ] High class alarms (> Class V)
* Challenges
  * [ ] Fog
  * [ ] Infectious fog
  * [ ] Darkness / Lights change
