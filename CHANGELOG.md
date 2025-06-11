# Changelog

See more at https://github.com/brgmnn/autogen-rundown


## [v0.69.0](https://github.com/brgmnn/autogen-rundown/releases/tag/v0.69.0) — June 11, 2025

<!-- Release notes generated using configuration in .github/release.yml at main -->

### New

* 🌟 New enemy: Infested Strikers
  * Glowing weaker version of a striker, that on death spawns two babies and a tiny fog cloud. Be careful not to get swarmed!

### Changes

* Balance: Updated `Extreme` and `Overload` Surge Alarms to spawn only chargers / nightmares instead of mixed populations
* Balance: Updated `OnlyNightmares` wave populations to include Nightmare Giants
* Balance: Infested strikers now available on Reactor waves and Door rolled population alarms
* Balance: Reactor waves should have shorter delays between spawns
* Balance: Increased wave pause max enemy cost for many waves. In general waves should feel faster.

**Full Changelog**: https://github.com/brgmnn/autogen-rundown/compare/v0.68.0...v0.69.0


## [v0.68.0](https://github.com/brgmnn/autogen-rundown/releases/tag/v0.68.0) — June 07, 2025

<!-- Release notes generated using configuration in .github/release.yml at main -->

### New

* Infection Hybrids can now spawn hibernating
* Boosters are no longer consumed when used in levels
* Glow stick colors can now be configured to show as player colors

### Changes

* Balance: Increased glow stick fade in duration from `2` to `6` seconds
* Balance: Increased glow stick fade out duration from `3` to `12` seconds
* Balance: Increased glow stick lifetime duration from `60` to `75` seconds
* Fix: hybrids will now roll to spawn as hibernating enemies
* Fix: Errors thrown from attempting to spawn hibernating SquidBoss

**Full Changelog**: https://github.com/brgmnn/autogen-rundown/compare/v0.67.0...v0.68.0


## [v0.67.0](https://github.com/brgmnn/autogen-rundown/releases/tag/v0.67.0) — June 01, 2025

<!-- Release notes generated using configuration in .github/release.yml at main -->

### New

* Added Apex Error alarm doors (boss error alarms)

### Changes

* Balance: Infectious fog levels now roll to spawn disinfection packs in fog zones
* Balance: Error alarm turn-off commands now _only_ deactivate all current error alarms. Door alarms, objective exit alarms etc. are no longer deactivated.
* Balance: Added cap to number of randomly placed error alarms in the level. In general there should not be levels with _too_ many error alarms now
* Balance: Rolled error alarms now scale in difficulty with level tier. D and E tier error alarms will be noticeably harder.
* Balance: Rolled error alarms can now roll different enemy populations based on level settings.
* Balance: Rolled error alarms turnoff chance increased from `50%` to `70%`
* Change: IPv6 addresses are displayed in upper case to avoid confusing when inputting into terminals
* Fix level objective selection to exclude objectives that use a timer if an existing objective already uses a timer
  * Level timers such as survival timers do not work well together it seems
* Fix survival objectives to not have error alarms, also due to survival timers and terminal turnoff messages conflicting
* Fix for some level generation lockups

**Full Changelog**: https://github.com/brgmnn/autogen-rundown/compare/v0.66.2...v0.67.0


## [v0.66.2](https://github.com/brgmnn/autogen-rundown/releases/tag/v0.66.2) — May 22, 2025

<!-- Release notes generated using configuration in .github/release.yml at main -->

### Changes

* Fix scout waves not spawning

**Full Changelog**: https://github.com/brgmnn/autogen-rundown/compare/v0.66.1...v0.66.2


## [v0.66.1](https://github.com/brgmnn/autogen-rundown/releases/tag/v0.66.1) — May 21, 2025

<!-- Release notes generated using configuration in .github/release.yml at main -->

### Changes

* Fixed incorrect release version

**Full Changelog**: https://github.com/brgmnn/autogen-rundown/compare/v0.66.0...v0.66.1


## [v0.66.0](https://github.com/brgmnn/autogen-rundown/releases/tag/v0.66.0) — May 21, 2025

<!-- Release notes generated using configuration in .github/release.yml at main -->

### New

* Added support for mod [VanillaReloaded](https://thunderstore.io/c/gtfo/p/tru0067/VanillaReloaded/)

**Full Changelog**: https://github.com/brgmnn/autogen-rundown/compare/v0.65.0...v0.66.0


## [v0.65.0](https://github.com/brgmnn/autogen-rundown/releases/tag/v0.65.0) — May 18, 2025

<!-- Release notes generated using configuration in .github/release.yml at main -->

### New

* Added seven letter codes for E-tier
* Added IPv6 uplink addresses for D-tier and E-tier

### Changes

* Adjusted `CorruptedTerminalUplink` code per line in log files

**Full Changelog**: https://github.com/brgmnn/autogen-rundown/compare/v0.64.0...v0.65.0


## [v0.64.0](https://github.com/brgmnn/autogen-rundown/releases/tag/v0.64.0) — May 17, 2025

<!-- Release notes generated using configuration in .github/release.yml at main -->

### New

* Added longer, and more uplink codes for uplink and corrupted uplink objectives at higher difficulties
* Added longer codes for harder reactor levels

**Full Changelog**: https://github.com/brgmnn/autogen-rundown/compare/v0.63.0...v0.64.0


## [v0.63.0](https://github.com/brgmnn/autogen-rundown/releases/tag/v0.63.0) — May 16, 2025

<!-- Release notes generated using configuration in .github/release.yml at main -->

### New

* 🌟 Added `CorruptedTerminalUplink` objective!
  * See [R5C3 Main](https://gtfo.fandom.com/wiki/ALT://R5C3) objective for an example of this objective in the base game
* Added 2 new surge alarms (surprise)

### Changes

* Fixed `SpecialTerminalCommand` King of the Hill scans to have curated terminal placements on all tiles so no more terminals spawning in side zones
* Fixed `TerminalUplink` having 1 fewer terminals and terminal rounds than it should have in it's max range for random rolls
* Fixed several extract alarms not being correct
* Balance: Increased distance between many scan puzzle components for Class 5 and above alarm scans
* Balance: Enabled Class 2 Surge alarm on B-tier and Class 3 Surge alarm on C-tier
* Balance: Increase spawns for Mega Mother to match base game
* Balance: Change Mega Mother Tech boss tile to be the R3D1 boss tile
* Balance: Mega Mother room auto alerts shortly after opening door
* Balance: Changed Surge Alarms to only spawn strikers
* Enable stuck flyer fix from EEC

**Full Changelog**: https://github.com/brgmnn/autogen-rundown/compare/v0.62.0...v0.63.0


## [v0.62.0](https://github.com/brgmnn/autogen-rundown/releases/tag/v0.62.0) — May 10, 2025

<!-- Release notes generated using configuration in .github/release.yml at main -->

### New

* Added R8A1 third zone as hub tile for Service complex
* Added R8A1 exit elevator garden tile
* Added harder and a variety of scout waves

### Changes

* Balance: Removed Class 9 alarms from regular security doors (Apex security doors can still have Class 9 alarms)
* Balance: Reduced difficulty of Matter Wave Projector exit scan
* Balance: Reduced chance of Nightmare enemies by 10% on C/D/E tiers.
  * C-tier: 50% to 40%
  * D-tier: 60% to 50%
  * E-tier: 70% to 60%
* Balance: Slightly reduced number of fetch codes for `ReactorStartup` objectives on D/E tier as well as number of zones to search 
* Balance: Increased Giant Beserker health from `180` to `200`, weakspot multiplier also increased from `2.25` to `2.5`
  * Sniper will still 1-shot Giant Beserker
* Balance: Increased Giant Beserker stagger threshold from `45` to `50`
* Fixed some error alarms turnoff zones not having alarms to open them
* Fixed `RetrieveBigItem` intel message prompt when entering the item zone to correctly display the zone number
* Removed `geo_64x64_tech_data_center_HA_05` tile from regular map generation pool due to consistent level generation failures due to it's layout
* Removed garden tiles from general Service complex tile pool. They can still be placed as part of specific level generation, but they should be less common now
  * Garden tiles added on chance basis for the last 3 final zones on `ReactorStartup` code fetching and `ReactorShutdown` password retrieval
* Removed red strobe light seen in a few levels 

**Full Changelog**: https://github.com/brgmnn/autogen-rundown/compare/v0.61.1...v0.62.0


## [v0.61.1](https://github.com/brgmnn/autogen-rundown/releases/tag/v0.61.1) — May 07, 2025

<!-- Release notes generated using configuration in .github/release.yml at main -->

### Changes

* Added support for mod: [ArsenalityRebalance](https://thunderstore.io/c/gtfo/p/leezurli/ArsenalityRebalance/)

**Full Changelog**: https://github.com/brgmnn/autogen-rundown/compare/v0.61.0...v0.61.1


## [v0.61.0](https://github.com/brgmnn/autogen-rundown/releases/tag/v0.61.0) — April 29, 2025

<!-- Release notes generated using configuration in .github/release.yml at main -->

### New

* New enemy: Giant Nightmare Striker
  * Functions very similar to a Giant Charger, but with higher health and slightly increased crit multiplier on chest: 2.25x crit, 180hp

### Changes

* Adjusted number of reactor waves, in general most levels should have fewer reactor waves total now
* Reworked reactor wave difficulty. Waves are generally harder with fewer easy starting waves
  * Waves are broadly generated based on four groups: first wave, first half of waves, second half of waves, last wave
  * Added reactor surge waves (points are continually spawned until wave points total is reached)
* Reactor levels requiring fetching codes now have the following logic:
  * First wave code is always given for free
  * Last wave code always requires fetching from terminal
  * Remaining number of fetching code waves are randomly distributed between the first and last waves
* Removed progression requirements from weekly rundown
* Removed single use resource packs
* Reduced max rolled error alarms in E-tier level layout from 3 to 2.
  * Chance of getting 3 was already very low (~3%) but now it's fixed at 2. A level can still have more than 2 error alarms (bulkheads are rolled independently and manually placed alarms are not counted).
* Reduced max bosses that can spawn per bulkhead level layout:
  * C-tier: max 1 boss per bulkhead
  * D-tier: max 2 bosses per bulkhead
  * E-tier: max 4 boss per bulkhead

**Full Changelog**: https://github.com/brgmnn/autogen-rundown/compare/v0.60.0...v0.61.0


## [v0.60.0](https://github.com/brgmnn/autogen-rundown/releases/tag/v0.60.0) — April 19, 2025

<!-- Release notes generated using configuration in .github/release.yml at main -->

### New

* Added new variations for level starting area and bulkhead placements
* Added infection hybrids to be able to spawn on blood doors on non-infection levels
* Added 200 new predefined level names

### Changes

* Balance: `ReactorShutdown` objective exit alarms reduced for non-main objectives
  * Extreme and Overload reactor shutdown missions now trigger either error alarms, finite point fixed waves, or for A tier extreme: no waves.
* Balance: `ReactorShutdown` removed stealth boss double PMother alarm and triple Mother stealth alarm. Replaced with PMother/Tank and double mother/shadow pouncer
* Balance: `ReactorShutdown` replaced E-tier pouncer bosses with shadow pouncers
* Balance: `ReactorShutdown` removed all completely free shutdown scans (stealth scans are now always boss bait)
* Balance: `TimedTerminalSequence` all zone error alarms moved to only D and E tier levels (chance based).

**Full Changelog**: https://github.com/brgmnn/autogen-rundown/compare/v0.59.0...v0.60.0


## [v0.59.0](https://github.com/brgmnn/autogen-rundown/releases/tag/v0.59.0) — April 16, 2025

<!-- Release notes generated using configuration in .github/release.yml at main -->

### New

* 🌟 Added `HsuActivateSmall` objective! This is bringing the neonate / data sphere etc. to the depressurization room and optionally returning with it. See [R3A1](https://gtfo.fandom.com/wiki/ALT://R3A1) for an example in vanilla.
* 🌟 Added `GatherTerminal` objective! Players must find terminals and execute commands on them to download encryption keys. See [R6D2](https://gtfo.fandom.com/wiki/ALT://R6D2) for an example in vanilla.
* Added new `Service` complex armory dead end title from [donan3967 geo pack 2](https://thunderstore.io/c/gtfo/p/donan3967/donan3967_geo_pack_2/)
* Added chance for disinfection zone to spawn in infected fog maps
* 🌟 New error alarm: boss error alarms.
  * See See [R4E1](https://gtfo.fandom.com/wiki/ALT://R4E1) for a vanilla example
* 🌟 New enemy: Infection Hybrid
  * Added to D/E tier blood doors pool

### Changes

* Balance: increased door alarm difficulty floor. D-tier and E-tier levels have fewer easy scans now.
* Fixed missing weekly E-tier visual menu color
* Fixed several missing enemy spawns. Chargers and nightmares now spawn on C-tier and below
* Balance: Shadow giants added to hibernating enemy pool for shadow enabled D-tier
* Balance: Reduced hibernation points for Nightmares
* Balance: Slightly increased hibernating spawn points for shadows on E-tier
* Balance: Increased and reworked boss spawns
  * Added pouncers and shadow pouncers into boss spawns

**Full Changelog**: https://github.com/brgmnn/autogen-rundown/compare/v0.58.1...v0.59.0


## [v0.58.1](https://github.com/brgmnn/autogen-rundown/releases/tag/v0.58.1) — April 09, 2025

<!-- Release notes generated using configuration in .github/release.yml at main -->

### Changes

* Fixed incorrect JSON key for `Level.DimensionsData`.

**Full Changelog**: https://github.com/brgmnn/autogen-rundown/compare/v0.58.0...v0.58.1


## [v0.58.0](https://github.com/brgmnn/autogen-rundown/releases/tag/v0.58.0) — April 09, 2025

<!-- Release notes generated using configuration in .github/release.yml at main -->

### Changes

* Fix Pouncers/Snatchers crashing the game due to missing dimension arena
* Rerolled and checked monthly seeds

**Full Changelog**: https://github.com/brgmnn/autogen-rundown/compare/v0.57.0...v0.58.0


## [v0.57.0](https://github.com/brgmnn/autogen-rundown/releases/tag/v0.57.0) — April 08, 2025

<!-- Release notes generated using configuration in .github/release.yml at main -->

### Changes

* Balance: D-tier default per zone ammo increased from 4 to 5 ammo packs
* Balance: E-tier default per zone ammo increased from 4 to 6 ammo packs
* Add resources in `ReactorStartup` fetch code zones: +5 ammo packs, +4 health packs, +3 tool packs
* Fixed `ReactorStartup` resources not being distributed to the entrance corridor
* Reworked resource spawning in reactor area
* Fixed first time startup bug with Arsenality
* Fixed `ReactorStartup` fog waves to return level fog to it's normal state at end of the wave instead of clearing it
* `ReactorStartup` fog waves are now infectious if the level fog is infectious
* Change resource packs to allow 1-use packs to spawn

**Full Changelog**: https://github.com/brgmnn/autogen-rundown/compare/v0.56.0...v0.57.0


## [v0.56.0](https://github.com/brgmnn/autogen-rundown/releases/tag/v0.56.0) — April 07, 2025

<!-- Release notes generated using configuration in .github/release.yml at main -->

### New

* Add donan3967 data center reactor tile
* Added support for peer mod [Arsenality](https://thunderstore.io/c/gtfo/p/W33B/Arsenality/)

### Changes

* Disable `geo_64x64_mining_reactor_open_HA_01.prefab` from reactor tile list

**Full Changelog**: https://github.com/brgmnn/autogen-rundown/compare/v0.55.0...v0.56.0


## [v0.55.0](https://github.com/brgmnn/autogen-rundown/releases/tag/v0.55.0) — April 03, 2025

<!-- Release notes generated using configuration in .github/release.yml at main -->

### New

* Added donan3967's [geo_pack_2](https://thunderstore.io/c/gtfo/p/donan3967/donan3967_geo_pack_2/) geomorphs.
  * Enabled the floodways hub and I-tile
* Enabled `TimedTerminalSequence` into the main objectives pool
* Added warden mission description for multiple levels

### Changes

* Changed monthly rundown to use more controlled random distribution for both main objectives and complex selection.
  * Main objectives and complexes are now selected from a weighted pool of options, to ensure a more even selection for rundowns. This should reduce cases where many levels are the same objective and complex.
* Balance: Made skip doors "Security Controls" zone in Survival missions harder, error alarm is increased upon forcing open doors
* Balance: `ReactorShutdown` now has exit trickles for enemies when on main

**Full Changelog**: https://github.com/brgmnn/autogen-rundown/compare/v0.54.0...v0.55.0


## [v0.54.0](https://github.com/brgmnn/autogen-rundown/releases/tag/v0.54.0) — April 01, 2025

<!-- Release notes generated using configuration in .github/release.yml at main -->

### New

* Add boss fight tiles
* Add new enemy
* Disinfect packs can now spawn in some very specific zones!
* Reworked ClearPath objective level layout

### Changes

* Balanced Survival Objective zones to always have a fixed number of extra resources
* Updated Survival Objective to build zones forward
* Fixed a bug where some zone settings would not be applied - https://github.com/brgmnn/autogen-rundown/commit/f165600e26ca9e8f9cd6c4048bb230d7e74a404e
* Fixed a bug where Terminal Uplink objectives would sometimes happen in a single giant zone - https://github.com/brgmnn/autogen-rundown/commit/3025ec09671f43162274a138129a35b386dfd78d

**Full Changelog**: https://github.com/brgmnn/autogen-rundown/compare/v0.53.0...v0.54.0


## [v0.53.0](https://github.com/brgmnn/autogen-rundown/releases/tag/v0.53.0) — March 20, 2025

<!-- Release notes generated using configuration in .github/release.yml at main -->

### Changes

* Balance: Reduce points of most nightmare alarms to account for their difficulty
* Fixed survival skip zones timer being hard coded at 60s

**Full Changelog**: https://github.com/brgmnn/autogen-rundown/compare/v0.52.0...v0.53.0


## [v0.52.0](https://github.com/brgmnn/autogen-rundown/releases/tag/v0.52.0) — March 15, 2025

<!-- Release notes generated using configuration in .github/release.yml at main -->

### New

* Added limited support for Security Sensors in some of the starting zones
  * Includes new dependency on: [EOSExt_SecuritySensor](https://thunderstore.io/c/gtfo/p/Inas07/EOSExt_SecuritySensor)
* Added optional "Skip" for Survival objectives
  *  Side zone with terminal command to force open all security doors to the extract. However survival time is cut short.

### Changes

* Fixed typo in ComplexResourceSet for dakgeos
  *  Causes crashes on level startup, fixes https://github.com/brgmnn/autogen-rundown/issues/16#issuecomment-2691918752
* Removed global backwards direction for layout generation
  * Fixes https://github.com/brgmnn/autogen-rundown/issues/16#issuecomment-2697541679
* Fixed Survival Timer for Secondary/Overload
* Changed "ZONE_123" intel messages to "ZONE 123"

**Full Changelog**: https://github.com/brgmnn/autogen-rundown/compare/v0.51.0...v0.52.0


## [v0.51.0](https://github.com/brgmnn/autogen-rundown/releases/tag/v0.51.0) — February 25, 2025

<!-- Release notes generated using configuration in .github/release.yml at main -->

### Changes

* Weekly / Monthly now require at least 1 A-tier clear to proceed. Previously rundowns with only 1 A level could be skipped.
* Fixed glitched text in one lore message
* Checked and rolled March monthly seeds

**Full Changelog**: https://github.com/brgmnn/autogen-rundown/compare/v0.50.0...v0.51.0


## [v0.50.0](https://github.com/brgmnn/autogen-rundown/releases/tag/v0.50.0) — February 21, 2025

<!-- Release notes generated using configuration in .github/release.yml at main -->

### New

* Added Warden intel messages for `HSUFindSample` missions

### Changes

* Added improvements to bulkhead zone direction placement to help reduce level lockup and broken level generation
* Removed `tech_datacenter_I_RLC_01.prefab` due to big item spawning issues - a65cb4972578377ed2042c9f6e7893cf4f3cd390
* Fix some cases where `ClearPath` objective would not correctly specify the exit zone number on some mod geomorphs
* Balance: Slightly adjusted resources granted in `HSUFindSample` King-of-the-Hill scans

**Full Changelog**: https://github.com/brgmnn/autogen-rundown/compare/v0.49.0...v0.50.0


## [v0.49.0](https://github.com/brgmnn/autogen-rundown/releases/tag/v0.49.0) — February 16, 2025

<!-- Release notes generated using configuration in .github/release.yml at main -->

### New

* Reworks HSU sample retrieval objective
  * Added generator / keycard / error alarm / Apex Alarm variants
 * Added several new Geomorph packs (new map tiles) and enabled most of the tiles
   * [donan3967 geo pack 1](https://github.com/brgmnn/autogen-rundown/commit/af277c2087c9ce9c8f642738557cdd60f801a019)
     * 2 new hub (X-tile) geos, and 1 new corridor (I-tile) geo
   * [DaksGeos](https://github.com/brgmnn/autogen-rundown/commit/217e822399e4039e8a926b12671688f89a1914bd)
     * 1 new exit geo, 1 new hub geo, 1 new corridor geo
   * [CheeseGeos](https://github.com/brgmnn/autogen-rundown/commit/d527de39615c6a0108c54add2787c67e6ed36757)
     * 1 new exit geo, 3 new hub geo, 4 new corridor geos, 1 new dead end geo
* Added AdvancedWardenObjective mod for additional events 
* Added customizable lighting for some events
* Levels are now center aligned in the Rundown menu
* Added new Warden Elevator Drop Intel (instead of all levels having the same message)

### Changes

* Fixed PlaySound events sometimes not triggering
* Fixed crash on first time startup due to attempting to remove EOS directory that doesn't exist
  * Fixed #15 
* Balance: Reduced free doors in A-tier alarm puzzle pack (`2` to `1`), which slightly reduces the chance of doors with no scans
* Balance: Increased base zone estimate time for enemy points from 1.2s/pt to 2.4s/pt
  * This will slightly increase time given to fetch codes and clear areas based on number of enemies
* Balance: Reworked exit alarms for HSU collect sample
* Updated color on rundown screen for daily / monthly to be colored on E-tier

**Full Changelog**: https://github.com/brgmnn/autogen-rundown/compare/v0.48.0...v0.49.0


## [v0.48.0](https://github.com/brgmnn/autogen-rundown/releases/tag/v0.48.0) — February 03, 2025

<!-- Release notes generated using configuration in .github/release.yml at main -->

### New

* Tech / Labs: Added `lab_reactor_HA_02.prefab` geo to reactor tile list.
  * R5D2 high reactor zone 
* Mining / DigSite: Added `elevator_shaft_dig_site_04.prefab` geo to hub tile list.
  * R5D1 extreme first zone

### Changes

* Fixed `ReactorShutdown` not working in Secondary / Overload layouts
* Increased difficulty of scans across all tiers
* Added stealth surprise scans to all tiers (with varying chance of occurring)
* Improve monthly randomness between levels

**Full Changelog**: https://github.com/brgmnn/autogen-rundown/compare/v0.47.0...v0.48.0


## [v0.47.0](https://github.com/brgmnn/autogen-rundown/releases/tag/v0.47.0) — January 31, 2025

<!-- Release notes generated using configuration in .github/release.yml at main -->

### Changes

* Add `RELEASE_CONTAINMENT` variant of King of the Hill / Special Terminal Command objective.
* Check 02/2025 monthly seeds for level lock.

**Full Changelog**: https://github.com/brgmnn/autogen-rundown/compare/v0.46.0...v0.47.0


## [v0.46.0](https://github.com/brgmnn/autogen-rundown/releases/tag/v0.46.0) — January 28, 2025

<!-- Release notes generated using configuration in .github/release.yml at main -->

### New

* Completely reworked Reactor Shutdown
  * Reactor Shutdown objectives can now have locked reactors requiring fetching a password
  * Alarms and waves reworked
* Added King of the Hill special terminal command

### Changes

* Fix Terminal Uplink objectives not having team scan to start the uplink
* Fixed open reactor layout to work for Reactor Shutdown

**Full Changelog**: https://github.com/brgmnn/autogen-rundown/compare/v0.45.0...v0.46.0


## [v0.45.0](https://github.com/brgmnn/autogen-rundown/releases/tag/v0.45.0) — January 22, 2025

<!-- Release notes generated using configuration in .github/release.yml at main -->

### New

* Added error alarm SpecialTerminalCommand objective
  * Also re-balanced chances for each special terminal command type
* Add key puzzle in SpecialTerminalCommand objective layouts to appear 55% of the time
* Add some additional modifiers to alarm rolls

### Changes

* Fixed Survival event timer restarting after adding Extreme extra time
* Adjusted more resources in Survival missions
* Finished HiSec Cargo Crate and Data Sphere objectives to have working exit alarms
* Adjusted down unlock requirements for monthly/weekly rundowns

**Full Changelog**: https://github.com/brgmnn/autogen-rundown/compare/v0.44.0...v0.45.0


## [v0.44.0](https://github.com/brgmnn/autogen-rundown/releases/tag/v0.44.0) — December 22, 2024

<!-- Release notes generated using configuration in .github/release.yml at main -->

### Changes

* Checked 2025/01 monthly rundown for level locks

**Full Changelog**: https://github.com/brgmnn/autogen-rundown/compare/v0.43.0...v0.44.0


## [v0.43.0](https://github.com/brgmnn/autogen-rundown/releases/tag/v0.43.0) — December 19, 2024

<!-- Release notes generated using configuration in .github/release.yml at main -->

### New

* Enabled "Unaugmented" (No booster) clear tracking via LocalProgression (still in testing)

### Changes

* Fix LocalProgression not saving

**Full Changelog**: https://github.com/brgmnn/autogen-rundown/compare/v0.42.0...v0.43.0


## [v0.42.0](https://github.com/brgmnn/autogen-rundown/releases/tag/v0.42.0) — December 18, 2024

<!-- Release notes generated using configuration in .github/release.yml at main -->

### Changes

* Fix monthly rundown build seeds and validate entire rundown doesn't lock up
* Fix intel for retrieve big items always showing zone 20

**Full Changelog**: https://github.com/brgmnn/autogen-rundown/compare/v0.41.0...v0.42.0


## [v0.41.0](https://github.com/brgmnn/autogen-rundown/releases/tag/v0.41.0) — December 17, 2024

<!-- Release notes generated using configuration in .github/release.yml at main -->

### Changes

* Removed vanity items buttons from rundown screen
* Fixed floodways reactor tile using the wrong complex
* Reduced requirements to get to next tier in monthly/weekly in case of locked levels

**Full Changelog**: https://github.com/brgmnn/autogen-rundown/compare/v0.40.0...v0.41.0


## [v0.40.0](https://github.com/brgmnn/autogen-rundown/releases/tag/v0.40.0) — December 16, 2024

### New

* Added Monthly and Weekly rundowns! 🎉 
* Added nightmare enemies as possible sleepers
* Added infectious fog as possible fog roll

### Changes

* Fix Rundowns to use local progression instead of existing rundown ID 🎉
* Reduced number of zones for retrieve big item to balance

**Full Changelog**: https://github.com/brgmnn/autogen-rundown/compare/v0.39.0...v0.40.0


## [v0.39.0](https://github.com/brgmnn/autogen-rundown/releases/tag/v0.39.0) — December 11, 2024

<!-- Release notes generated using configuration in .github/release.yml at main -->

### New

* Added R8B3 center room tower as option for Uplink Terminal objectives with one terminal

**Full Changelog**: https://github.com/brgmnn/autogen-rundown/compare/v0.38.3...v0.39.0


## [v0.38.3](https://github.com/brgmnn/autogen-rundown/releases/tag/v0.38.3) — December 10, 2024

<!-- Release notes generated using configuration in .github/release.yml at main -->

### Changes

* Fixed Open and Unlock door events not working in Secondary / Overload bulkheads

**Full Changelog**: https://github.com/brgmnn/autogen-rundown/compare/v0.38.2...v0.38.3


## [v0.38.2](https://github.com/brgmnn/autogen-rundown/releases/tag/v0.38.2) — December 09, 2024

<!-- Release notes generated using configuration in .github/release.yml at main -->

### Changes

* Fixed Terminal Uplink always spawning terminals in the first zone of the layer.

**Full Changelog**: https://github.com/brgmnn/autogen-rundown/compare/v0.38.1...v0.38.2


## [v0.38.1](https://github.com/brgmnn/autogen-rundown/releases/tag/v0.38.1) — August 04, 2024

### Changes

* Fixed Scout waves to not be infinite

**Full Changelog**: https://github.com/brgmnn/autogen-rundown/compare/v0.38.0...v0.38.1


## [v0.38.0](https://github.com/brgmnn/autogen-rundown/releases/tag/v0.38.0) — August 03, 2024

### New

* Added Zone prefixes for reactor rooms `Reactor, ZONE 123`
* Reworked Timed Sequence to have closed doors until starting the sequence

### Changes

* Fixed additional issues with Alarms not spawning correctly
* Fixed several issues with ReactorShutdown
  * There are still known issues with `ReactorShutdown` missions not working on certain tilesets.

**Full Changelog**: https://github.com/brgmnn/autogen-rundown/compare/v0.37.0...v0.38.0


## [v0.37.0](https://github.com/brgmnn/autogen-rundown/releases/tag/v0.37.0) — July 28, 2024

### New

* Add new `TimedTerminalSequence` objective! [[1]](https://gtfo.fandom.com/wiki/Timed_Sequence)
* Added in remaining FlowGeo's tiles

### Changes

* Add fix for max connections on tiles not being respected.
* Fix issue with stealth scans attempting to randomize enemies.

**Full Changelog**: https://github.com/brgmnn/autogen-rundown/compare/v0.36.0...v0.37.0


## [v0.36.0](https://github.com/brgmnn/autogen-rundown/releases/tag/v0.36.0) — July 21, 2024

### New

* Survival main objectives now add additional time when opening the Secondary bulkhead to clear secondary objectives.
* Survival main objectives with overload objective now has shorter overload (with no additional time to complete it, you just have to go fast!)
  * Overload objectives for a survival main get 2x resources in the first zone

### Changes

* Adjusted lower tiers (A/B/C) enemy points per zone to be higher
* Reduced enemy points by 40% in starting zone for 2x and 3x bulkhead custom spawns
* Reduced number of `VeryHard` alarm wave settings in D-tier and increased `Hard` settings
* Added extra variation in wave populations rolled for various level modifiers (chargers / shadows etc.)
* Fixed flyers wave populations not rolling
* Fixed level modifiers not being applied correctly
* Fixed surge alarms rolling their wave pops and settings
* Fix mod dependency version mismatch

**Full Changelog**: https://github.com/brgmnn/autogen-rundown/compare/v0.35.0...v0.36.0


## [v0.35.0](https://github.com/brgmnn/autogen-rundown/releases/tag/v0.35.0) — July 20, 2024

### New

* Added FlowGeo's Datacenter exit tile
* Added FlowGeo's Datacenter elevator tile

### Changes

* Fixed exit alarm not spawning
  * This should fix a number of other issues including Survival objectives not unlocking the final door
* Adjusted zone coverage values to match more closely those set in geomorph prefabs

**Full Changelog**: https://github.com/brgmnn/autogen-rundown/compare/v0.34.0...v0.35.0


## [v0.34.0](https://github.com/brgmnn/autogen-rundown/releases/tag/v0.34.0) — July 19, 2024

### Changes

* Added more level name variations
* Updated mod dependencies:
  * `MTFO` - 4.6.1 to 4.6.2
* Reworked Alarm generation code, more variation in alarms will now appear

**Full Changelog**: https://github.com/brgmnn/autogen-rundown/compare/v0.33.0...v0.34.0


## [v0.33.0](https://github.com/brgmnn/autogen-rundown/releases/tag/v0.33.0) — July 14, 2024

### Changes

* Add charger/shadow waves for certain alarms in D-tier
* Move survival level to D-tier

**Full Changelog**: https://github.com/brgmnn/autogen-rundown/compare/v0.32.0...v0.33.0


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

**Full Changelog**: https://github.com/brgmnn/autogen-rundown/compare/v0.31.0...v0.32.0


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

