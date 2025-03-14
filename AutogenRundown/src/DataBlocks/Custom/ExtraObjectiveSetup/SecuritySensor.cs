using AutogenRundown.DataBlocks.Custom.ExtraObjectiveSetup.ExtensionSecuritySensor;
using AutogenRundown.DataBlocks.Objectives;
using Newtonsoft.Json;

namespace AutogenRundown.DataBlocks.Custom.ExtraObjectiveSetup;

// Looks like it will be quite hard to add these as we have to specify the sensor position absolutely in the world
// coordinates.
//
// Seems the map is broken up into 64x64 unit squares. Elevator is in quadrant 0,0.
public class SecuritySensor : Definition
{
    [JsonProperty("SensorGroup")]
    public List<Sensor> SensorGroups { get; set; } = new();

    public List<WardenObjectiveEvent> EventsOnTrigger { get; set; } = new();

    /// <summary>
    /// Adds a random number of randomly placed sensors within the quadrant.
    /// </summary>
    /// <param name="quadrant">
    /// (X, Y) tuple. Each quadrant is a 64x64 block square on the map. Elevator is a (0, 0).
    /// </param>
    /// <param name="baseSensor"></param>
    /// <param name="count"></param>
    public void AddInQuadrant((int, int) quadrant, int count = 10)
    {
        var (quadrantX, quadrantZ) = quadrant;

        for (var i = 0; i < count; i++)
        {
            SensorGroups.Add(new Sensor()
            {
                Position = new SensorPosition
                {
                    X = quadrantX*64 - 32 + Generator.Between(4, 60),
                    Z = quadrantZ*64 - 32 + Generator.Between(4, 60),
                    Y = 0
                },
                Radius = 3.5,
                Text = Sensor.GenText()
            });
        }
    }
}
