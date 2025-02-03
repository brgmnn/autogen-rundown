using AutogenRundown.DataBlocks.Custom.ExtraObjectiveSetup;
using AutogenRundown.DataBlocks.Objectives;
using AutogenRundown.DataBlocks.ZoneData;
using AutogenRundown.DataBlocks.Zones;

namespace AutogenRundown.DataBlocks;

public partial record LevelLayout : DataBlock
{
    /// <summary>
    /// There is a chance to lock the reactor itself and require the team to get a code for it
    /// </summary>
    /// <param name="director"></param>
    /// <param name="objective"></param>
    /// <param name="startish"></param>
    /// <exception cref="Exception"></exception>
    public void BuildLayout_ReactorShutdown(
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

        // Create some initial zones
        var prev = BuildBranch(start, Generator.Between(0, 2), "primary");

        var reactor = BuildReactor(prev);
        var reactorZone = planner.GetZone(reactor)!;

        // Create the reactor definitions
        var reactorDefinition = new ReactorShutdown()
        {
            ZoneNumber = reactor.ZoneNumber,
            Bulkhead = director.Bulkhead
        };

        // If the reactor is locked, they will need to fetch a code for it.
        var lockedReactor = Generator.Flip(
            (director.Tier, director.Bulkhead) switch
            {
                ("A", Bulkhead.Main) => 0.2,
                ("A", _) => 0.0,

                ("B", Bulkhead.Main) => 0.4,

                ("C", Bulkhead.Main) => 0.5,
                ("C", Bulkhead.Extreme) => 0.4,

                ("D", Bulkhead.Main) => 0.7,
                ("D", Bulkhead.Extreme) => 0.5,

                ("E", Bulkhead.Main) => 0.9,
                ("E", Bulkhead.Extreme) => 0.6,
                ("E", Bulkhead.Overload) => 0.4,

                (_, _) => 0.3
            });

        // Set up the password retrieval for the reactor
        if (lockedReactor)
        {
            // If we have a password on the terminal, make the reactor light red and or flickering:
            //  -> RedToWhite_1_Flickering
            // Or maybe:
            //      Reactor_blue_to_White_R2E1 (but red)
            reactorZone.LightSettings = Lights.Light.Monochrome_Red_R7D1;

            var codeTerminal = BuildBranch(
                reactor,
                Generator.Between(1, 2),
                "reactor_password");

            reactorDefinition.Password.PasswordProtected = true;
            reactorDefinition.Password.TerminalZoneSelectionDatas = new()
            {
                new()
                {
                    new ZoneSelectionData
                    {
                        ZoneNumber = codeTerminal.ZoneNumber,
                        Bulkhead = director.Bulkhead
                    }
                }
            };
        }

        // Some objective texts
        // Find the main reactor and shut it down
        objective.MainObjective = $"Find the main reactor in {Lore.Zone(ZoneAliasStart + reactor.ZoneNumber)} and shut it down";
        objective.GoToZone = $"Navigate to {Lore.Zone(ZoneAliasStart + reactor.ZoneNumber)} and initiate the shutdown process";

        objective.LayoutDefinitions!.Definitions.Add(reactorDefinition);
    }
}

