using AutogenRundown.DataBlocks.Alarms;
using AutogenRundown.DataBlocks.Custom.AdvancedWardenObjective;
using AutogenRundown.DataBlocks.Enums;
using AutogenRundown.DataBlocks.Objectives;
using AutogenRundown.DataBlocks.WorldEvents;
using AutogenRundown.DataBlocks.ZoneData;
using AutogenRundown.DataBlocks.Zones;
using AutogenRundown.Extensions;

namespace AutogenRundown.DataBlocks;

// Ideas:
//
//      EMERGENCY_ESCAPE_PROTOCOL -- From R7C1 Monster room

public partial record LevelLayout
{
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

        level.FogSettings = Fog.DefaultFog;
        // level.CustomSuccessScreen = SuccessScreen.ResourcesExpended;

        // TODO: adjust this. Error should have less
        // Normal generation for this
        var nodes = AddBranch(start, 2, "special_terminal");
        var terminal = nodes.Last();

        // // Adds the penultimate (or just only) zone as a forward extract candidate
        // AddForwardExtractStart(nodes.TakeLast(2).First());
        // AddForwardExtractStart(terminal, chance: 0.4);

        // // 55% chance to attempt to lock the end zone with a key puzzle
        // if (Generator.Flip(0.55))
        //     AddKeyedPuzzle(terminal, "special_terminal", director.Bulkhead == Bulkhead.Main ? 2 : 1);

        planner.UpdateNode(terminal with { Tags = terminal.Tags.Extend("bulkhead_candidate") });



        var (exitCorridor, exitCorridorZone) = AddZone_Forward(terminal);
        exitCorridorZone.CustomGeomorph = "Assets/AssetPrefabs/Complex/Mining/Geomorphs/Refinery/geo_64x64_mining_refinery_I_HA_06.prefab";

        // Events to simulate the reactor blowing
        exitCorridorZone.EventsOnOpenDoor
            .AddSound(Sound.MachineryBlow, 10.0)
            .AddScreenShake(3.0, 27.0)
            .AddSetZoneLights(exitCorridor.ZoneNumber, 0, new SetZoneLight
                {
                    LightSettings = Light.LightSettings.LightsOff,
                    Duration = 0.1,
                    Seed = 1,
                }, 0.1, 27.5)
            .AddSound(Sound.Environment_PowerdownFailure, delay: 27.3)
            .AddSetZoneLights(exitCorridor.ZoneNumber, 0, new SetZoneLight
            {
                LightSettings = Light.LightSettings.AuxiliaryPower,
                Duration = 0.1,
                Seed = 1,
            }, 0.1, 31.5)
            .AddSound(Sound.LightsOn_Vol3, 31.0);

        var (exit, exitZone) = AddZone_Forward(exitCorridor);

        planner.UpdateNode(exit with { Tags = exit.Tags.Extend("no_enemies") });

        exitZone.LightSettings = (Lights.Light)Light.LightSettings.AuxiliaryPower.PersistentId;
        exitZone.CustomGeomorph = "Assets/AssetPrefabs/Complex/Mining/Geomorphs/geo_64x64_mining_HSU_exit_R8E1.prefab";

        exitZone.EventsOnOpenDoor
            .AddActivateChainedPuzzle("CustomSpawnExit", 1.0)
            .AddSetNavMarker("WE_R8E1_Center", 0.5);

        var scanDoneEvents = new List<WardenObjectiveEvent>();

        scanDoneEvents
            .AddWinOnDeath(2.0)
            .AddMessage("WARDEN SECURITY SYSTEMS DISABLED", 2.5);

        exitZone.WorldEventChainedPuzzleData.Add(new WorldEventChainedPuzzle
        {
            Puzzle = ChainedPuzzle.TeamScan,
            WorldEventObjectFilter = "CustomSpawnExit",
            EventsOnScanDone = scanDoneEvents
        });
    }
}
