using AutogenRundown.DataBlocks.Alarms;
using AutogenRundown.DataBlocks.Zones;
using AutogenRundown.Utils;

namespace AutogenRundown.DataBlocks;

public partial record LevelLayout
{
    #region Basic layout
    /// <summary>
    /// Adds a new zone onto the source zone node
    /// </summary>
    /// <param name="source"></param>
    /// <param name="node"></param>
    /// <returns></returns>
    public (ZoneNode, Zone) AddZone(ZoneNode source, ZoneNode node)
    {
        var zoneIndex = level.Planner.NextIndex(director.Bulkhead);
        var next = node with { Bulkhead = director.Bulkhead, ZoneNumber = zoneIndex };

        if (next.Tags == null)
            next.Tags = new Tags();

        var nextZone = new Zone
        {
            Coverage = CoverageMinMax.GenNormalSize(),
            LightSettings = Lights.GenRandomLight(),
        };
        nextZone.RollFog(level);

        level.Planner.Connect(source, next);
        var zone = level.Planner.AddZone(next, nextZone);

        return (next, zone);
    }

    /// <summary>
    /// Builds a branch, connecting zones and returning the last zone.
    /// </summary>
    /// <param name="baseNode"></param>
    /// <param name="zoneCount"></param>
    /// <param name="branch"></param>
    /// <returns>The last zone node in the branch</returns>
    [Obsolete("Use and rename AddBranch instead")]
    public ZoneNode BuildBranch(ZoneNode baseNode, int zoneCount, string branch = "primary")
    {
        var prev = baseNode;

        if (zoneCount < 1)
            return prev;

        // Generate the zones for this branch
        for (var i = 0; i < zoneCount; i++)
        {
            var zoneIndex = level.Planner.NextIndex(director.Bulkhead);
            var next = new ZoneNode(director.Bulkhead, zoneIndex, branch);
            var nextZone = new Zone
            {
                Coverage = CoverageMinMax.GenNormalSize(),
                LightSettings = Lights.GenRandomLight(),
            };
            nextZone.RollFog(level);

            level.Planner.Connect(prev, next);
            level.Planner.AddZone(next, nextZone);

            prev = next;
        }

        return prev;
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="baseNode"></param>
    /// <param name="zoneCount"></param>
    /// <param name="branch"></param>
    /// <param name="zoneCallback"></param>
    /// <returns></returns>
    public ICollection<ZoneNode> AddBranch(
        ZoneNode baseNode,
        int zoneCount,
        string branch = "primary",
        Action<Zone>? zoneCallback = null)
    {
        var prev = baseNode;

        // If we don't have any zones to add, just return the base node.
        // Note that we do not invoke the zone callback in this case.
        if (zoneCount < 1)
            return new List<ZoneNode> { baseNode };

        var insertedNodes = new List<ZoneNode>();

        // Generate the zones for this branch
        for (var i = 0; i < zoneCount; i++)
        {
            var zoneIndex = level.Planner.NextIndex(director.Bulkhead);
            var next = new ZoneNode(director.Bulkhead, zoneIndex, branch);
            var nextZone = new Zone
            {
                Coverage = CoverageMinMax.GenNormalSize(),
                LightSettings = Lights.GenRandomLight(),
            };
            nextZone.RollFog(level);

            level.Planner.Connect(prev, next);
            level.Planner.AddZone(next, nextZone);

            insertedNodes.Add(next);

            zoneCallback?.Invoke(nextZone);

            prev = next;
        }

        return insertedNodes;
    }
    #endregion

    #region General layout building blocks

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
        if (errorZones < 1)
        {
            Plugin.Logger.LogError($"BuildChallenge_ErrorWithOff_KeycardInSide(): Called with invalid number of error zones: {errorZones}");
            return (start, planner.GetZone(start)!);
        }

        // We need to scale up the resources in the error zones as the alarms can be quite challenging to work through
        var resourceMultiplier = level.Tier switch
        {
            _ => 3.0
        };

        // first error alarm zone
        var (firstError, firstErrorZone) = AddZone(
            start,
            new ZoneNode { Branch = "primary", MaxConnections = 3 });
        firstErrorZone.Coverage = CoverageMinMax.Large;
        firstErrorZone.SetMainResourceMulti(value => value * 3);

        // Add subsequent error alarm zones to go through
        var penultimate = AddBranch(
            firstError,
            errorZones - 1,
            "primary",
            zone => zone.SetMainResourceMulti(value => value * 3)).Last();
        planner.UpdateNode(penultimate with { MaxConnections = 2 });

        // Add penultimate zone
        var (end, endZone) = AddZone(penultimate, new ZoneNode { Branch = "primary" });
        endZone.SetMainResourceMulti(value => value * 3);
        // endZone.Coverage = CoverageMinMax.Medium;

        // Build the side chain for the keycard if we have keycard zones
        // If this is zero it will just default to returning the penultimate zone
        var keycard = AddBranch(
            penultimate,
            sideKeycardZones,
            "keycard",
            zone => zone.SetMainResourceMulti(value => value * 3)).Last();

        // Lock the end zone
        AddKeycardPuzzle(end, keycard);

        // Build terminal zones
        // If `terminalTurnoffZones` is set to zero, then the turnoff terminal will be placed in the end zone.
        ZoneNode? terminal = AddBranch(
            end,
            terminalTurnoffZones,
            "error_turnoff",
            zone => zone.SetMainResourceMulti(value => value * 3)).Last();

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
        AddErrorAlarm(firstError, terminal, ChainedPuzzle.AlarmError_Baseline with
        {
            PersistentId = 0,
            Population = population,
            Settings = settings
        });

        return (end, endZone);
    }

    #endregion
}
