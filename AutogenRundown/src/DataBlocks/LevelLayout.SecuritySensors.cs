using AutogenRundown.DataBlocks.Custom.ExtraObjectiveSetup;
using AutogenRundown.DataBlocks.Enemies;
using AutogenRundown.DataBlocks.Objectives;
using AutogenRundown.Extensions;

namespace AutogenRundown.DataBlocks;

public partial record LevelLayout
{
    /// <summary>
    /// Adds sensors to this quadrant
    /// </summary>
    /// <param name="quadrant"></param>
    /// <param name="resets"></param>
    public void AddSecuritySensors_SinglePouncer((int, int) quadrant, bool resets = true)
    {
        var sensorEvents = new List<WardenObjectiveEvent>();

        sensorEvents
            .AddToggleSecuritySensors(false, 0, 0.1)
            .AddSound(Sound.LightsOff)
            .AddSpawnWave(GenericWave.SinglePouncer, 2.0);

        var resetTime = Generator.Between(8, 15);

        if (resets)
            sensorEvents
                .AddToggleSecuritySensors(true, 0, resetTime)
                .AddSound(Sound.LightsOn_Vol4, resetTime - 0.4);

        var sensor = new SecuritySensor
        {
            EventsOnTrigger = sensorEvents
        };

        var count = level.Tier switch
        {
            "B" => 8,
            "C" => 10,
            "D" => 12,
            "E" => 15,
            _ => 10
        };

        Plugin.Logger.LogDebug($"{Name} -- Rolled Security Sensors: quadrant = {quadrant}, count = {count}");

        sensor.AddInQuadrant(quadrant, count);

        level.EOS_SecuritySensor.Definitions.Add(sensor);

        #region Warden Intel Messages
        /*
         * HSU Find Sample specific warden intel messages
         */
        level.ElevatorDropWardenIntel.Add((Generator.Between(1, 5), Generator.Draw(new List<string>
        {
            ">... [distorted static]\r\n>... There's that broken scan again.\r\n<size=200%><color=red>>... Step away from it!</color></size>",
            ">... <size=200%><color=red>Careful!</color></size>\r\n>... The corrupted scan just re-initialized.\r\n>... No telling what it summons.",
            ">... <size=200%><color=red>Don't stand on that scanner!</color></size>\r\n>... Last time, we barely escaped.\r\n>... It calls forth... something.",
            ">... <size=200%><color=red>The scan just reset!</color></size>\r\n>... It's only a matter of time.\r\n>... Brace yourselves."
        }))!);
        #endregion
    }
}
