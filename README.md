# Autogen Rundown 🎲

![Dynamic JSON Badge](https://img.shields.io/badge/dynamic/json?url=https%3A%2F%2Fthunderstore.io%2Fapi%2Fexperimental%2Fpackage%2Fthe_tavern%2FAutogenRundown%2F&query=%24.latest.version_number&style=flat&label=Version&color=%2300aaff&cacheSeconds=10800)
![Dynamic JSON Badge](https://img.shields.io/badge/dynamic/json?url=https%3A%2F%2Fthunderstore.io%2Fapi%2Fv1%2Fpackage-metrics%2Fthe_tavern%2FAutogenRundown%2F&query=%24.downloads&suffix=%20downloads&style=flat&label=GTFO&color=%23c32918&cacheSeconds=10800)

<!--
* Can we have boosters generate progression for us

JQ Commands for checking blocks:

Check Alarm Persistent IDs for levellayout

  jq -r '.Blocks | map(select(.name | test("^[0-9]+_B1")) | {name, zones: .Zones | map({ChainedPuzzleToEnter})})' build/34855/GameData_LevelLayoutDataBlock_bin.json


TODO:
  * Use Zone Prefix Override to alert to specific tiles.
  * Enable unit tests again after fixing chained puzzles.
  * Don't allow small item pickup to have so many extras that we don't need to do all the zones
  * Make giants blood door a bit harder? B-tier was easy
  * Rework lights on zone to load into lightsettings and let us set our own light settings
  * Make extract scan short if there's no extract alarm (Don't do it just based on level tier)
  * Add a way to automatically install deps?

Weekly 9-A1 bug:
  * Doesn't seem to be the chained puzzles.. Unclear for now
  * Disappears if I disable EITHER overload or secondary
  * It appears to be the map layout. I suspect it's the door not having a place to place though

Exit Geo failures to spawn:
  * Assets/Bundles/RLC_Mining/geo_64x64_mining_storage_exit_hub_RLC_01.prefab
  * Assets/geo_64x64_dig_site_exit_dak_01.prefab
Working exit geos
  * Assets/Prefabs/Geomorph/Mining/geo_storage_FA_dead_end_01.prefab

  Note: It may well be a space issue. The two failed tiles are quite large, so when they don't spawn it might just be that there isn't space for them.

Monthly April
  * Find HSU seems like it needs re-rolling occasionally. May be some of the levels can't place well
  * Need to take another look at balance for reactor shutdown. D/E looks to either be way too hard or way too easy.

Monthly March
  * March 27th
    * E2
      * Time maybe too generous, finished with 2:30 left
      *
  * Yoshi Feedback
    [ ] May want to look in to making it so error alarm terminals don't cancel others. Many instances of trivialized scans because of error turnoff.

Monthly Feb
  * Double cargo B1 -- Not sure why?
  * March
    [x] A1 - Reactor shutdown main no exit trickle
    [x] B2 - Overload: need to add time for doing king of the hill. it's not possible with how much time it takes to not get bonus time
              See if we can use AWO for setting the timer with additional objectives?
    [x] B2 - Extreme: Survival timer still doesn't add time, instead resets time with new value and then is reset back to starting time
    [x] B2 - Survival timer: On B-tier at least it seems like a tiny bit more ammo would be good.
    [x] C1 - Overload: got the mega mother room for the terminal uplink. - Getting hubs for uplink rooms is no good

  * Reactor D2
    [ ] Need to ensure resources spawn. Flat mining reactor doesn't have enoug spawn points, no tool spawned?
    [ ] Possibly reduce the number of waves but up the difficulty. First 3-4 waves were easy and a bit of nothing
    [x] Increase time granted for clearing zones. Maybe double the time granted per point of eneies? Feels like they need a tiny bit more.
    [ ] Increase resources in side rooms. Requires a lot of shooting and hard to find any resources in larger rooms
    [ ] May consider reducing the number of zones?
    [ ] Mother wave had good amount of time. DONT CHANGE
Monthly Feb 0.49.0
  * Secondary or otherwise key seems to always spawn in the elevator
  * C2 Extreme
    * Bug: Bulkhead Key spawned in the elevator drop?????!!!!
      * It was in the Cheese Geos hub
    * Bug: No power cells spawned in secondary
    * All of these bugs seem to be from that Cheese Geos tile!!!!
  * C2 Main
    * Keycard spawned deep in zone instead of in side zone
  * D4 Main
    * _Very_ difficult first room scan: tbd if we should fix this somehow? No doors to hold or anything

Missing tiles to add
  * Add R8A1 garden hub tile to hubs

Seed: 2024_12_19
* Monthly: B1 Extreme: reactor startup zone unlocks and intel aren't quite right. Use Intel instead

Seed: 2024_12_18
* Still too many zones in A retrieve big items

Probably need to rework enemy sleeper spawning now with level planner

Seed: 2024_07_08
* Boosters confirmed not working for modded rundowns

-->

Automatic Rundown generation, using procedural seed based generation. Three active rundowns to choose from: the daily, weekly, and monthly. Play with friends with zero configuration of seeds.

![Rundown Selection](https://github.com/brgmnn/autogen-rundown/blob/03583187a757755e486623da73d4712e00187be7/docs/rundown_selection.jpg?raw=true "Rundown Selection")

Track your progression in each rundown separately from the base game and other modded rundowns, see if you can clear them "Unaugmented"! (Without boosters)

![Monthly Rundown Preview](https://github.com/brgmnn/autogen-rundown/blob/03583187a757755e486623da73d4712e00187be7/docs/monthly_rundown.jpg?raw=true "Monthly Rundown Preview")

Levels and rundowns are designed to be similar and in the spirit of vanilla GTFO. The largest difference is in the addition of new tilesets (geomorphs) to add more variety to the existing games set of tiles.

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


> [!CAUTION]
> GTFO game and mod spoilers below!

## Base game features

* Levels
  * [x] A Tier
  * [x] B Tier
  * [x] C Tier
  * [x] D Tier
  * [x] E Tier
  * Additional objectives
    * [x] Secondary
    * [x] Overload
  * Lights
    * [x] Random lights selection
    * [x] Reactor specific lights
    * [ ] Fog specific lights
    * [x] Custom lights(?)
  * Zones
    * Custom Geomorphs
      * [x] Hubs
      * [x] I's (corridors)
      * [ ] Warp portals
      * [x] Rundown 8 Geomorphs
    * Curated event layouts
      * [x] Class 10+ alarm room
      * [x] King of the hill room
    * [ ] Specialized bulkhead geomorphs and layouts
    * [ ] Bulkheads behind bulkheads
  * Challenges
    * [x] Key to unlock doors
    * [x] Cell to power generator to unlock doors
    * [x] Locked terminals
    * [x] Fog
    * [x] Infectious fog
    * [ ] Security Sensors
  * [ ] Dimensions
* Objectives
  * [x] Clear Path — *Navigate through the zones to a separate extraction elevator*
  * [x] Gather Small Items — *IDs, GLPs etc.*
  * [x] HSU Find Sample
  * [x] Reactor Startup
    * [x] Fetch codes from terminals
  * [x] Reactor Shutdown
  * [x] Input Special Terminal Command
  * [x] Retrieve Big Items — *Fog Turbine etc.*
  * [x] Power Cell Distribution — *Distributing cells to individual generators*
  * [x] Terminal Uplink
  * [x] ~~Central Generator Cluster~~ — *Fetching cells for a central generator cluster*. **Unreliable cluster spawns**
  * [ ] HSU Activate Small - *Bring Neonate to depressurizer*
  * [x] Survival - *Timed survival*
  * [ ] Gather Terminal
  * [ ] Corrupted Terminal Uplink
  * [x] Timed Terminal Sequence
* Enemies
  * [x] Basic hybernation
  * [x] Event based activation
  * Types of enemies present
    * [x] Strikers / Shooters
    * [x] Giants
    * [x] Chargers
    * [x] Shadows
    * [x] Beserkers
    * [x] Hybrids
    * [x] Scouts
      * [x] Regular
      * [x] Zoomer
      * [x] Shadow
      * [x] Charger
      * [x] Beserker
    * [-] Mothers
      * [x] Mother
      * [x] P-Mother
      * [ ] Nightmare Mother
    * [-] Tanks
      * [x] Tank
      * [ ] Immortal Tank
      * [x] Potato Tank
    * [x] Pouncers (Snatchers)
  * Custom enemy spawning
    * [ ] Balanced default spawns (in progress)
    * [x] Charger only zones
    * [x] Shadow only zones
    * [ ] Beserker only zones
* Alarms
  * [x] Basic alarms
  * [x] Blood doors
  * [x] Error alarms
  * [x] S-Class alarms
  * [x] Surge alarms
  * [x] High class alarms (> Class V)


## Additional vanilla-like features

* Alarms
  * [x] Secret Alarms — *Appear to be free but aren't*
  * [x] Events on/during Alarms - *Alarms can have events such as lights out or fog flood during the alarm*
* Challenges
  * [x] Reactor terminals can be locked just like regular terminals
  * [x] Secret Reactor Shutdown - *Secret alarm but for the reactor shutdown*
* Cosmetic
  * Glowsticks — *All colors available to spawn*
    * [x] Green (normal)
    * [x] Yellow
    * [x] Halloween (orange)
    * [x] Christmas (red)
  * Inserting generator cells turns on emergency lights
* Lore
  * Custom Warden intel messages
    * [x] General
    * [ ] Objectives
      * [x] HSU Find Sample
      * [ ] Clear Path
      * [ ] Reactor Startup
      * [ ] Reactor Shutdown
      * [ ] Distribute Power Cells

## Acknowledgements

Many thanks to the modding community for making GTFO modding possible, and a special thank you to the following creators for work that this mod depends on:

* Dak's [MTFO](https://thunderstore.io/c/gtfo/p/dakkhuza/MTFO/) and [Geomorph Pack](https://thunderstore.io/c/gtfo/p/dakkhuza/DakGeos/)
* Inas07's [many mods](https://thunderstore.io/c/gtfo/p/Inas07/)
  * Notably [LocalProgression](https://thunderstore.io/c/gtfo/p/Inas07/LocalProgression/) and [ExtraObjectiveSetup](https://thunderstore.io/c/gtfo/p/Inas07/ExtraObjectiveSetup/)
* Flowaria's [Geomorph Pack](https://thunderstore.io/c/gtfo/p/Flowaria/FlowGeos/)
  * Including the fantastic Floodways Reactor tile enables reactor missions in the service Complex.
* donan3967's [Geomorph Pack](https://thunderstore.io/c/gtfo/p/donan3967/donan3967_geo_pack_1/)
* Red_Leicester_Cheese's [Geomorph Pack](https://thunderstore.io/c/gtfo/p/Red_Leicester_Cheese/CheeseGeos/)
