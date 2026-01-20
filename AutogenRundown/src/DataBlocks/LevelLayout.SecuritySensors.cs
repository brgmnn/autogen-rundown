using AutogenRundown.DataBlocks.Custom.ZoneSensors;
using AutogenRundown.DataBlocks.Enemies;
using AutogenRundown.DataBlocks.Objectives;
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
        var sensorEvents = new List<WardenObjectiveEvent>();

        sensorEvents
            .DisableZoneSensors(0, 0.1)
            .AddSound(Sound.LightsOff)
            .AddSpawnWave(wave, 2.0);

        if (resets)
        {
            var resetTime = Generator.Between(8, 15);

            sensorEvents
                .EnableZoneSensorsWithReset(0, resetTime)
                .AddSound(Sound.LightsOn_Vol4, resetTime - 0.4);
        }

        var count = level.Tier switch
        {
            "B" => 8,
            "C" => 10,
            "D" => 12,
            "E" => 15,
            _ => 10
        };

        Plugin.Logger.LogDebug($"{Name} -- Rolled Security Sensors: zone = {node}, count = {count}");

        level.ZoneSensors.Add(new ZoneSensorDefinition
        {
            Bulkhead = node.Bulkhead,
            ZoneNumber = node.ZoneNumber,
            SensorGroups = new List<ZoneSensorGroupDefinition>
            {
                new ZoneSensorGroupDefinition { Count = count }
            },
            EventsOnTrigger = sensorEvents
        });
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
}
