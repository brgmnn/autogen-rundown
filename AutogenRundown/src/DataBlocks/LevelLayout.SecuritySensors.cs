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

        if (resets)
            sensorEvents
                .AddToggleSecuritySensors(true, 0, 9.0)
                .AddSound(Sound.LightsOn_Vol4, 8.6);

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
            _ => 0,
        };

        Plugin.Logger.LogDebug($"{Name} -- Rolled Security Sensors: quadrant = {quadrant}, count = {count}");

        sensor.AddInQuadrant(quadrant, count);

        level.EOS_SecuritySensor.Definitions.Add(sensor);
    }
}
