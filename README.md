# Autogen Rundown 🎲

![Dynamic JSON Badge](https://img.shields.io/badge/dynamic/json?url=https%3A%2F%2Fthunderstore.io%2Fapi%2Fexperimental%2Fpackage%2Fthe_tavern%2FAutogenRundown%2F&query=%24.latest.version_number&style=flat&label=Version&color=%2300aaff&cacheSeconds=10800)
![Dynamic JSON Badge](https://img.shields.io/badge/dynamic/json?url=https%3A%2F%2Fthunderstore.io%2Fapi%2Fv1%2Fpackage-metrics%2Fthe_tavern%2FAutogenRundown%2F&query=%24.downloads&suffix=%20downloads&style=flat&label=GTFO&color=%23c32918&cacheSeconds=10800)

<!--
* Can we have boosters generate progression for us

JQ Commands for checking blocks:

Check Alarm Persistent IDs for levellayout

  jq -r '.Blocks | map(select(.name | test("^[0-9]+_B1")) | {name, zones: .Zones | map({ChainedPuzzleToEnter})})' build/34855/GameData_LevelLayoutDataBlock_bin.json


TODO:
  * Use https://thunderstore.io/c/gtfo/p/randomguy0753/ZoneScan/ for full room scanning
  * Use Zone Prefix Override to alert to specific tiles.
  * Enable unit tests again after fixing chained puzzles.
  * Don't allow small item pickup to have so many extras that we don't need to do all the zones
  * Make giants blood door a bit harder? B-tier was easy
  * Rework lights on zone to load into lightsettings and let us set our own light settings
  * Make extract scan short if there's no extract alarm (Don't do it just based on level tier)
  * Add a way to automatically install deps?
  * Rising fog?
  * GIGA Infection Hybrid blood door might be _too_ much, it's like 12 hybrids
  * Objective: Distribute Power Cells
    * Needs something to happen when completing all the cells ideally
    * Would be good if the fog could rise or lower when inserting the cells
  * Add more disinfection options for infection levels and hybrids
  * Special Terminal Command:
    * REROUTE_POWER - Should probably have more severe consequences then just turning the lights off?
    * REROUTE_POWER - Main - Is too many sequential zones. 6 zones in a row to go through
  * Add more points of enemies
  * Matter Wave Projector - Needs slightly less zones to go through. Very linear
  * Bring big pickup for scan
  * Add more variety on fog for vibes
  * Rework the special command layout
  * Remove artifact heat from the menus
  * King of the Hill terminal spawns are... dodgy
    * They often spawn inside things.
  * Uplink objectives stop ALL WAVES after completing them. Got to figure out a way to not do that.
  * Retrieve big item:
    * A-tier main only rolled no exit alarm.
  * Add "Assets/AssetPrefabs/Complex/Service/Geomorphs/Maintenance/geo_64x64_service_floodways_hub_AW_01.prefab", the giant HSU pit. Technically an I-tile
  * Weekly #40 A1 failed to generate objective room
  * Error thrown from EventManager.UpdateRundown. I suspect it might be from the daily.
  * Check out Seasonal D4. End room that does nothing?
  * Add support for text data block mergers. It should be relatively safe to do this even with overwrites

NOTE
  * Make sure we match the complex for `Reality` with the dimension geo we want. This is for the terminals to spawn correctly

Idea:
  * Secondary survival that procs on door open
  * Side quest room to lower the fog
  * Bring big pickup for scan on door
  * Add full team scan on extract instead of warden scan
  * R8E2 "ADMIN_TEMP_OVERRIDE" command level
    * Keep putting in command to keep surge error at bay
  * Clear path to a warp portal that jumps you to the success screen 

# Datasphere color pack
https://thunderstore.io/c/gtfo/p/ProjectZaero/Data_Sphere_Flavor_Pack/

# Geo pack ideas
https://thunderstore.io/c/gtfo/p/TheDoggyDoge/DogsTilePack/
https://thunderstore.io/c/gtfo/p/xiaoyao/XAOYAO_developer_package/


Level Lockup:
  Double check logs on any of the old versions for this:
  ```
  ZoneNode { Bulkhead = None, ZoneNumber = 0, Branch = , MaxConnections = 0, Tags =  }
  ```
  There was a bug in the error alarm turnoff code where it would try and place a turnoff alarm from a nonexistent zone
  ```
  [Error  :     Unity] WARNING : Zone1 (Zone_1 - 224): Failed to find any good StartAreas in zone 0 (223) expansionType:Towards_Random m_buildFromZone.m_areas: 0 scoredCount:0 dim: Reality
  ```
  Happens in:
    LG_ZoneJob_CreateExpandFromData
  Second error:
  ```
  [Error  :     Unity] NullReferenceException: Object reference not set to an instance of an object.
  [Error  :     Unity] <b>LG_ZoneJob_CreateExpandFromData:</b> sourceExpander is null for New Game Object[SubSeed:24(24)] | zone dim: Reality | job dim: Reality!
  ```

Requests:
  * Add support for VanillaReloaded mod
    * https://discord.com/channels/782438773690597389/1342735255009755227/1373594263019061281

Check: https://discord.com/channels/782438773690597389/783918553626836992/1407457297889890530


Problems:
  - Assets/AssetPrefabs/Complex/Mining/Geomorphs/geo_64x64_mining_portal_HA_01.prefab
    - Has some innacessible spawn locations where boxes and big pickups can spawn.
    - There are two side flanges next to the entry door for the portal room which are blocked but still marked as spawn locations


2025_08_31
  * Seasonal Fall_2025 - A4 Decaying Banishment: Pretty much perfect short A level. 26 mins, 5 zones, straight forward clear path
    * Maybe too high scan levels? 2 class 5 and 2 class 3

2025_08_27
  * Generator cluster needs a little less zones

2025_08_17
  * Reduce number of gather so we have to do all objectives
  * Why does D2 only have onion shooters on first alarm door in extreme

2025_04_29
  * D1: Error alarm with no turn off is quite a bit harder
    * Probably we need a few less alarms
    * Might want to have fewer enemies on reactor fetch code tiles
    * D1 had A LOT of zones to run through, may want to not have multiple zones of fetch codes and do single zone fetch codes
    * For fetch codes that have zones not starting at reactor, need to factor traverse time to get there and back
  * D1 (or D2?)
    * Need to fix the zone count for layout for reactors
      * Maybe consider using a different layout for it?
    * Consider if we reduce number of fetch codes from 6 to 5
    * May need to BUFF the reactor waves a little

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
  [ ] Check if we can use a mod like https://thunderstore.io/c/gtfo/p/W33B/Arsenality/
    * Seems like it adds datablock files directly
  * Try using fixed bioscan points for room wide scans?
  [ ] B1 Secondary (Cryo) - No exit alarm
  [ ] C4 - Security sensors spawn LOADS of snatchers
  [ ] E1 - Maybe a bit too easy?
  [ ] D1 - Activate small HSU doesn't require the objective to exit
      - Can we exclude double mother / double tank from first zone

Monthly March
  * March 27th
    * E2
      * Time maybe too generous, finished with 2:30 left
  * Yoshi Feedback
    [ ] May want to look in to making it so error alarm terminals don't cancel others. Many instances of trivialized scans because of error turnoff.

Monthly Feb
  * Double cargo B1 -- Not sure why?
  * Reactor D2
    [ ] Mother wave had good amount of time. DONT CHANGE

Missing tiles to add
  * Add R8A1 garden hub tile to hubs


Enemies:
  A:
    * Baseline
  B:
    * Baseline
  C:
    * Baseline
    * Chargers
    * Nightmares
  D:
    * Baseline
    * Chargers
    * Shadows
    * Nightmares
  E:
    * Baseline
    * Chargers
    * Shadows
    * Nightmares

-->

Automatic Rundown generation, using procedural seed based generation. Four active rundowns to choose from: the daily, weekly, monthly, and each season. Play with friends with zero configuration of seeds.

* [Installation](#installation)
* [Base game features](#base-game-features)
* [Additional vanilla-like features](#additional-vanilla-like-features)
* [3rd Party Mod Support](#3rd-party-mod-support)

![Rundown Selection](/docs/rundown_selection_loop.apng "Rundown Selection")

Track your progression in each rundown separately from the base game and other modded rundowns, see if you can clear them "Unaugmented"! (Without boosters)

![Monthly Rundown Preview](https://github.com/brgmnn/autogen-rundown/blob/main/docs/monthly_rundown.jpg?raw=true "Monthly Rundown Preview")

Levels and rundowns are designed to be similar and in the spirit of vanilla GTFO. The largest difference is in the addition of new tilesets (geomorphs) to add more variety to the existing games set of tiles.


## Installation

<!-- ### With Mod Manager -->

Go to the Autogen Rundown Thunderstore mod page (https://gtfo.thunderstore.io/package/the_tavern/AutogenRundown/) and install via your mod manager.

<!-- ### Manual Installation 📦
Download the latest version of the mods manual install zip from the [releases page](https://github.com/brgmnn/autogen-rundown/releases/latest).

> [!IMPORTANT]
> Backup your GTFO game folder before copying any mod files in so you can easily restore it

1. Back up your GTFO game folder (`<Steam Location>\steamapps\common\GTFO`)
  * Right click on GTFO in steam, select "properties", select "Installed Files"
  * Click "Browse..." at the top
  * Windows File Explorer will open inside the GTFO game folder. Navigate up one level and copy/paste the folder. Name it appropriately for your backup (eg. `GTFO_backup`).
1. Extract the `Autogen_manual_install_<Version>.zip` archive into a folder. *Do not extract into the game folder*.
2. Move the contents of `AutogenRundown_GTFO` into the GTFO game folder: `<Steam Location>\steamapps\common\GTFO`.
3. Run the game. -->

To check if the installation is successful, after the game launches you should see a watermark in the bottom right corner of the game identifying the mod version `AR <version>`. If you further select a level from one of the rundowns you should also see a watermark identifying the leve, the rundown, and the seed used for that rundown.

![GTFO Watermark](docs/watermark.jpg)

<!-- #### Uninstalling the mod

To uninstall the mod, open `<Steam Location>\steamapps\common` and delete the GTFO game folder `GTFO`. Then rename your backup GTFO game folder (eg. `GTFO_backup`) to `GTFO`.

If you did not make a backup of the game: delete the GTFO game folder and run "Verify integrity of game files" from the Steam "Installed Files" properties menu. This will redownload the GTFO game folder.

#### Updating to the latest mod version

Follow the installation instructions again, check yes to overwrite all files. -->


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
    * [x] Specialized bulkhead geomorphs and layouts
    * [x] Bulkheads behind bulkheads
  * Challenges
    * [x] Key to unlock doors
    * [x] Cell to power generator to unlock doors
    * [x] Locked terminals
    * [x] Fog
    * [x] Infectious fog
    * [x] Security Sensors
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
  * [x] Central Generator Cluster — *Fetching cells for a central generator cluster*
  * [x] HSU Activate Small - *Bring Neonate to depressurizer*
  * [x] Survival - *Timed survival*
  * [x] Gather Terminal
  * [x] Corrupted Terminal Uplink
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
    * [x] Mothers
      * [x] Mother
      * [x] P-Mother
      * [x] Nightmare Mother
    * [x] Tanks
      * [x] Tank
      * [ ] Immortal Tank
      * [x] Potato Tank
    * [x] Pouncers (Snatchers)
  * Custom enemy spawning
    * [x] Balanced default spawns (in progress)
    * [x] Charger only zones
    * [x] Shadow only zones
    * [x] Beserker only zones
* Alarms
  * [x] Basic alarms
  * [x] Blood doors
  * [x] Error alarms
  * [x] S-Class alarms
  * [x] Surge alarms
  * [x] High class alarms (> Class V)
* Lore
  * [x] All base game logs can be found on terminals


## Additional vanilla-like features

* Alarms
  * [x] Secret Alarms — *Appear to be free but aren't*
  * [x] Events on/during Alarms — *Alarms can have events such as lights out or fog flood during the alarm*
  * [x] Extreme/Overload Surge Alarms — Surge alarms with chargers / beserkers
* Challenges
  * [x] Reactor terminals can be locked just like regular terminals
  * [x] Secret Reactor Shutdown — *Secret alarm but for the reactor shutdown*
* Cosmetic
  * Glowsticks — *All colors available to spawn*
    * [x] Green (normal)
    * [x] Yellow
    * [x] Halloween (orange)
    * [x] Christmas (red)
    * [x] Player colored glowsticks (client side)
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
      * [ ] Central Generator Cluster


## 3rd Party Mod Support

AutogenRundown supports the following 3rd party peer mods. You can install them along side AutogenRundown. Note that several of these mods conflict with each other such as the weapon modification mods.

* [Arsenality](https://thunderstore.io/c/gtfo/p/W33B/Arsenality)
* [ArsenalityRebalance](https://thunderstore.io/c/gtfo/p/leezurli/ArsenalityRebalance)
* [VanillaReloaded](https://thunderstore.io/c/gtfo/p/tru0067/VanillaReloaded)


## Acknowledgements

Many thanks to the modding community for making GTFO modding possible, and a special thank you to the following creators for work that this mod depends on:

* Dak's [MTFO](https://thunderstore.io/c/gtfo/p/dakkhuza/MTFO/) and [Geomorph Pack](https://thunderstore.io/c/gtfo/p/dakkhuza/DakGeos/)
* Inas07's [many mods](https://thunderstore.io/c/gtfo/p/Inas07/)
  * Notably [LocalProgression](https://thunderstore.io/c/gtfo/p/Inas07/LocalProgression/) and [ExtraObjectiveSetup](https://thunderstore.io/c/gtfo/p/Inas07/ExtraObjectiveSetup/)
* Flowaria's [Geomorph Pack](https://thunderstore.io/c/gtfo/p/Flowaria/FlowGeos/)
  * Including the fantastic Floodways Reactor tile enables reactor missions in the service Complex.
* donan3967's [Geomorph Pack 1](https://thunderstore.io/c/gtfo/p/donan3967/donan3967_geo_pack_1/) and [Geomorph Pack 2](https://thunderstore.io/c/gtfo/p/donan3967/donan3967_geo_pack_2/)
* Red_Leicester_Cheese's [Geomorph Pack](https://thunderstore.io/c/gtfo/p/Red_Leicester_Cheese/CheeseGeos/)
* SamDB's [SamGeos](https://thunderstore.io/c/gtfo/p/Sam_D_B/SamGeos/) and [SamGeosV2](https://thunderstore.io/c/gtfo/p/Sam_D_B/SamGeosV2/)
