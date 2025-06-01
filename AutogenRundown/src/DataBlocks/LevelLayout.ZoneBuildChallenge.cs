using AutogenRundown.DataBlocks.Alarms;
using AutogenRundown.DataBlocks.Zones;

namespace AutogenRundown.DataBlocks;

/*
 * --- General layout building blocks ---
 */
public partial record LevelLayout
{
    /// <summary>
    /// Sets start to a hub zone with the next zone locked behind a powered down door that
    /// requires a generator to be powered. Cell is in a side zone from the hub. Layout:
    ///
    ///     start(hub) -> end
    ///                -> power_cell[0,...]
    ///
    /// Note that the start zone will be set to a hub wwith 4 connections
    /// </summary>
    /// <param name="start"></param>
    /// <param name="sideCellZones"></param>
    /// <returns></returns>
    public (ZoneNode, Zone) BuildChallenge_GeneratorCellInSide(
        ZoneNode start,
        int sideCellZones = 1)
    {
        start = planner.UpdateNode(start with { MaxConnections = 3 });
        var startZone = planner.GetZone(start)!;
        startZone.GenHubGeomorph(level.Complex);

        var (end, endZone) = AddZone(start, new ZoneNode());
        var keycardNodes = AddBranch(start, sideCellZones, "power_cell");

        AddGeneratorPuzzle(end, keycardNodes.Last());

        return (end, endZone);
    }

    public (ZoneNode, Zone) BuildChallenge_GeneratorCellInZone(ZoneNode start)
    {
        var (end, endZone) = AddZone(start, new ZoneNode());

        AddGeneratorPuzzle(end, start);

        return (end, endZone);
    }

    public (ZoneNode, Zone) BuildChallenge_KeycardInZone(ZoneNode start)
    {
        var (end, endZone) = AddZone(start, new ZoneNode());

        AddKeycardPuzzle(end, start);

        return (end, endZone);
    }

    /// <summary>
    /// Adds a hub zone with a locked keycard going to the next zone. Keycard is in a side zone(s)
    ///
    /// Layout:
    ///   start(hub) -> end
    ///              -> keycard[0,...]
    ///
    /// Note that the start zone will be set to a hub with 4 connections
    /// </summary>
    /// <param name="start"></param>
    /// <param name="sideKeycardZones"></param>
    /// <returns></returns>
    public (ZoneNode, Zone) BuildChallenge_KeycardInSide(
        ZoneNode start,
        int sideKeycardZones = 1)
    {
        start = planner.UpdateNode(start with { MaxConnections = 3 });
        var startZone = planner.GetZone(start)!;
        startZone.GenHubGeomorph(level.Complex);

        var (end, endZone) = AddZone(start, new ZoneNode());
        var keycardNodes = AddBranch(start, sideKeycardZones, "keycard");

        AddKeycardPuzzle(end, keycardNodes.Last());

        return (end, endZone);
    }

    /// <summary>
    /// Mostly just a wrapper around `AddApexAlarm()` but with the conveniences of returning the
    /// node/zone from the alarm
    /// </summary>
    /// <param name="start"></param>
    /// <param name="population"></param>
    /// <param name="settings"></param>
    /// <returns></returns>
    public (ZoneNode, Zone) BuildChallenge_ApexAlarm(
        ZoneNode start,
        WavePopulation population,
        WaveSettings settings)
    {
        start = planner.UpdateNode(start with { MaxConnections = 3 });
        var startZone = planner.GetZone(start)!;
        startZone.GenHubGeomorph(level.Complex);

        var (end, endZone) = AddZone(start, new ZoneNode());

        startZone.AmmoPacks += 3.0;
        startZone.ToolPacks += 2.0;

        // TODO: consider adding a disinfect station somewhere for this
        if (startZone.InFog)
            startZone.ConsumableDistributionInZone = ConsumableDistribution.Alarms_FogRepellers.PersistentId;

        AddApexAlarm(end, population, settings);

        return (end, endZone);
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="start"></param>
    /// <returns></returns>
    public (ZoneNode, Zone) BuildChallenge_BossFight(ZoneNode start)
    {
        AddAlignedBossFight(start);

        var (end, endZone) = AddZone(start, new ZoneNode { Branch = "primary" });

        return (end, endZone);
    }

    /// <summary>
    /// Constructs a challenge layout that consists of:
    /// 1. Error alarm door
    ///     a. Any number of intermediate zones
    /// 2. Keycard puzzle
    ///     a. Any number of intermediate side zones
    ///
    /// Error alarm and keycard. Zone layout is as follows:
    ///   start -> firstErrorZone -> error zones [1,...] -> penultimate        -> end
    ///                                                      -> keycard[0,...]     -> error_off
    /// </summary>
    /// <param name="start"></param>
    /// <param name="errorZones">How many error zones to build. Must be at least 1</param>
    /// <param name="sideKeycardZones">
    ///     How many side keycard zones to build. If set to 0 the keycard will be placed in the locked keycard zone
    /// </param>
    /// <param name="terminalTurnoffZones">How many zones to get to the turn off terminal</param>
    /// <returns>The end ZoneNode of the challenge</returns>
    public (ZoneNode, Zone) BuildChallenge_ErrorWithOff_KeycardInSide(
        ZoneNode start,
        int errorZones,
        int sideKeycardZones,
        int terminalTurnoffZones)
    {
        // Enforce a minimum of 1 zone
        errorZones = Math.Max(1, errorZones);

        // first error alarm zone
        var (firstError, firstErrorZone) = AddZone(
            start,
            new ZoneNode { Branch = "primary", MaxConnections = 3 });
        firstErrorZone.Coverage = CoverageMinMax.Large;
        firstErrorZone.AmmoPacks += 5.0;
        firstErrorZone.ToolPacks += 3.0;
        firstErrorZone.HealthPacks += 4.0;

        // Add subsequent error alarm zones to go through
        var penultimate = AddBranch_Forward(
            firstError,
            errorZones - 1,
            "primary",
            (_, zone) =>
            {
                zone.AmmoPacks += 5.0;
                zone.ToolPacks += 3.0;
                zone.HealthPacks += 4.0;
            }).Last();
        planner.UpdateNode(penultimate with { MaxConnections = 2 });

        // Add the ending zone that will be returned
        var (end, endZone) = AddZone(penultimate, new ZoneNode { Branch = "primary" });

        // Build the side chain for the keycard if we have keycard zones
        // If this is zero it will just default to returning the penultimate zone
        var keycard = AddBranch(
            penultimate,
            sideKeycardZones,
            "keycard",
            (_, zone) =>
            {
                zone.AmmoPacks += 5.0;
                zone.ToolPacks += 3.0;
                zone.HealthPacks += 4.0;
            }).Last();

        // Lock the end zone
        AddKeycardPuzzle(end, keycard);

        // Build terminal zones
        // If `terminalTurnoffZones` is set to zero, then the turnoff terminal will be placed in the end zone.
        ZoneNode? terminal = AddBranch(
            end,
            terminalTurnoffZones,
            "error_turnoff",
            (_, zone) =>
            {
                zone.AmmoPacks += 5.0;
                zone.ToolPacks += 3.0;
                zone.HealthPacks += 4.0;
                zone.DisinfectPacks += 4.0;
            }).Last();

        var population = WavePopulation.Baseline;

        // First set shadows if we have them
        if (level.Settings.HasShadows())
            population = Generator.Flip(0.6) ? WavePopulation.OnlyShadows : WavePopulation
                .Baseline_Shadows;

        // Next check and set chargers first, then flyers
        if (level.Settings.HasChargers())
            population = WavePopulation.Baseline_Chargers;
        else if (level.Settings.HasFlyers())
            population = WavePopulation.Baseline_Flyers;

        // Error wave settings
        var settings = level.Tier switch
        {
            "B" => WaveSettings.Error_Easy,
            "C" => WaveSettings.Error_Normal,
            "D" => WaveSettings.Error_Hard,
            "E" => WaveSettings.Error_VeryHard,

            _ => WaveSettings.Error_Easy,
        };

        // Lock the first zone with the error alarm
        AddErrorAlarm(firstError, terminal, settings, population);

        return (end, endZone);
    }

    /// <summary>
    /// Generates a challenge zone build with:
    ///
    ///     start -> firstErrorZone (with cell) -> error zones [1,...] -> penultimate (with generator) -> end
    ///
    /// Note that this requires a team member carrying the cell through the error zones whilst
    /// the error alarm is ongoing which will be _hard_. Not recommended to use this challenge
    /// for anything above a C-tier.
    /// </summary>
    /// <param name="start"></param>
    /// <param name="errorZones"></param>
    /// <param name="sideCellZones"></param>
    /// <param name="terminalTurnoffZones"></param>
    /// <returns></returns>
    public (ZoneNode, Zone) BuildChallenge_ErrorWithOff_GeneratorCellCarry(
        ZoneNode start,
        int errorZones,
        int terminalTurnoffZones)
    {
        // Enforce a minimum of 1 zone
        errorZones = Math.Max(1, errorZones);

        // first error alarm zone
        var (firstError, firstErrorZone) = AddZone(
            start,
            new ZoneNode { Branch = "primary", MaxConnections = 3 });
        firstErrorZone.Coverage = CoverageMinMax.Large;
        firstErrorZone.AmmoPacks += 5.0;
        firstErrorZone.ToolPacks += 3.0;
        firstErrorZone.HealthPacks += 4.0;

        // Add subsequent error alarm zones to go through
        var penultimate = AddBranch_Forward(
            firstError,
            errorZones - 1,
            "primary",
            (_, zone) =>
            {
                zone.AmmoPacks += 5.0;
                zone.ToolPacks += 3.0;
                zone.HealthPacks += 4.0;
            }).Last();
        planner.UpdateNode(penultimate with { MaxConnections = 2 });

        // Add the ending zone that will be returned
        var (end, endZone) = AddZone(penultimate, new ZoneNode { Branch = "primary" });

        // Lock the end zone behind a generator puzzle
        // The cell has to be carried from the first error zone
        AddGeneratorPuzzle(end, firstError);

        // Build terminal zones
        // If `terminalTurnoffZones` is set to zero, then the turnoff terminal will be placed in the end zone.
        ZoneNode? terminal = AddBranch(
            end,
            terminalTurnoffZones,
            "error_turnoff",
            (_, zone) =>
            {
                zone.AmmoPacks += 5.0;
                zone.ToolPacks += 3.0;
                zone.HealthPacks += 4.0;
                zone.DisinfectPacks += 4.0;
            }).Last();

        var population = WavePopulation.Baseline;

        // First set shadows if we have them
        if (level.Settings.HasShadows())
            population = Generator.Flip(0.6) ? WavePopulation.OnlyShadows : WavePopulation
                .Baseline_Shadows;

        // Next check and set chargers first, then flyers
        if (level.Settings.HasChargers())
            population = WavePopulation.Baseline_Chargers;
        else if (level.Settings.HasFlyers())
            population = WavePopulation.Baseline_Flyers;

        // Error wave settings
        var settings = level.Tier switch
        {
            "B" => WaveSettings.Error_Easy,
            "C" => WaveSettings.Error_Normal,
            "D" => WaveSettings.Error_Hard,
            "E" => WaveSettings.Error_VeryHard,

            _ => WaveSettings.Error_Easy,
        };

        // Lock the first zone with the error alarm
        AddErrorAlarm(firstError, terminal, settings, population);

        return (end, endZone);
    }
}
