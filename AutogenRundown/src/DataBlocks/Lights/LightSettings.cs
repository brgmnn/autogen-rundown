using Newtonsoft.Json;

namespace AutogenRundown.DataBlocks.Light;

/// <summary>
/// It appears that when using AWO to set lights in a zone, we will need to ensure there are
/// category definitions for those lights _even if we don't want them at all_. For instance
/// if we want door lights to be off, we would need to set the category definition for
/// door lights with the following:
///
/// ```
/// new LightCategorySetting { Category = LightCategory.Door, Chance = 0.0 },
/// ```
///
/// It's not totally clear why we need to do that. Though it does open the possibility that we
/// could use it to only set the desired lights by only overriding the category settings that
/// we want? TBD. I suggest we always set all the categories if we want to set via AWO.
///
/// In vanilla it looks like they do omit light category settings.
/// </summary>
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

    #region Persistence
    public void Persist(LazyBlocksBin<LightSettings>? bin = null)
    {
        bin ??= Bins.LightSettings;
        bin.AddBlock(this);
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="lights"></param>
    /// <returns></returns>
    public static LightSettings FindOrPersist(LightSettings lights)
    {
        // We specifically don't want to persist none, as we want to set the PersistentID to 0
        if (lights == None)
            return None;

        var existing = Bins.LightSettings
            .GetBlock(block => block.CategorySettings == lights.CategorySettings);

        if (existing != null)
            return existing;

        if (lights.PersistentId == 0)
            lights.PersistentId = Generator.GetPersistentId();

        lights.Persist();

        return lights;
    }

    /// <summary>
    /// Instance version of static method
    /// </summary>
    /// <returns></returns>
    public LightSettings FindOrPersist() => FindOrPersist(this);
    #endregion

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
