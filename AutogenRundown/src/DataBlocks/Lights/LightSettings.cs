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
public record LightSettings : DataBlock<LightSettings>
{
    public static readonly LightSettings None = new() { PersistentId = 0 };

    #region Generator Lights
    /// <summary>
    /// The Auxiliary Power light is supposed to be used for illuminating a zone after a generator
    /// cell has been inserted.
    /// </summary>
    public static readonly LightSettings AuxiliaryPower = new()
    {
        PersistentId = Generator.GetPersistentId(),
        CategorySettings = new List<LightCategorySetting>
        {
            new()
            {
                Color = new() { Red = 0.5, Green = 0.45, Blue = 0.42 },
                Category = LightCategory.Sign,
                Chance = 1.0,
                ChanceBroken = 0.08,
                Intensity = 0.8,
                On = true
            },

            // Poor illumination for doors
            new()
            {
                Color = new() { Red = 1.0, Green = 0.9, Blue = 0.8 },
                Category = LightCategory.Door,
                Chance = 1.0,
                Intensity = 0.5,
                On = true
            },

            // Only the next zone doors get decent lights
            new()
            {
                Color = new() { Red = 0.9255541, Green = 1.0, Blue = 0.7877358 },
                Category = LightCategory.DoorImportant,
                Chance = 1.0,
                Intensity = 1.0,
                On = true
            },

            // The Aux lights are mostly emergency, and special. With tiny chance for general
            new()
            {
                Color = new() { Red = 1.0, Green = 0.27, Blue = 0.27 },
                Category = LightCategory.Emergency,
                Chance = 1.0,
                Intensity = 0.9,
                On = true
            },
            new()
            {
                Color = new() { Red = 1.0, Green = 0.27, Blue = 0.27 },
                Category = LightCategory.Special,
                Chance = 0.4,
                Intensity = 0.75,
                On = true
            },
            new()
            {
                Color = new() { Red = 1.0, Green = 1.0, Blue = 0.7877358 },
                Category = LightCategory.Special,
                Chance = 1.0,
                ChanceBroken = 0.03,
                Intensity = 0.6,
                On = true
            },
            new()
            {
                Color = new() { Red = 1.0, Green = 1.0, Blue = 0.7877358 },
                Category = LightCategory.General,
                Chance = 0.04,
                ChanceBroken = 0.03,
                Intensity = 0.3,
                On = true
            },

            LightCategorySetting.Off(LightCategory.Independent),
        }
    };
    #endregion

    public static readonly LightSettings LightsOff = new()
    {
        PersistentId = Generator.GetPersistentId(),
        CategorySettings = new List<LightCategorySetting>()
        {
            LightCategorySetting.Off(LightCategory.General),
            LightCategorySetting.Off(LightCategory.Special),
            LightCategorySetting.Off(LightCategory.Emergency),
            LightCategorySetting.Off(LightCategory.Independent),
            LightCategorySetting.Off(LightCategory.Door),
            LightCategorySetting.Off(LightCategory.Sign),
            LightCategorySetting.Off(LightCategory.DoorImportant),
        }
    };

    public static readonly LightSettings ErrorFlashOn = new()
    {
        PersistentId = Generator.GetPersistentId(),
        CategorySettings = new List<LightCategorySetting>()
        {
            new()
            {
                Color = new() { Red = 1.0, Green = 0.27, Blue = 0.27 },
                Category = LightCategory.General,
                Chance = 1.0,
                Intensity = 0.9,
                On = true
            },

            LightCategorySetting.Off(LightCategory.Special),
            LightCategorySetting.Off(LightCategory.Emergency),
            LightCategorySetting.Off(LightCategory.Independent),
            LightCategorySetting.Off(LightCategory.Door),
            LightCategorySetting.Off(LightCategory.Sign),
            LightCategorySetting.Off(LightCategory.DoorImportant),
        }
    };
    //
    // public static readonly LightSettings RedZoneDoor = new()
    // {
    //     PersistentId = Generator.GetPersistentId(),
    //     CategorySettings = new List<LightCategorySetting>()
    //     {
    //         new()
    //         {
    //             Color = new() { Red = 1.0, Green = 0.27, Blue = 0.27 },
    //             Category = LightCategory.DoorImportant,
    //             Chance = 1.0,
    //             Intensity = 1.2,
    //             On = true
    //         },
    //     }
    // };

    public new static void SaveStatic()
    {
        AuxiliaryPower.Persist();

        LightsOff.Persist();
        ErrorFlashOn.Persist();

        // RedZoneDoor.Persist();
    }

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
        => Setup<GameDataLightSettings>(Bins.LightSettings, "LightSettings");

    #endregion
}
