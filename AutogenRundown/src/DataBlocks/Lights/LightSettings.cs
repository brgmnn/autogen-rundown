using Newtonsoft.Json;

namespace AutogenRundown.DataBlocks.Light;

public record LightSettings : DataBlock
{
    public static readonly LightSettings None = new() { PersistentId = 0 };

    /// <summary>
    /// TODO: Check what happens if we have multiple category settings with the same category?
    /// I assume they're randomly picked between?
    /// </summary>
    [JsonProperty("LightCategorySettings")]
    public List<LightCategorySetting> CategorySettings { get; set; } = new();

    public LightSettings(PidOffsets offsets = PidOffsets.Normal)
        : base(Generator.GetPersistentId(offsets))
    { }


    #region Loading in the base game lights
    record GameDataLightSettings : LightSettings
    {
        public GameDataLightSettings() : base(PidOffsets.None)
        { }
    }

    public static void Setup()
        => Setup<GameDataLightSettings, LightSettings>(Bins.LightSettings, "LightSettings");
    #endregion
}
