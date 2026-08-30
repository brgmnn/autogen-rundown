using AutogenRundown.DataBlocks.Custom.AdvancedWardenObjective;
using AutogenRundown.DataBlocks.Custom.ZoneSensors;
using AutogenRundown.DataBlocks.Enemies;
using AutogenRundown.DataBlocks.Objectives;
using AutogenRundown.DataBlocks.Levels;
using AutogenRundown.DataBlocks.Zones;
using AutogenRundown.Extensions;

namespace AutogenRundown.DataBlocks;

public partial record LevelLayout
{
    /// <summary>
    /// Adds difficulty-scaled security sensors to a zone.
    /// Parameters are randomized based on level tier if not specified.
    /// </summary>
    /// <param name="node">Zone to add sensors to</param>
    /// <param name="wave">Enemy wave to spawn on trigger (null = auto-select based on tier)</param>
    /// <param name="moving">Whether sensors should patrol (null = random based on tier)</param>
    public void AddSecuritySensors(ZoneNode node, GenericWave? wave = null, bool? moving = null)
    {
        planner.UpdateNode(node with { Tags = node.Tags.Extend("security_sensors") });

        // Only the first sensor group placed on the level contributes an intel candidate
        if (level.ZoneSensors.Count == 0)
        {
            #region Warden Intel Messages
            level.ElevatorDropWardenIntel.Add((Generator.Between(1, 5), Generator.Draw(new List<string>
            {
                ">... [distorted static]\r\n>... There's that broken scan again.\r\n<size=200%><color=red>>... Step away from it!</color></size>",
                ">... <size=200%><color=red>Careful!</color></size>\r\n>... The corrupted scan just re-initialized.\r\n>... No telling what it summons.",
                ">... <size=200%><color=red>Don't stand on that scanner!</color></size>\r\n>... Last time, we barely escaped.\r\n>... It calls forth... something.",
                ">... <size=200%><color=red>The scan just reset!</color></size>\r\n>... It's only a matter of time.\r\n>... Brace yourselves.",
                ">... [sensor flicker]\r\n>... There's movement, then nothing.\r\n>... <size=200%><color=red>Something's slipping past the scan!</color></size>",
                ">... [low static]\r\n>... The console blinked... then cleared.\r\n>... <size=200%><color=red>It's hiding right under our noses!</color></size>",
                ">... [short beep]\r\n>... We lost a blip on the radar.\r\n>... <size=200%><color=red>Keep your backs covered!</color></size>",
                ">... The scan shows a distortion.\r\n>... Then it vanishes instantly.\r\n>... <size=200%><color=red>Something doesn't want to be found!</color></size>",
                ">... [electrical buzz]\r\n>... The reading just dropped to zero.\r\n>... <size=200%><color=red>But we know it's still out there!</color></size>",
                ">... [whispered curses]\r\n>... The sensor's glitching again.\r\n>... <size=200%><color=red>Something's tampering with our detection!</color></size>",
                ">... [panicked breath]\r\n>... <size=200%><color=red>Who just disappeared?!</color></size>\r\n>... We didn't even see it happen!",
                ">... That faint silhouette is back.\r\n>... The scan can't lock on.\r\n>... <size=200%><color=red>It slips away in the dark!</color></size>",
                ">... [quiet step]\r\n>... <size=200%><color=red>Did anyone hear that?</color></size>\r\n>... Nothing shows on the scanner!",
                ">... The device beeped once.\r\n>... Then total silence.\r\n>... <size=200%><color=red>Something is watching, unseen!</color></size>",
                ">... [frantic re-calibration]\r\n>... The reading remains hidden.\r\n>... <size=200%><color=red>It knows how to avoid detection!</color></size>",
                ">... The sensor froze for a second.\r\n>... Felt like something brushed by.\r\n>... <size=200%><color=red>Keep your eyes open!</color></size>",
                ">... [sudden static burst]\r\n>... The console lit up, then died.\r\n>... <size=200%><color=red>It's messing with our gear!</color></size>",
                ">... He's missing?\r\n>... No trace, no struggle.\r\n>... <size=200%><color=red>This can't be natural!</color></size>",
                ">... [low beep]\r\n>... The sensor caught a shape, briefly.\r\n>... <size=200%><color=red>It vanished like a ghost!</color></size>",
                ">... [soft rustling]\r\n>... Something dragged them away...\r\n>... <size=200%><color=red>We saw no attacker!</color></size>",
                ">... [faint moan]\r\n>... We heard them cry out.\r\n>... <size=200%><color=red>But there's no one there now!</color></size>",
                ">... The logs show anomalies.\r\n>... One moment they're here...\r\n>... <size=200%><color=red>Next moment they're gone!</color></size>",
                ">... [low humming]\r\n>... <size=200%><color=red>Freeze! There's a circle right by your boot!</color></size>\r\n>... Back up. Slow. Slower.",
                ">... The light on the floor, see it?\r\n>... I see three of them. Maybe four.\r\n>... <size=200%><color=red>There is no way through!</color></size>",
                ">... <size=200%><color=red>He walked straight into it!</color></size>\r\n>... The whole ring lit up red.\r\n>... [alarm blaring]",
                ">... It's moving. The circle is moving.\r\n>... <size=200%><color=red>It's crawling along the walkway toward us!</color></size>\r\n>... Move! Find the gap before it closes!",
                ">... Don't move. Don't even breathe.\r\n>... It's drifting past me... it's on my boots...\r\n>... <size=200%><color=red>It went through him. IT WENT THROUGH HIM.</color></size>",
                ">... <size=200%><color=red>It blinked out! Go, go, go!</color></size>\r\n>... How long do we have?\r\n>... Nobody knows. Nobody ever knows.",
                ">... The floor's dark. It's been dark a while now.\r\n>... <size=200%><color=red>That doesn't mean safe. That means it's due.</color></size>\r\n>... Count with me.",
                ">... It's herding him into the corner.\r\n>... There's nowhere left to stand!\r\n>... <size=200%><color=red>Jump it! Just jump it!</color></size>",
                ">... Somebody tripped one.\r\n>... [distant roaring]\r\n>... <size=200%><color=red>They heard it. All of them heard it.</color></size>",
                ">... <size=200%><color=red>Never trust an empty room down here.</color></size>\r\n>... The clean floors are always the worst ones.\r\n>... Something is watching the ground.",
                ">... Wait, was that a glow behind us?\r\n>... <size=200%><color=red>We already walked through it.</color></size>\r\n>... [rising screeching]",
                ">... Step where I step. Exactly where I step.\r\n>... I can't see past your light!\r\n>... <size=200%><color=red>Then stop. Stop right now.</color></size>",
                ">... <size=200%><color=red>Two of them just crossed paths!</color></size>\r\n>... The gap is gone.\r\n>... We wait. We just... wait.",
                ">... [scanner pulsing]\r\n>... <size=200%><color=red>It caught his leg. Only his leg.</color></size>\r\n>... Doesn't matter. It counted him.",
            }))!);
            #endregion
        }

        // 1. Determine sensor density by tier (count calculated at runtime from zone area)
        //    And radius
        var (density, radius) = level.Tier switch
        {
            "A" => Generator.Select(new List<(double, (SensorDensity, double))>
            {
                (1.0, (SensorDensity.Low, 2.3)),
                (0.4, (SensorDensity.Medium, 1.2)),
            }),

            "B" => Generator.Select(new List<(double, (SensorDensity, double))>
            {
                (1.0, (SensorDensity.Low, 2.3)),
                (0.6, (SensorDensity.Medium, 1.2)),
                (0.1, (SensorDensity.Medium, 2.3)),
            }),

            "C" => Generator.Select(new List<(double, (SensorDensity, double))>
            {
                (1.0, (SensorDensity.Low, 2.3)),
                (1.0, (SensorDensity.Medium, 1.2)),
                (0.3, (SensorDensity.Medium, 2.3)),
                (0.3, (SensorDensity.High, 1.2)),
            }),

            "D" => Generator.Select(new List<(double, (SensorDensity, double))>
            {
                (0.5, (SensorDensity.Low, 2.3)),
                (1.0, (SensorDensity.Medium, 1.2)),
                (1.0, (SensorDensity.Medium, 2.3)),
                (0.6, (SensorDensity.High, 1.2)),
                (0.1, (SensorDensity.VeryHigh, 1.2)),
            }),

            "E" => Generator.Select(new List<(double, (SensorDensity, double))>
            {
                (1.0, (SensorDensity.Medium, 2.3)),
                (1.0, (SensorDensity.High, 1.2)),
                (0.7, (SensorDensity.Medium, 2.3)),
                (0.5, (SensorDensity.VeryHigh, 1.2)),
            }),

            _ => (SensorDensity.Medium, 2.3)
        };

        // 2. Determine if sensors move
        var movingChance = (density, level.Tier) switch
        {
            (SensorDensity.Low, "A") => 0.40,
            (SensorDensity.Low, "B") => 0.45,
            (SensorDensity.Low, "C") => 0.52,
            (SensorDensity.Low, "D") => 0.60,
            (SensorDensity.Low, "E") => 0.66,

            (SensorDensity.Medium, "A" or "B") => 0.33,
            (SensorDensity.Medium, _)          => 0.50,

            (SensorDensity.High, "C") => 0.21,
            (SensorDensity.High, "D") => 0.33,
            (SensorDensity.High, "E") => 0.45,
            (SensorDensity.High, _)   => 0.05,

            (SensorDensity.VeryHigh, "D") => 0.08,
            (SensorDensity.VeryHigh, "E") => 0.17,
            (SensorDensity.VeryHigh, _)   => 0.00,

            _ => 0.5
        };
        var isMoving = moving ?? Generator.Flip(movingChance);

        // 3. Determine TriggerEach (independent triggering)
        var triggerEachChance = (density, level.Tier) switch
        {
            (SensorDensity.Low, _) => 0.33,

            (SensorDensity.Medium, "A" or "B") => 0.65,
            (SensorDensity.Medium, _)          => 0.50,

            (SensorDensity.High, "C") => 0.82,
            (SensorDensity.High, "D") => 0.75,
            (SensorDensity.High, "E") => 0.60,
            (SensorDensity.High, _)   => 0.90,

            (SensorDensity.VeryHigh, "D") => 0.95,
            (SensorDensity.VeryHigh, "E") => 0.90,
            (SensorDensity.VeryHigh, _)   => 1.00,

            _ => 0.5
        };
        var triggerEach = Generator.Flip(triggerEachChance);

        // 4. Select wave (easier for moving sensors)
        var selectedWave = wave ?? SelectWaveForTier(isMoving);

        // 5. Determine if sensors should cycle on/off
        var cycleChance = level.Tier switch
        {
            "A" => 0.70,
            "B" => 0.55,
            "C" => 0.40,
            "D" => 0.25,
            "E" => 0.15,
            _ => 0.40
        };
        var shouldCycle = triggerEach && Generator.Flip(cycleChance);

        // 6. Create sensor definition first to get its ID
        var sensorDef = new ZoneSensorDefinition
        {
            Bulkhead = node.Bulkhead,
            ZoneNumber = node.ZoneNumber,
            SensorGroups = new List<ZoneSensorGroupDefinition>
            {
                new()
                {
                    Density = density,
                    Moving = isMoving ? Generator.Between(2, 3) : 1,
                    Speed = isMoving ? Generator.NextDouble(0.6, 0.85) : 0.6,
                    TriggerEach = triggerEach,
                    Radius = radius
                }
            }
        };

        // 7. Setup cycling event loop if applicable
        if (shouldCycle)
        {
            var loopIndex = 300 + level.ZoneSensors.Count;
            // var offTime = Generator.Between(50, 70);
            // var onTime = Generator.Between(15, 30);
            var offTime = Generator.Between(3, 18);
            var onTime = Generator.Between(8, 25);
            var cycleTime = offTime + onTime;

            var eventLoop = new EventLoop
            {
                LoopIndex = loopIndex,
                LoopDelay = cycleTime,
                LoopCount = -1
            };

            eventLoop.EventsToActivate
                .DisableZoneSensors(sensorDef.Id, 0.4);

            if (triggerEach)
                eventLoop.EventsToActivate.EnableZoneSensors(sensorDef.Id, offTime);
            else
                eventLoop.EventsToActivate.EnableZoneSensorsWithReset(sensorDef.Id, offTime);

            level.GetObjective(node.Bulkhead).EventsOnElevatorLand.Add(
                new WardenObjectiveEvent
                {
                    Type = WardenObjectiveEventType.StartEventLoop,
                    EventLoop = eventLoop,
                    Delay = Generator.Between(5, 15)
                });
        }

        // 8. Build trigger events
        var sensorEvents = new List<WardenObjectiveEvent>();
        sensorEvents
            .AddZoneSound(Sound.LightsOff, sensorDef)
            .AddSpawnWave(selectedWave, 2.0);

        if (!shouldCycle && !triggerEach)
        {
            var resetTime = Generator.Between(8, 15);
            sensorEvents
                .EnableZoneSensorsWithReset(sensorDef.Id, resetTime)
                .AddZoneSound(Sound.LightsOn_Vol4, sensorDef, resetTime - 0.4);
        }

        sensorDef.EventsOnTrigger = sensorEvents;

        Plugin.Logger.LogDebug($"{Name} -- Security Sensors: " +
                               $"zone={node}, " +
                               $"density={density}, " +
                               $"moving={isMoving}, " +
                               $"triggerEach={triggerEach}, " +
                               $"cycling={shouldCycle}, " +
                               $"id={sensorDef.Id}");

        level.ZoneSensors.Add(sensorDef);
    }

    /// <summary>
    /// Selects an appropriate wave based on tier and level modifiers.
    /// Builds a weighted pool of finite-count sensor waves, then picks one via Generator.Select.
    /// Moving sensors have boss-tier options removed.
    /// </summary>
    private GenericWave SelectWaveForTier(bool isMoving)
    {
        var settings = level.Settings;
        var options = new List<(double, GenericWave)>();

        switch (level.Tier)
        {
            case "A":
                options.Add((1.0, GenericWave.Sensor_6pts));
                break;

            case "B":
                options.Add((0.4, GenericWave.Sensor_6pts));
                options.Add((1.0, GenericWave.Sensor_8pts));
                options.Add((0.4, GenericWave.Sensor_Shooters_6pts));
                break;

            case "C":
                options.Add((0.3, GenericWave.Sensor_Shooters_6pts));
                options.Add((0.4, GenericWave.Sensor_8pts));
                options.Add((1.0, GenericWave.Sensor_12pts));
                break;

            case "D":
                options.Add((0.3, GenericWave.Sensor_12pts));
                options.Add((0.4, GenericWave.Sensor_Shooters_12pts));
                options.Add((1.0, GenericWave.Sensor_16pts));
                options.Add((0.25, GenericWave.SingleMother));
                options.Add((0.15, GenericWave.SingleTank));
                break;

            case "E":
                options.Add((0.3, GenericWave.Sensor_Shooters_12pts));
                options.Add((1.0, GenericWave.Sensor_16pts));
                options.Add((0.4, GenericWave.SingleTank));
                options.Add((0.3, GenericWave.SinglePouncer));
                options.Add((0.2, GenericWave.SingleMother));
                break;

            default:
                options.Add((1.0, GenericWave.Sensor_8pts));
                break;
        }

        if (settings.HasChargers())
            options.Add((0.6, GenericWave.Sensor_Chargers_8pts));

        if (settings.HasShadows())
            options.Add((0.5, GenericWave.Sensor_Shadows_12pts));

        if (settings.HasShadows() && level.Tier is "D")
            options.Add((0.2, GenericWave.SinglePouncerShadow));

        if (settings.HasShadows() && level.Tier is "E")
            options.Add((0.35, GenericWave.SinglePouncerShadow));

        if (settings.HasNightmares())
            options.Add((0.5, GenericWave.Sensor_Nightmares_8pts));

        if (settings.Modifiers.Contains(LevelModifiers.Hybrids))
            options.Add((0.4, GenericWave.Sensor_Hybrids_8pts));

        if (isMoving)
            options.RemoveAll(o =>
                o.Item2 == GenericWave.SingleMother ||
                o.Item2 == GenericWave.SingleTank);

        return Generator.Select(options);
    }
}
