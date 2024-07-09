# Changelog

See more at https://github.com/brgmnn/autogen-rundown


## [v0.25.0](https://github.com/brgmnn/autogen-rundown/releases/tag/v0.25.0) — June 25, 2024

### New

* Added fixed E-tier Clear Path objective level.

### Changed

* Reduced number of zones before reaching reactors in Startup/Shutdown missions. Reactor I-tile/corridor now spawns immediately after the bulkhead door instead of several preamble zones.
* Increased reactor base wave duration back to 60s from 45s. All reactor waves should now have an extra 15s.
* Fixed incorrect bundling of dependency plugins in manual install package.
* Added custom README for Thunderstore listing and CHANGELOG file.

**Full Changelog**: https://github.com/brgmnn/autogen-rundown/compare/v0.24.2...v0.25.0


## [v0.24.2](https://github.com/brgmnn/autogen-rundown/releases/tag/v0.24.2) — June 25, 2024

## Changes

* Fixed fatal missing directory error on first time startup

**Full Changelog**: https://github.com/brgmnn/autogen-rundown/compare/v0.24.1...v0.24.2


## [v0.24.1](https://github.com/brgmnn/autogen-rundown/releases/tag/v0.24.1) — June 25, 2024

## Changes

* Fixed folder structure issue causing crash on startup

**Full Changelog**: https://github.com/brgmnn/autogen-rundown/compare/v0.24.0...v0.24.1


## [v0.24.0](https://github.com/brgmnn/autogen-rundown/releases/tag/v0.24.0) — June 25, 2024

## New

* Added Reactor Geomorph for Service Complex. Reactor startup/shutdown missions previously could not be used in the Service complex (Floodways, Gardens) as there was no geomorph in those complex tilesets. This release adds a dependency on FlowGeos which provides a reactor geo for floodways.

## Changes

* Reactor Startup D-tier mission is now a fixed regular level, not just a test level
* Fixed Service complex to use both Floodways and Gardens
* Fixed several warnings being thrown by duplicate alarm names
* Fixed `Baseline_Hard` wave settings including `Weakling` enemy types. This fixes many waves spawning shadows which should not have.

**Full Changelog**: https://github.com/brgmnn/autogen-rundown/compare/v0.23.2...v0.24.0


## [v0.23.2](https://github.com/brgmnn/autogen-rundown/releases/tag/v0.23.2) — June 25, 2024

## Fixed

* Fixed `Class S 1` alarms duration being too short. Duration is now 100s (was previously 25s)
* Increased size of code fetch terminal zones to try and reduce chance of soft locks with bad terminal placements

**Full Changelog**: https://github.com/brgmnn/autogen-rundown/compare/v0.23.1...v0.23.2


## [v0.23.1](https://github.com/brgmnn/autogen-rundown/releases/tag/v0.23.1) — June 25, 2024

## Fix

* Disable keyed zone rolls as there are still map generation hard locks happening with it

**Full Changelog**: https://github.com/brgmnn/autogen-rundown/compare/v0.23.0...v0.23.1


## [v0.23.0](https://github.com/brgmnn/autogen-rundown/releases/tag/v0.23.0) — June 25, 2024

## Added

* Added Zoomer and Nightmare scouts to rolls
* Added Nightmare wave to reactor startup
* Added Tank in baseline wave to reactor startup
* Added additional Potato Tank rolls to reactor startup waves

## Fixed

* Fixed baseline charger / nightmare reactor waves using baseline hybrid wave populations

## Balance

* Reduced Reactor hybrids only wave from 28pts to 16pts
* Reduced reactor wave fixed base time from 60s to 45s

**Full Changelog**: https://github.com/brgmnn/autogen-rundown/compare/v0.22.0...v0.23.0


## [v0.22.0](https://github.com/brgmnn/autogen-rundown/releases/tag/v0.22.0) — June 25, 2024

## Added

* Added some Nightmare enemies for several alarms

## Fixed

* Fixed `Class V Cluster` alarm only having 4 scans

## Balanced

* Rebalanced D-tier alarms
* Reduced number of zones spawned for each area and weighted distribution of zone count selection

## Other

* Automated release artifacts upload

**Full Changelog**: https://github.com/brgmnn/autogen-rundown/compare/v0.21.0...v0.22.0


## [v0.21.0](https://github.com/brgmnn/autogen-rundown/releases/tag/v0.21.0) — June 25, 2024

## Balance

* Reduce health multiplier in Reactor Startup
* Slightly reduce time factor for enemies/coverage in Reactor Startup fetch codes

**Full Changelog**: https://github.com/brgmnn/autogen-rundown/compare/v0.20.0...v0.21.0


## [v0.21.0-beta](https://github.com/brgmnn/autogen-rundown/releases/tag/v0.21.0-beta) — June 25, 2024

## Other

Test release for verifying automated workflows

**Full Changelog**: https://github.com/brgmnn/autogen-rundown/compare/v0.20.0...v0.21.0-beta


## [v0.20.0](https://github.com/brgmnn/autogen-rundown/releases/tag/v0.20.0) — June 25, 2024

## Balance

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

## Other

* Added more lore text for Reactor Startup and Reactor Shutdown objectives

**Full Changelog**: https://github.com/brgmnn/autogen-rundown/compare/v0.19.0...v0.20.0


## [v0.19.0](https://github.com/brgmnn/autogen-rundown/releases/tag/v0.19.0) — June 25, 2024

## Fixed

* Reworked door key puzzle generation. Keys for locked doors should no longer spawn behind those doors. Fixes #9
* Added additional zone direction hints to help with generation deadlocks

## Other

* Moved test level to D-tier

**Full Changelog**: https://github.com/brgmnn/autogen-rundown/compare/v0.18.2...v0.19.0


## [v0.18.2](https://github.com/brgmnn/autogen-rundown/releases/tag/v0.18.2) — June 25, 2024

## Fixed

* Added direction hinting for bulkhead zone generations

## Other

* Updated required version of `Inas07-LocalProgression`
* Updated vanilla data blocks for `WavePopulation` and `WaveSettings`.

**Full Changelog**: https://github.com/brgmnn/autogen-rundown/compare/v0.18.1...v0.18.2


## [v0.18.1](https://github.com/brgmnn/autogen-rundown/releases/tag/v0.18.1) — June 25, 2024

## Fixed

* Issue with mod being flagged by the automod in Thunderstore due to some NuGet packages which are already bundled by BepInEx.
  * The fix is to use the package from BepInEx instead of from NuGet.


## [v0.18.0](https://github.com/brgmnn/autogen-rundown/releases/tag/v0.18.0) — June 25, 2024

## Fixed

* Rundown 8 game updated! 🥳 


## [v0.17.1](https://github.com/brgmnn/autogen-rundown/releases/tag/v0.17.1) — June 25, 2024

## Balanced

* Reactor Startup
  * Increased reactor enemy wave population points totals for Easy / Medium / hard.
    * Easy: 25pts to 40pts
    * Medium: 30pts to 60pts
    * Hard: 40pts to 80pts
  * Increased wave failed time for fetch code waves from 30s to 1/2 the verify time.
  * Increased wave verify time granted from puzzle component travel distance.


## [v0.17.0](https://github.com/brgmnn/autogen-rundown/releases/tag/v0.17.0) — June 25, 2024

## Added

* Reactor Startup - Code fetching 💻
  * _Reactor startup objectives can now spawn with additional areas that must be access to fetch codes from terminals_
* Set manual install BepInEx logging to disk to `All`

## Fixed

* Issue where some fixed data blocks were being saved last instead of first
* Added workaround to reduce chance of failed level generation on Reactor Startup

## Balanced

* Adjusted "Class S I Alarm" to use 100s sustained scan instead of 140s scan.


## [v0.16.0](https://github.com/brgmnn/autogen-rundown/releases/tag/v0.16.0) — June 25, 2024

## Added

* New objective! Reactor Startup
  * Currently with simple input codes (no code fetching yet)
* Chance for triple bulkhead starting area to spawn in Digsite using `Assets/AssetPrefabs/Complex/Mining/Geomorphs/Digsite/geo_64x64_mining_dig_site_hub_SF_01.prefab`

## Fixed

* Matter Wave Projector objective having the wrong help text
* Zone numbers possibly colliding between Main/Secondary/Overload


## [v0.15.0](https://github.com/brgmnn/autogen-rundown/releases/tag/v0.15.0) — June 25, 2024

## Added

* Big pickup item - Matter Wave Projector: Custom level layout generation
* Fog turbine in starting area zones for fog levels

## Fixed

* Big pickup having a low number of zones for single or two items.


## [v0.14.0](https://github.com/brgmnn/autogen-rundown/releases/tag/v0.14.0) — June 25, 2024

## Added

* Fog level modifiers!
  * B-Tier and below can now have fog covered zones
  * Fog zones and starting zone will get fog repellers on fog levels
* All glowsticks can be found now: green, yellow, Halloween (orange), and Christmas (red)

## Balance

* Increased chance for bosses to roll in D-tier
  * Added 2x mother and 2x tank rolls


## [v0.13.0](https://github.com/brgmnn/autogen-rundown/releases/tag/v0.13.0) — June 25, 2024

## Added

* Retrieve big item objective! Things like Cryo Crate, Neonate, Datasphere etc. #10 
* Reworked hibernating enemy spawns
  * Better mix of base enemies, charger only, shadow only, and mix of all groups.

## Fixed

* Custom build directors having their objectives overridden by build layout
* Fixed shadow's incorrect point value
* Reduced chance of clear path exit geo being blocked from spawning

## Balance Changes

* Removed "Special Terminal Command" from Main objective pool.
* Enemy balance:
  * D-Tier base zone points: 25-30pts to 30-35pts
  * E-Tier base zone points: 30-35pts to 35-40pts
  * Slightly reduced chance of giant shooter spawns


## [v0.12.0](https://github.com/brgmnn/autogen-rundown/releases/tag/v0.12.0) — June 25, 2024

## Added

* Added v1 boss spawns (hibernation)

## Fixed

* Fixed duplicate objectives in a single level, subsequent layouts cannot roll the same objective as previously generated layouts.


## [v0.11.0](https://github.com/brgmnn/autogen-rundown/releases/tag/v0.11.0) — June 25, 2024

## Added

* Added error alarm locked room for turning off the error alarm

## Fixed

* Fixed Mixed class alarms only spawning a few hybrids.


## [v0.10.0](https://github.com/brgmnn/autogen-rundown/releases/tag/v0.10.0) — June 25, 2024

## Added

* Distribute Power Cells objective added!
  * 1 to 5 cells can be rolled (minimum 1 on A/B, only E can roll 5 cells to be placed).
* Added custom geomorphs, currently used mostly for distribute power cells objective.
  * Hub Custom Geomorphs
  * "I" (corridor) Custom Geomorphs
* Reworked level generation to primarily use internal planner. In turn this improves the bulkhead placement.

## Fixed

* Zone Placement Weights not being weighted enough
* Adjusted starting zone size to avoid boxed in zones
* Fixed some chained puzzles not being loaded to the data blocks
* Fixed chained puzzle packs sometimes running out for longer levels

## Not Added

* Generator Cluster objective added but disabled. Marker seed re-rolling isn't possible with generating levels via this mod, and only about 50% of rolls result in a generator cluster spawning.


## [v0.9.0](https://github.com/brgmnn/autogen-rundown/releases/tag/v0.9.0) — June 25, 2024

## Fixed

* Zone indexes for secondary / overload incorrectly not starting from 0 with the new planner
* Fixed broken objectives from keycard branching zones
* Fixed uplink zone spawns


## [v0.8.0](https://github.com/brgmnn/autogen-rundown/releases/tag/v0.8.0) — June 25, 2024

## New

* Enable D-Tier!
* Enable E-Tier!
* Added locked zones with key card entry
* Added Shadows roll to enemy generation
* Added Charger/Shadow giant chance rolls to respective specialty zone rolls

## Fixed

* HSU Sample Collect: Fixed an issue with zones being too small for the number of HSU's
* HSU Sample Collect: Always place HSU's in final zone
* Special Terminal Command: Always place Terminal in final zone

## Balance Changes

* Reduced chance for blood doors to spawn and lowered cap on B/C/D levels.

**Full Changelog**: https://github.com/brgmnn/autogen-rundown/compare/v0.7.0...v0.8.0


## [v0.7.0](https://github.com/brgmnn/autogen-rundown/releases/tag/v0.7.0) — June 25, 2024

## New

* Add Terminal Uplink objective
* Add fully custom enemy spawning
  * Custom hibernation groups
  * Individual enemy spawns
  * Custom alarm waves
  * All vanilla waves and spawns preserved
* Add fog EventBuilder events
* Add Charger hibernation rolls
* Add level modifier system with charger modifiers

## Fixed

* Increased S-Class I scan distance from door from 6.0 to 12.0

**Full Changelog**: https://github.com/brgmnn/autogen-rundown/compare/v0.6.0...v0.7.0


## [v0.6.0](https://github.com/brgmnn/autogen-rundown/releases/tag/v0.6.0) — June 25, 2024

## Features

* Reworked enemy points spawning system, scouts now included in points count for rolls
  * Scouts have 50% reduced chance of spawning in zones with a blood door entrance

## Fixes

* Slightly reduced max number of zones for all layouts
* Removed surge alarm from reactor shutdown roll pool
* Fixed scout spawns to manually spawn exact numbers of scouts
* Reduced max number of scouts that can spawn

**Full Changelog**: https://github.com/brgmnn/autogen-rundown/compare/v0.5.0...v0.6.0


## [v0.5.0](https://github.com/brgmnn/autogen-rundown/releases/tag/v0.5.0) — June 25, 2024

## Improvements

* Reworked blood doors for more varied enemy spawns
* Changed bulkhead keys to spawn one in each bulkhead area

## Fixes

* Fixed issue with incorrect scan for first alarm scans

**Full Changelog**: https://github.com/brgmnn/autogen-rundown/compare/v0.4.0...v0.5.0


## [v0.4.0](https://github.com/brgmnn/autogen-rundown/releases/tag/v0.4.0) — June 25, 2024

## New features

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

## Improvements

* Adjusted enemy spawning points
* Adjusted room sizes down
* Removed Memory Sticks from small item collection
* Reworked level layout planner to reduce chance of failed map generation
* Manual install: BepInEx terminal is disabled by default now

**Full Changelog**: https://github.com/brgmnn/autogen-rundown/compare/v0.3.0...v0.4.0


## [v0.3.0](https://github.com/brgmnn/autogen-rundown/releases/tag/v0.3.0) — June 25, 2024

Add manual install builds and documentation

**Full Changelog**: https://github.com/brgmnn/autogen-rundown/compare/v0.2.1...v0.3.0


## [v0.2.1](https://github.com/brgmnn/autogen-rundown/releases/tag/v0.2.1) — June 25, 2024

Update README.

**Full Changelog**: https://github.com/brgmnn/autogen-rundown/compare/v0.2.0...v0.2.1


## [v0.2.0](https://github.com/brgmnn/autogen-rundown/releases/tag/v0.2.0) — June 25, 2024

Added:

* HSU Sample collection
* Scouts
* Balancing of enemies
* Fixed exit zone for HSU
* Blood door and error alarm rolls

**Full Changelog**: https://github.com/brgmnn/autogen-rundown/compare/v0.1.0...v0.2.0


## [v0.1.0](https://github.com/brgmnn/autogen-rundown/releases/tag/v0.1.0) — June 25, 2024

Initial release, only support for A and B Tier.

