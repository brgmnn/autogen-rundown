using Newtonsoft.Json;

namespace AutogenRundown.DataBlocks.Custom.AdvancedWardenObjective;

public record SetZoneLight
{
    [JsonIgnore]
    public SetZoneLightType SetLight { get; set; } = SetZoneLightType.Change;

    public string Type
    {
        get => SetLight switch
        {
            SetZoneLightType.Change => "SetZoneLightData",
            SetZoneLightType.Revert => "RevertToOriginal"
        };
        private set { }
    }

    /// <summary>
    /// LightSettingsDB id to change zone lights to.
    /// </summary>
    [JsonProperty("LightDataID")]
    public uint LightSettings { get; set; } = 0u;

    /// <summary>
    /// How long to transition between the lights
    /// </summary>
    [JsonProperty("TransitionDuration")]
    public double Duration { get; set; } = 1.0;

    public int Seed { get; set; } = 0;
}
