using AutogenRundown.DataBlocks.Alarms;
using AutogenRundown.DataBlocks.Custom.AdvancedWardenObjective;
using AutogenRundown.DataBlocks.Enums;
using AutogenRundown.DataBlocks.Objectives;
using AutogenRundown.DataBlocks.WorldEvents;
using AutogenRundown.DataBlocks.ZoneData;
using AutogenRundown.DataBlocks.Zones;
using AutogenRundown.Extensions;
using AutogenRundown.Utils;

namespace AutogenRundown.DataBlocks;

// Ideas:

public partial record LevelLayout
{
    /// <summary>
    /// A mad dash to the exit
    /// </summary>
    /// <param name="start"></param>
    /// <param name="delay"></param>
    public void AddKdsDeep_R8E1Exit(
        ZoneNode start,
        double delay = 0.0)
    {
        // Level settings
        level.FogSettings = Fog.DefaultFog;
        level.CustomSuccessScreen = SuccessScreen.ResourcesExpended;


        // ------ Snatcher scan corridor ------
        var (corridor1, corridor1Zone) = AddZone_Forward(start, new ZoneNode { MaxConnections = 1 });

        {
            corridor1Zone.CustomGeomorph = "Assets/AssetPrefabs/Complex/Mining/Geomorphs/Refinery/geo_64x64_mining_refinery_I_HA_03.prefab";
            corridor1Zone.Coverage = CoverageMinMax.Small_10;
            corridor1Zone.AliasPrefix = "KDS Deep, ZONE";
            corridor1Zone.Altitude = Altitude.OnlyHigh;
            // corridor1Zone.LightSettings = Lights.Light.Monochrome_Red;
            corridor1Zone.LightSettings = Lights.Light.RedToYellow_1;
        }


        // ------ Penultimate corridor ------
        var (corridor2, corridor2Zone) = AddZone_Forward(corridor1, new ZoneNode { MaxConnections = 1 });

        {
            corridor2Zone.CustomGeomorph =
                "Assets/AssetPrefabs/Complex/Mining/Geomorphs/Refinery/geo_64x64_mining_refinery_I_HA_06.prefab";
            corridor2Zone.Coverage = CoverageMinMax.Tiny_3;
            corridor2Zone.SecurityGateToEnter = SecurityGate.Apex;
            corridor2Zone.AliasPrefix = "KDS Deep, ZONE";
            corridor2Zone.Altitude = Altitude.OnlyHigh;
            corridor2Zone.LightSettings = Lights.Light.RedToYellow_1;

            var puzzle = Generator.Select(level.Tier switch
            {
                "D" => new List<(double, ChainedPuzzle)>
                {
                    (1.0, ChainedPuzzle.AlarmClass4_Surge)
                },
                "E" => new List<(double, ChainedPuzzle)>
                {
                    (1.0, ChainedPuzzle.AlarmClass4_Surge)
                },
                _ => new List<(double, ChainedPuzzle)>
                {
                    (1.0, ChainedPuzzle.AlarmClass4_Mixed)
                }
            });

            corridor2Zone.UseStaticBioscanPointsInZone = true;
            corridor2Zone.Alarm = ChainedPuzzle.FindOrPersist(puzzle);

            var explosionDelay = delay + 17;
            var auxLightsDelay = explosionDelay + 4;

            // Events to simulate the reactor blowing
            corridor2Zone.EventsOnDoorScanDone
                .AddSound(Sound.MachineryBlow, delay)
                .AddScreenShake(3.0, explosionDelay)
                .AddSetZoneLights(corridor1.ZoneNumber, 0, new SetZoneLight
                {
                    LightSettings = Light.LightSettings.LightsOff,
                    Duration = 0.1,
                    Seed = 1,
                }, 0.1, explosionDelay + 0.5)
                .AddSetZoneLights(corridor2.ZoneNumber, 0, new SetZoneLight
                {
                    LightSettings = Light.LightSettings.LightsOff,
                    Duration = 0.1,
                    Seed = 1,
                }, 0.1, explosionDelay + 0.7)
                .AddSound(Sound.Environment_PowerdownFailure, delay: explosionDelay + 1.0)
                .AddSetZoneLights(corridor1.ZoneNumber, 0, new SetZoneLight
                {
                    LightSettings = Light.LightSettings.AuxiliaryPower,
                    Duration = 0.1,
                    Seed = 1,
                }, 0.1, auxLightsDelay + 0.5)
                .AddSetZoneLights(corridor2.ZoneNumber, 0, new SetZoneLight
                {
                    LightSettings = Light.LightSettings.AuxiliaryPower,
                    Duration = 0.1,
                    Seed = 1,
                }, 0.1, auxLightsDelay + 0.5)
                .AddSound(Sound.LightsOn_Vol3, auxLightsDelay);
        }

        // ------ KDS Deep HSU Exit tile ------
        var (exit, exitZone) = AddZone_Forward(
            corridor2,
            new ZoneNode
            {
                MaxConnections = 0,
                Tags = new Tags("no_enemies", "no_blood_door")
            });

        {
            exitZone.LightSettings = (Lights.Light)Light.LightSettings.AuxiliaryPower.PersistentId;
            exitZone.CustomGeomorph = "Assets/AssetPrefabs/Complex/Mining/Geomorphs/geo_64x64_mining_HSU_exit_R8E1.prefab";
            exitZone.AliasPrefix = "KDS Deep, ZONE";
            exitZone.Altitude = Altitude.OnlyHigh;
            exitZone.LightSettings = Lights.Light.Reactor_blue_to_red_all_on_1;

            exitZone.Alarm = ChainedPuzzle.TeamScan;

            exitZone.EventsOnOpenDoor
                .AddActivateChainedPuzzle("CustomSpawnExit", 1.0)
                .AddSetNavMarker("WE_R8E1_Center", 0.5);

            // Plays the dramatic tension when they see the destruction
            exitZone.EventsOnTrigger.Add(
                new WardenObjectiveEvent
                {
                    Type = WardenObjectiveEventType.PlaySound,
                    Trigger = WardenObjectiveEventTrigger.None,
                    TriggerFilter = "Evt_TriggerVoice_R8E1",
                    SoundId = Sound.DramaticTension
                });

            var scanDoneEvents = new List<WardenObjectiveEvent>();
            var surviveDuration = level.Tier switch
            {
                "A" => 30.0,
                "B" => 40.0,
                "C" => 50.0,
                "D" => 70.0,
                "E" => 90.0
            };

            scanDoneEvents
                .AddMessage("SURVIVE", 6.0)
                .AddWinOnDeath(surviveDuration)
                .AddMessage("WARDEN SECURITY SYSTEMS DISABLED", surviveDuration + 2.5);

            exitZone.WorldEventChainedPuzzleData.Add(new WorldEventChainedPuzzle
            {
                Puzzle = ChainedPuzzle.TeamScan,
                WorldEventObjectFilter = "CustomSpawnExit",
                EventsOnScanDone = scanDoneEvents
            });
        }
    }

    /// <summary>
    /// Only allow this level to be built on C-tier and below on Mining complex
    /// </summary>
    /// <param name="director"></param>
    /// <param name="objective"></param>
    /// <param name="startish"></param>
    /// <exception cref="Exception"></exception>
    public void BuildLayout_ReachKdsDeep(
        BuildDirector director,
        WardenObjective objective,
        ZoneNode? startish)
    {
        // There's a problem if we have no start zone
        if (startish == null)
        {
            Plugin.Logger.LogError($"No node returned when calling Planner.GetLastZone({director.Bulkhead})");
            throw new Exception("No zone node returned");
        }

        var start = (ZoneNode)startish;
        var elevator = planner.GetZone(start)!;

        elevator.Coverage = CoverageMinMax.Small_10;

        Plugin.Logger.LogDebug($"What zone is start? {start}");

        // planner.UpdateNode(terminal with { Tags = terminal.Tags.Extend("bulkhead_candidate") });



        switch (level.Tier, director.Bulkhead)
        {
            #region Tier: C

            case ("C", _):
            {
                Generator.SelectRun(new List<(double, Action)>
                {
                    (1.0, () => { }),
                });
                break;
            }

            #endregion

            #region Tier: D

            case ("D", _):
            {
                Generator.SelectRun(new List<(double, Action)>
                {
                    (1.0, () => { }),
                });
                break;
            }

            #endregion

            #region Tier: E

            case ("E", _):
            {
                Generator.SelectRun(new List<(double, Action)>
                {
                    (1.0, () => { }),
                });
                break;
            }

            #endregion

            default:
            {
                break;
            }
        }

        AddKdsDeep_R8E1Exit(start, Generator.Between(0, 4));
        }
}
