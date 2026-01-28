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
    /// Adds security sensors to this zone
    /// </summary>
    /// <param name="node"></param>
    /// <param name="wave"></param>
    /// <param name="resets"></param>
    public void AddSecuritySensors_Simple(ZoneNode node, GenericWave wave, bool resets = true)
    {
        var count = level.Tier switch
        {
            "B" => 8,
            "C" => 10,
            "D" => 12,
            "E" => 15,
            _ => 10
        };

        // Create sensor definition first to get its ID
        var sensorDef = new ZoneSensorDefinition
        {
            Bulkhead = node.Bulkhead,
            ZoneNumber = node.ZoneNumber,
            SensorGroups = new List<ZoneSensorGroupDefinition>
            {
                new ZoneSensorGroupDefinition { Count = count }
            }
        };

        // Build events using the definition's ID
        var sensorEvents = new List<WardenObjectiveEvent>();
        sensorEvents
            .DisableZoneSensors(sensorDef.Id, 0.1)
            .AddSound(Sound.LightsOff)
            .AddSpawnWave(wave, 2.0);

        if (resets)
        {
            var resetTime = Generator.Between(8, 15);

            sensorEvents
                .EnableZoneSensorsWithReset(sensorDef.Id, resetTime)
                .AddSound(Sound.LightsOn_Vol4, resetTime - 0.4);
        }

        sensorDef.EventsOnTrigger = sensorEvents;

        Plugin.Logger.LogDebug($"{Name} -- Rolled Security Sensors: zone = {node}, count = {count}, id = {sensorDef.Id}");

        level.ZoneSensors.Add(sensorDef);
    }

    /// <summary>
    /// Adds sensors to this zone
    /// </summary>
    /// <param name="node"></param>
    /// <param name="resets"></param>
    public void AddSecuritySensors_SinglePouncer(ZoneNode node, bool resets = true)
    {
        AddSecuritySensors_Simple(node, GenericWave.SinglePouncer, resets);

        #region Warden Intel Messages
        level.ElevatorDropWardenIntel.Add((Generator.Between(1, 5), Generator.Draw(new List<string>
        {
            ">... [distorted static]\r\n>... There's that broken scan again.\r\n<size=200%><color=red>>... Step away from it!</color></size>",
            ">... <size=200%><color=red>Careful!</color></size>\r\n>... The corrupted scan just re-initialized.\r\n>... No telling what it summons.",
            ">... <size=200%><color=red>Don't stand on that scanner!</color></size>\r\n>... Last time, we barely escaped.\r\n>... It calls forth... something.",
            ">... <size=200%><color=red>The scan just reset!</color></size>\r\n>... It's only a matter of time.\r\n>... Brace yourselves."
        }))!);
        #endregion
    }

    /// <summary>
    /// Adds sensors to this zone
    /// </summary>
    /// <param name="node"></param>
    /// <param name="resets"></param>
    public void AddSecuritySensors_SinglePouncerShadow(ZoneNode node, bool resets = true)
    {
        AddSecuritySensors_Simple(node, GenericWave.SinglePouncerShadow, resets);

        #region Warden Intel Messages
        level.ElevatorDropWardenIntel.Add((Generator.Between(1, 5), Generator.Draw(new List<string>
        {
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
            // ">... The readout spikes erratically.\r\n>... Then it drops to zero.\r\n>... <size=200%><color=red>It's in our blind spots!</color></size>",
            ">... [faint moan]\r\n>... We heard them cry out.\r\n>... <size=200%><color=red>But there's no one there now!</color></size>",
            ">... The logs show anomalies.\r\n>... One moment they're here...\r\n>... <size=200%><color=red>Next moment they're gone!</color></size>",
        }))!);
        #endregion
    }

    /// <summary>
    /// Adds difficulty-scaled security sensors to a zone.
    /// Parameters are randomized based on level tier if not specified.
    /// </summary>
    /// <param name="node">Zone to add sensors to</param>
    /// <param name="wave">Enemy wave to spawn on trigger (null = auto-select based on tier)</param>
    /// <param name="moving">Whether sensors should patrol (null = random based on tier)</param>
    public void AddSecuritySensors(ZoneNode node, GenericWave? wave = null, bool? moving = null)
    {
        // 1. Determine sensor density by tier (count calculated at runtime from zone area)
        var density = level.Tier switch
        {
            "A" => SensorDensity.Low,
            "B" => SensorDensity.Low,
            "C" => SensorDensity.Medium,
            "D" => SensorDensity.Medium,
            "E" => SensorDensity.High,
            _ => SensorDensity.Medium
        };

        // 2. Determine if sensors move
        var movingChance = level.Tier switch
        {
            "A" => 0.10,
            "B" => 0.15,
            "C" => 0.20,
            "D" => 0.30,
            "E" => 0.40,
            _ => 0.15
        };
        var isMoving = moving ?? Generator.Flip(movingChance);

        // 3. Determine TriggerEach (independent triggering)
        var triggerEachChance = level.Tier switch
        {
            "A" => 0.80,
            "B" => 0.75,
            "C" => 0.65,
            "D" => 0.55,
            "E" => 0.45,
            _ => 0.70
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
            "E" => 0.10,
            _ => 0.40
        };
        var shouldCycle = Generator.Flip(cycleChance);

        // 6. Determine sensor radius by tier
        var radius = level.Tier switch
        {
            "A" => 2.0,
            "B" => 2.2,
            "C" => 2.3,
            "D" => 2.4,
            "E" => 2.5,
            _ => 2.3
        };

        // 7. Create sensor definition first to get its ID
        var sensorDef = new ZoneSensorDefinition
        {
            Bulkhead = node.Bulkhead,
            ZoneNumber = node.ZoneNumber,
            SensorGroups = new List<ZoneSensorGroupDefinition>
            {
                new ZoneSensorGroupDefinition
                {
                    Density = density,
                    Moving = isMoving ? Generator.Between(2, 4) : 1,
                    Speed = isMoving ? Generator.NextDouble(1.2, 2.0) : 1.5,
                    TriggerEach = triggerEach,
                    Radius = radius
                }
            }
        };

        // 8. Setup cycling event loop if applicable
        if (shouldCycle)
        {
            var loopIndex = 300 + level.ZoneSensors.Count;
            var cycleTime = Generator.Between(8, 15);
            var offTime = Generator.Between(3, 6);

            var eventLoop = new EventLoop
            {
                LoopIndex = loopIndex,
                LoopDelay = cycleTime,
                LoopCount = -1
            };

            eventLoop.EventsToActivate
                .DisableZoneSensors(sensorDef.Id, 0.0)
                .AddSound(Sound.LightsOff, 0.0)
                .EnableZoneSensorsWithReset(sensorDef.Id, offTime)
                .AddSound(Sound.LightsOn_Vol4, offTime - 0.4);

            level.Objective[node.Bulkhead].EventsOnElevatorLand.Add(
                new WardenObjectiveEvent
                {
                    Type = WardenObjectiveEventType.StartEventLoop,
                    EventLoop = eventLoop,
                    Delay = Generator.Between(5, 15)
                });
        }

        // 9. Build trigger events
        var sensorEvents = new List<WardenObjectiveEvent>();
        sensorEvents
            .DisableZoneSensors(sensorDef.Id, 0.1)
            .AddSound(Sound.LightsOff)
            .AddSpawnWave(selectedWave, 2.0);

        if (!shouldCycle)
        {
            var resetTime = Generator.Between(8, 15);
            sensorEvents
                .EnableZoneSensorsWithReset(sensorDef.Id, resetTime)
                .AddSound(Sound.LightsOn_Vol4, resetTime - 0.4);
        }

        sensorDef.EventsOnTrigger = sensorEvents;

        Plugin.Logger.LogDebug($"{Name} -- Security Sensors: zone={node}, density={density}, " +
            $"moving={isMoving}, triggerEach={triggerEach}, cycling={shouldCycle}, id={sensorDef.Id}");

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
                options.Add((1.0, GenericWave.Sensor_8pts));
                break;

            case "B":
                options.Add((0.4, GenericWave.Sensor_8pts));
                options.Add((1.0, GenericWave.Sensor_12pts));
                break;

            case "C":
                options.Add((0.3, GenericWave.Sensor_12pts));
                options.Add((1.0, GenericWave.Sensor_16pts));
                break;

            case "D":
                options.Add((0.3, GenericWave.Sensor_16pts));
                options.Add((1.0, GenericWave.Sensor_20pts));
                options.Add((0.25, GenericWave.SingleMother));
                options.Add((0.15, GenericWave.SingleTank));
                break;

            case "E":
                options.Add((1.0, GenericWave.Sensor_20pts));
                options.Add((0.4, GenericWave.SingleTank));
                options.Add((0.3, GenericWave.SinglePouncer));
                options.Add((0.2, GenericWave.SingleMother));
                break;

            default:
                options.Add((1.0, GenericWave.Sensor_12pts));
                break;
        }

        if (settings.HasChargers())
            options.Add((0.6, GenericWave.Sensor_Chargers_12pts));

        if (settings.HasShadows())
            options.Add((0.5, GenericWave.Sensor_Shadows_8pts));

        if (settings.HasNightmares())
            options.Add((0.5, GenericWave.Sensor_Nightmares_12pts));

        if (settings.Modifiers.Contains(LevelModifiers.Hybrids))
            options.Add((0.4, GenericWave.Sensor_Hybrids_8pts));

        if (isMoving)
            options.RemoveAll(o =>
                o.Item2 == GenericWave.SingleMother ||
                o.Item2 == GenericWave.SingleTank ||
                o.Item2 == GenericWave.SinglePouncer);

        return Generator.Select(options);
    }
}
