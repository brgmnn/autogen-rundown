using AutogenRundown.DataBlocks.Custom.ExtraObjectiveSetup;
using AutogenRundown.DataBlocks.Objectives;
using AutogenRundown.Extensions;

namespace AutogenRundown.DataBlocks;

public partial record LevelLayout
{
    /// <summary>
    /// Adds sensors to this quadrant
    /// </summary>
    /// <param name="quadrant"></param>
    public void AddSecuritySensors((int, int) quadrant)
    {
        var sensorEvents = new List<WardenObjectiveEvent>();

        sensorEvents.AddSecuritySensors(false, 0, 0.1)
            .AddSound(Sound.LightsOff)
            .AddSecuritySensors(true, 0, 4.5);

        var sensor = new SecuritySensor
        {
            EventsOnTrigger = sensorEvents
        };

        sensor.AddInQuadrant(quadrant, 20);

        level.EOS_SecuritySensor.Definitions.Add(sensor);
    }
}
