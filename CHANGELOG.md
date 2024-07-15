# Changelog

See more at https://github.com/brgmnn/autogen-rundown


## [v0.32.0](https://github.com/brgmnn/autogen-rundown/releases/tag/v0.32.0) — July 14, 2024

### Changes

* Fix `Survival` objective go to elevator text
* Added Nightmare/Zoomer scouts to E-tier
* Improve zone time factor estimates. For details see `GetClearTimeEstimate` in `Zone`. Time estimates now add time base on the following factors:
  * Zone size, also increased time factor for coverage by 17%
  * Alarm to enter zone: number of scans, sum of duration of each scan, distance between scans
  * Blood doors to enter zone
  * Total enemy points in the zone
  * Fixed times per boss in zone and behind blood doors


## [v0.31.0](https://github.com/brgmnn/autogen-rundown/releases/tag/v0.31.0) — July 13, 2024

### New

* Add `Survival` objective 🎉

### Changes

* Fix incorrect spawn location for HSU Find Sample objective

**Full Changelog**: https://github.com/brgmnn/autogen-rundown/compare/v0.30.0...v0.31.0


## [v0.30.0](https://github.com/brgmnn/autogen-rundown/releases/tag/v0.30.0) — July 13, 2024

### New

* Added Power Cell Distribution objective to Secondary/Overload objectives.
* Added `FLUSH_VENTS` variant of Special Terminal Command objective (bring your fog repellers!)

### Changes

* Fixed issue with HSU Find Sample and some related zone placement code not returning any valid zones. Should fix several cases of Autogen crashes on startup
* Increased relative chance of Service complex spawning from 50% to 70%. Mining / Tech have 100% relative chance.

**Full Changelog**: https://github.com/brgmnn/autogen-rundown/compare/v0.29.0...v0.30.0


## [v0.29.0](https://github.com/brgmnn/autogen-rundown/releases/tag/v0.29.0) — July 12, 2024

### Changes

* Fix `Complex` not being set in D-tier reactor startup mission. Reactor tile should spawn correctly now.
* Adjust reactor startup code fetch terminal placements from `AtEnd` to `NotAtStart`. Terminals will now spawn in the middle/end of the zone instead of just at the end of the zone.
* Update `GatherSmallItems`, `SpecialTerminalCommand`, and `HSUFindSample` to use layout Planner both for zone placement and objective item placement. This should fix issues with items spawning before the Main bulkhead door.
* Slightly reduce chance of Prisoner Efficiency missions. This is temporary to help reduce chance of level generation lockup.

**Full Changelog**: https://github.com/brgmnn/autogen-rundown/compare/v0.28.0...v0.29.0


## [v0.28.0](https://github.com/brgmnn/autogen-rundown/releases/tag/v0.28.0) — July 11, 2024

### New

* Fixed artifact collection to work

### Changes

* Changed D-tier reactor level to use random complex
* Fixed enemy spawning not happening in all zones

**Full Changelog**: https://github.com/brgmnn/autogen-rundown/compare/v0.27.0...v0.28.0


## [v0.27.0](https://github.com/brgmnn/autogen-rundown/releases/tag/v0.27.0) — July 10, 2024

### New

* Added compact starting area spawns for 2x and 3x bulkhead
* Added a _surprise_ to Reactor Startup

### Changes

* Buffed some of the reactor D-tier enemy waves

**Full Changelog**: https://github.com/brgmnn/autogen-rundown/compare/v0.26.0...v0.27.0


## [v0.26.0](https://github.com/brgmnn/autogen-rundown/releases/tag/v0.26.0) — July 10, 2024

### New

* Starting Area of level can now roll compact hub zone layouts for 2x and 3x bulkheads. These starting areas will only have 1 zone in the elevator drop zone.

### Changes

* Add additional Warden Intel messages for HSU and Terminal Uplink main objectives.

**Full Changelog**: https://github.com/brgmnn/autogen-rundown/compare/v0.25.0...v0.26.0


## [v0.25.0](https://github.com/brgmnn/autogen-rundown/releases/tag/v0.25.0) — July 08, 2024

### New

* Added fixed E-tier Clear Path objective level.

### Changes

* Reduced number of zones before reaching reactors in Startup/Shutdown missions. Reactor I-tile/corridor now spawns immediately after the bulkhead door instead of several preamble zones.
* Increased reactor base wave duration back to 60s from 45s. All reactor waves should now have an extra 15s.
* Fixed incorrect bundling of dependency plugins in manual install package.
* Added custom README for Thunderstore listing and CHANGELOG file.

**Full Changelog**: https://github.com/brgmnn/autogen-rundown/compare/v0.24.2...v0.25.0


## [v0.24.2](https://github.com/brgmnn/autogen-rundown/releases/tag/v0.24.2) — July 07, 2024

### Changes

* Fixed fatal missing directory error on first time startup

**Full Changelog**: https://github.com/brgmnn/autogen-rundown/compare/v0.24.1...v0.24.2


## [v0.24.1](https://github.com/brgmnn/autogen-rundown/releases/tag/v0.24.1) — July 07, 2024

### Changes

* Fixed folder structure issue causing crash on startup

**Full Changelog**: https://github.com/brgmnn/autogen-rundown/compare/v0.24.0...v0.24.1


## [v0.24.0](https://github.com/brgmnn/autogen-rundown/releases/tag/v0.24.0) — July 06, 2024

### New

* Added Reactor Geomorph for Service Complex. Reactor startup/shutdown missions previously could not be used in the Service complex (Floodways, Gardens) as there was no geomorph in those complex tilesets. This release adds a dependency on FlowGeos which provides a reactor geo for floodways.

### Changes

* Reactor Startup D-tier mission is now a fixed regular level, not just a test level
* Fixed Service complex to use both Floodways and Gardens
* Fixed several warnings being thrown by duplicate alarm names
* Fixed `Baseline_Hard` wave settings including `Weakling` enemy types. This fixes many waves spawning shadows which should not have.

**Full Changelog**: https://github.com/brgmnn/autogen-rundown/compare/v0.23.2...v0.24.0


## [v0.23.2](https://github.com/brgmnn/autogen-rundown/releases/tag/v0.23.2) — July 03, 2024

### Changes

* Fixed `Class S 1` alarms duration being too short. Duration is now 100s (was previously 25s)
* Increased size of code fetch terminal zones to try and reduce chance of soft locks with bad terminal placements

**Full Changelog**: https://github.com/brgmnn/autogen-rundown/compare/v0.23.1...v0.23.2


## [v0.23.1](https://github.com/brgmnn/autogen-rundown/releases/tag/v0.23.1) — July 02, 2024

### Changes

* Disable keyed zone rolls as there are still map generation hard locks happening with it

**Full Changelog**: https://github.com/brgmnn/autogen-rundown/compare/v0.23.0...v0.23.1


## [v0.23.0](https://github.com/brgmnn/autogen-rundown/releases/tag/v0.23.0) — July 02, 2024

### Changes

* Added Zoomer and Nightmare scouts to rolls
* Added Nightmare wave to reactor startup
* Added Tank in baseline wave to reactor startup
* Added additional Potato Tank rolls to reactor startup waves
* Fixed baseline charger / nightmare reactor waves using baseline hybrid wave populations
* Reduced Reactor hybrids only wave from 28pts to 16pts
* Reduced reactor wave fixed base time from 60s to 45s

**Full Changelog**: https://github.com/brgmnn/autogen-rundown/compare/v0.22.0...v0.23.0


## [v0.22.0](https://github.com/brgmnn/autogen-rundown/releases/tag/v0.22.0) — June 30, 2024

### New

* Added some Nightmare enemies for several alarms

### Changes

* Fixed `Class V Cluster` alarm only having 4 scans
* Rebalanced D-tier alarms
* Reduced number of zones spawned for each area and weighted distribution of zone count selection
* Automated release artifacts upload

**Full Changelog**: https://github.com/brgmnn/autogen-rundown/compare/v0.21.0...v0.22.0


## [v0.21.0](https://github.com/brgmnn/autogen-rundown/releases/tag/v0.21.0) — June 30, 2024

### Changes

* Reduce health multiplier in Reactor Startup
* Slightly reduce time factor for enemies/coverage in Reactor Startup fetch codes

**Full Changelog**: https://github.com/brgmnn/autogen-rundown/compare/v0.20.0...v0.21.0


## [v0.21.0-beta](https://github.com/brgmnn/autogen-rundown/releases/tag/v0.21.0-beta) — June 30, 2024

### Changes

Test release for verifying automated workflows

**Full Changelog**: https://github.com/brgmnn/autogen-rundown/compare/v0.20.0...v0.21.0-beta


## [v0.20.0](https://github.com/brgmnn/autogen-rundown/releases/tag/v0.20.0) — June 27, 2024

### Changes

* Reduced difficulty of reactor base waves
  * Easy:
    * Points: 40pts -> 30pts
    * Duration: 30s -> 40s
  * Medium:
    * 60pts -> 40pts
    * Duration: 60s -> 50s
  * Hard: 
    * Points: 80pts -> 50pts
    * Duration: 70s -> 60s
* Reworked D-tier reactor waves for more variety
* Added Tank Potato as possible D-tier reactor enemy
* Slightly increased time scale factor for D-tier in all metrics
* Reduced resource multiplier for D-tier Reactor Startup
* Increase reactor wave times by 15s
* Added more lore text for Reactor Startup and Reactor Shutdown objectives

**Full Changelog**: https://github.com/brgmnn/autogen-rundown/compare/v0.19.0...v0.20.0


## [v0.19.0](https://github.com/brgmnn/autogen-rundown/releases/tag/v0.19.0) — June 26, 2024

### Changes

* Reworked door key puzzle generation. Keys for locked doors should no longer spawn behind those doors. Fixes #9
* Added additional zone direction hints to help with generation deadlocks
* Moved test level to D-tier

**Full Changelog**: https://github.com/brgmnn/autogen-rundown/compare/v0.18.2...v0.19.0


## [v0.18.2](https://github.com/brgmnn/autogen-rundown/releases/tag/v0.18.2) — June 25, 2024

### Changes

* Added direction hinting for bulkhead zone generations
* Updated required version of `Inas07-LocalProgression`
* Updated vanilla data blocks for `WavePopulation` and `WaveSettings`.

**Full Changelog**: https://github.com/brgmnn/autogen-rundown/compare/v0.18.1...v0.18.2


## [v0.18.1](https://github.com/brgmnn/autogen-rundown/releases/tag/v0.18.1) — January 01, 2024

### Changes

* Issue with mod being flagged by the automod in Thunderstore due to some NuGet packages which are already bundled by BepInEx.
  * The fix is to use the package from BepInEx instead of from NuGet.


## [v0.18.0](https://github.com/brgmnn/autogen-rundown/releases/tag/v0.18.0) — January 01, 2024

### Changes

* Rundown 8 game updated! 🥳 


## [v0.17.1](https://github.com/brgmnn/autogen-rundown/releases/tag/v0.17.1) — November 25, 2023

### Changes

* Reactor Startup
  * Increased reactor enemy wave population points totals for Easy / Medium / hard.
    * Easy: 25pts to 40pts
    * Medium: 30pts to 60pts
    * Hard: 40pts to 80pts
  * Increased wave failed time for fetch code waves from 30s to 1/2 the verify time.
  * Increased wave verify time granted from puzzle component travel distance.


## [v0.17.0](https://github.com/brgmnn/autogen-rundown/releases/tag/v0.17.0) — November 24, 2023

### New

* Reactor Startup - Code fetching 💻
  * _Reactor startup objectives can now spawn with additional areas that must be access to fetch codes from terminals_
* Set manual install BepInEx logging to disk to `All`

### Changes

* Issue where some fixed data blocks were being saved last instead of first
* Added workaround to reduce chance of failed level generation on Reactor Startup
* Adjusted "Class S I Alarm" to use 100s sustained scan instead of 140s scan.


## [v0.16.0](https://github.com/brgmnn/autogen-rundown/releases/tag/v0.16.0) — November 21, 2023

### New

* New objective! Reactor Startup
  * Currently with simple input codes (no code fetching yet)
* Chance for triple bulkhead starting area to spawn in Digsite using `Assets/AssetPrefabs/Complex/Mining/Geomorphs/Digsite/geo_64x64_mining_dig_site_hub_SF_01.prefab`

### Changes

* Matter Wave Projector objective having the wrong help text
* Zone numbers possibly colliding between Main/Secondary/Overload


## [v0.15.0](https://github.com/brgmnn/autogen-rundown/releases/tag/v0.15.0) — November 19, 2023

### New

* Big pickup item - Matter Wave Projector: Custom level layout generation
* Fog turbine in starting area zones for fog levels

### Changes

* Big pickup having a low number of zones for single or two items.


## [v0.14.0](https://github.com/brgmnn/autogen-rundown/releases/tag/v0.14.0) — November 19, 2023

### New

* Fog level modifiers!
  * B-Tier and below can now have fog covered zones
  * Fog zones and starting zone will get fog repellers on fog levels
* All glowsticks can be found now: green, yellow, Halloween (orange), and Christmas (red)

### Changes

* Increased chance for bosses to roll in D-tier
  * Added 2x mother and 2x tank rolls


## [v0.13.0](https://github.com/brgmnn/autogen-rundown/releases/tag/v0.13.0) — November 12, 2023

### New

* Retrieve big item objective! Things like Cryo Crate, Neonate, Datasphere etc. #10 
* Reworked hibernating enemy spawns
  * Better mix of base enemies, charger only, shadow only, and mix of all groups.

### Changes

* Custom build directors having their objectives overridden by build layout
* Fixed shadow's incorrect point value
* Reduced chance of clear path exit geo being blocked from spawning
* Removed "Special Terminal Command" from Main objective pool.
* Enemy balance:
  * D-Tier base zone points: 25-30pts to 30-35pts
  * E-Tier base zone points: 30-35pts to 35-40pts
  * Slightly reduced chance of giant shooter spawns


## [v0.12.0](https://github.com/brgmnn/autogen-rundown/releases/tag/v0.12.0) — November 10, 2023

### New

* Added v1 boss spawns (hibernation)

### Changes

* Fixed duplicate objectives in a single level, subsequent layouts cannot roll the same objective as previously generated layouts.


## [v0.11.0](https://github.com/brgmnn/autogen-rundown/releases/tag/v0.11.0) — November 08, 2023

### New

* Added error alarm locked room for turning off the error alarm

### Changes

* Fixed Mixed class alarms only spawning a few hybrids.


## [v0.10.0](https://github.com/brgmnn/autogen-rundown/releases/tag/v0.10.0) — November 07, 2023

### New

* Distribute Power Cells objective added!
  * 1 to 5 cells can be rolled (minimum 1 on A/B, only E can roll 5 cells to be placed).
* Added custom geomorphs, currently used mostly for distribute power cells objective.
  * Hub Custom Geomorphs
  * "I" (corridor) Custom Geomorphs
* Reworked level generation to primarily use internal planner. In turn this improves the bulkhead placement.

### Changes

* Zone Placement Weights not being weighted enough
* Adjusted starting zone size to avoid boxed in zones
* Fixed some chained puzzles not being loaded to the data blocks
* Fixed chained puzzle packs sometimes running out for longer levels
* Generator Cluster objective added but **disabled**. Marker seed re-rolling isn't possible with generating levels via this mod, and only about 50% of rolls result in a generator cluster spawning.


## [v0.9.0](https://github.com/brgmnn/autogen-rundown/releases/tag/v0.9.0) — November 03, 2023

### Changes

* Zone indexes for secondary / overload incorrectly not starting from 0 with the new planner
* Fixed broken objectives from keycard branching zones
* Fixed uplink zone spawns


## [v0.8.0](https://github.com/brgmnn/autogen-rundown/releases/tag/v0.8.0) — October 30, 2023

### New

* Enable D-Tier!
* Enable E-Tier!
* Added locked zones with key card entry
* Added Shadows roll to enemy generation
* Added Charger/Shadow giant chance rolls to respective specialty zone rolls

### Changes

* HSU Sample Collect: Fixed an issue with zones being too small for the number of HSU's
* HSU Sample Collect: Always place HSU's in final zone
* Special Terminal Command: Always place Terminal in final zone
* Reduced chance for blood doors to spawn and lowered cap on B/C/D levels.

**Full Changelog**: https://github.com/brgmnn/autogen-rundown/compare/v0.7.0...v0.8.0


## [v0.7.0](https://github.com/brgmnn/autogen-rundown/releases/tag/v0.7.0) — October 24, 2023

### New

* Add Terminal Uplink objective
* Add fully custom enemy spawning
  * Custom hibernation groups
  * Individual enemy spawns
  * Custom alarm waves
  * All vanilla waves and spawns preserved
* Add fog EventBuilder events
* Add Charger hibernation rolls
* Add level modifier system with charger modifiers

### Changes

* Increased S-Class I scan distance from door from 6.0 to 12.0

**Full Changelog**: https://github.com/brgmnn/autogen-rundown/compare/v0.6.0...v0.7.0


## [v0.6.0](https://github.com/brgmnn/autogen-rundown/releases/tag/v0.6.0) — October 17, 2023

### Changes

* Reworked enemy points spawning system, scouts now included in points count for rolls
  * Scouts have 50% reduced chance of spawning in zones with a blood door entrance
* Slightly reduced max number of zones for all layouts
* Removed surge alarm from reactor shutdown roll pool
* Fixed scout spawns to manually spawn exact numbers of scouts
* Reduced max number of scouts that can spawn

**Full Changelog**: https://github.com/brgmnn/autogen-rundown/compare/v0.5.0...v0.6.0


## [v0.5.0](https://github.com/brgmnn/autogen-rundown/releases/tag/v0.5.0) — October 15, 2023

### Changes

* Reworked blood doors for more varied enemy spawns
* Changed bulkhead keys to spawn one in each bulkhead area
* Fixed issue with incorrect scan for first alarm scans

**Full Changelog**: https://github.com/brgmnn/autogen-rundown/compare/v0.4.0...v0.5.0


## [v0.4.0](https://github.com/brgmnn/autogen-rundown/releases/tag/v0.4.0) — October 15, 2023

### New

* Added C-Tier
* Secondary / Overload missions
  * Bulkhead DCs with Keys to access
* New objective: Reactor shutdown
  * Reactor specific lighting
  * Reactor shutdown specific scans
* New objective: Input Special Terminal Command
* Added random light selection
* Added watermark for mod version and seed
* Added scaling exit scan times

### Changes

* Adjusted enemy spawning points
* Adjusted room sizes down
* Removed Memory Sticks from small item collection
* Reworked level layout planner to reduce chance of failed map generation
* Manual install: BepInEx terminal is disabled by default now

**Full Changelog**: https://github.com/brgmnn/autogen-rundown/compare/v0.3.0...v0.4.0


## [v0.3.0](https://github.com/brgmnn/autogen-rundown/releases/tag/v0.3.0) — October 10, 2023

### Changes

Add manual install builds and documentation

**Full Changelog**: https://github.com/brgmnn/autogen-rundown/compare/v0.2.1...v0.3.0


## [v0.2.1](https://github.com/brgmnn/autogen-rundown/releases/tag/v0.2.1) — October 09, 2023

### Changes

* Update README.

**Full Changelog**: https://github.com/brgmnn/autogen-rundown/compare/v0.2.0...v0.2.1


## [v0.2.0](https://github.com/brgmnn/autogen-rundown/releases/tag/v0.2.0) — October 09, 2023

### New

* HSU Sample collection
* Scouts
* Blood door and error alarm rolls

### Changes

* Balancing of enemies
* Fixed exit zone for HSU

**Full Changelog**: https://github.com/brgmnn/autogen-rundown/compare/v0.1.0...v0.2.0


## [v0.1.0](https://github.com/brgmnn/autogen-rundown/releases/tag/v0.1.0) — October 06, 2023

Initial release 🎉

### New 

* A Tier levels
* B Tier levels

