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
    /// <summary>
    /// A mad dash to the exit
    /// </summary>
    /// <param name="start"></param>
    /// <param name="delay"></param>
    public void AddKdsDeep_R8E1Exit(
        ZoneNode start,
        double delay = 0.0)
    {
        // ------ Snatcher scan corridor ------
        var (corridor1, corridor1Zone) = AddZone_Forward(start);
        corridor1Zone.CustomGeomorph = "Assets/AssetPrefabs/Complex/Mining/Geomorphs/Refinery/geo_64x64_mining_refinery_I_HA_03.prefab";
        corridor1Zone.AliasPrefix = "KDS Deep, ZONE";
        corridor1Zone.Altitude = Altitude.OnlyHigh;
        corridor1Zone.LightSettings = Lights.Light.Monochrome_Red;

        // ------ Penultimate corridor ------
        var (corridor2, corridor2Zone) = AddZone_Forward(corridor1);
        corridor2Zone.CustomGeomorph = "Assets/AssetPrefabs/Complex/Mining/Geomorphs/Refinery/geo_64x64_mining_refinery_I_HA_06.prefab";
        corridor2Zone.SecurityGateToEnter = SecurityGate.Apex;
        corridor2Zone.AliasPrefix = "KDS Deep, ZONE";
        corridor2Zone.Altitude = Altitude.OnlyHigh;
        corridor2Zone.LightSettings = Lights.Light.RedToYellow_1;

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
            .AddSetZoneLights(corridor2.ZoneNumber, 0, new SetZoneLight
            {
                LightSettings = Light.LightSettings.AuxiliaryPower,
                Duration = 0.1,
                Seed = 1,
            }, 0.1, auxLightsDelay + 0.5)
            .AddSound(Sound.LightsOn_Vol3, auxLightsDelay);

        // ------ KDS Deep HSU Exit tile ------
        var (exit, exitZone) = AddZone_Forward(corridor2);

        planner.UpdateNode(exit with { Tags = exit.Tags.Extend("no_enemies") });

        exitZone.LightSettings = (Lights.Light)Light.LightSettings.AuxiliaryPower.PersistentId;
        exitZone.CustomGeomorph = "Assets/AssetPrefabs/Complex/Mining/Geomorphs/geo_64x64_mining_HSU_exit_R8E1.prefab";
        exitZone.AliasPrefix = "KDS Deep, ZONE";
        exitZone.Altitude = Altitude.OnlyHigh;
        exitZone.LightSettings = Lights.Light.Reactor_blue_to_red_all_on_1;

        exitZone.EventsOnOpenDoor
            .AddActivateChainedPuzzle("CustomSpawnExit", 1.0)
            .AddSetNavMarker("WE_R8E1_Center", 0.5);

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

        AddKdsDeep_R8E1Exit(terminal, 0.0);
    }
}
