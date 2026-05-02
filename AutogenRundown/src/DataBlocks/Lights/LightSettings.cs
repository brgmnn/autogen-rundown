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

    #region Red Theme Lights
    /// <summary>
    /// Bright red, based on vanilla Monochrome_Red (PID 20)
    /// </summary>
    public static readonly LightSettings RedTheme_Bright = new()
    {
        PersistentId = Generator.GetPersistentId(),
        CategorySettings = new List<LightCategorySetting>
        {
            new() { Category = LightCategory.General, Color = new() { Red = 0.86, Green = 0.30, Blue = 0.30 }, Chance = 0.5, ChanceBroken = 0.05, Intensity = 0.6 },
            new() { Category = LightCategory.General, Color = new() { Red = 0.90, Green = 0.40, Blue = 0.20 }, Chance = 0.5, ChanceBroken = 0.05, Intensity = 0.55 },
            new() { Category = LightCategory.Special, Color = new() { Red = 0.90, Green = 0.76, Blue = 0.76, Alpha = 0.0 }, Chance = 0.5, Intensity = 0.7 },
            new() { Category = LightCategory.Special, Color = new() { Red = 0.92, Green = 0.82, Blue = 0.78, Alpha = 0.0 }, Chance = 0.5, Intensity = 0.6 },
            LightCategorySetting.Off(LightCategory.Emergency),
            new() { Category = LightCategory.Independent, Color = new() { Red = 0.90, Green = 0.23, Blue = 0.23 }, Chance = 0.5, Intensity = 0.7 },
            new() { Category = LightCategory.Door, Color = new() { Red = 0.63, Green = 0.13, Blue = 0.13 }, Chance = 0.5, ChanceBroken = 0.05, Intensity = 0.7 },
            new() { Category = LightCategory.Sign, Color = new() { Red = 0.66, Green = 0.13, Blue = 0.13 }, Chance = 0.5, ChanceBroken = 0.05, Intensity = 0.7 },
            new() { Category = LightCategory.DoorImportant, Color = new() { Red = 0.90, Green = 0.83, Blue = 0.83 }, Chance = 1.0, Intensity = 0.7 },
        }
    };

    /// <summary>
    /// Very dark red, based on vanilla Monochrome_Red_R1D1_z9 (PID 87)
    /// </summary>
    public static readonly LightSettings RedTheme_Dark = new()
    {
        PersistentId = Generator.GetPersistentId(),
        CategorySettings = new List<LightCategorySetting>
        {
            new() { Category = LightCategory.General, Color = new() { Red = 0.90, Green = 0.20, Blue = 0.20 }, Chance = 0.5, ChanceBroken = 0.05, Intensity = 0.15 },
            new() { Category = LightCategory.General, Color = new() { Red = 0.70, Green = 0.15, Blue = 0.15 }, Chance = 0.5, ChanceBroken = 0.05, Intensity = 0.2 },
            new() { Category = LightCategory.Special, Color = new() { Red = 0.90, Green = 0.76, Blue = 0.76, Alpha = 0.0 }, Chance = 0.5, Intensity = 0.5 },
            new() { Category = LightCategory.Special, Color = new() { Red = 0.80, Green = 0.65, Blue = 0.62, Alpha = 0.0 }, Chance = 0.5, Intensity = 0.4 },
            LightCategorySetting.Off(LightCategory.Emergency),
            new() { Category = LightCategory.Independent, Color = new() { Red = 0.90, Green = 0.20, Blue = 0.20 }, Chance = 0.5, ChanceBroken = 0.05, Intensity = 0.6 },
            new() { Category = LightCategory.Door, Color = new() { Red = 0.63, Green = 0.06, Blue = 0.06 }, Chance = 0.5, ChanceBroken = 0.05, Intensity = 1.0 },
            new() { Category = LightCategory.Sign, Color = new() { Red = 0.66, Green = 0.07, Blue = 0.07 }, Chance = 0.5, ChanceBroken = 0.05, Intensity = 1.0 },
            new() { Category = LightCategory.DoorImportant, Color = new() { Red = 0.90, Green = 0.83, Blue = 0.83 }, Chance = 1.0, Intensity = 0.5 },
        }
    };

    /// <summary>
    /// Flickering red, based on vanilla Monochrome_Red_Copy (PID 69)
    /// </summary>
    public static readonly LightSettings RedTheme_Flickering = new()
    {
        PersistentId = Generator.GetPersistentId(),
        CategorySettings = new List<LightCategorySetting>
        {
            new() { Category = LightCategory.General, Color = new() { Red = 0.90, Green = 0.32, Blue = 0.32 }, Chance = 0.5, ChanceBroken = 0.5, Intensity = 0.4 },
            new() { Category = LightCategory.General, Color = new() { Red = 0.88, Green = 0.38, Blue = 0.18 }, Chance = 0.5, ChanceBroken = 0.5, Intensity = 0.35 },
            new() { Category = LightCategory.Special, Color = new() { Red = 0.90, Green = 0.76, Blue = 0.76, Alpha = 0.0 }, Chance = 0.5, Intensity = 0.8 },
            new() { Category = LightCategory.Special, Color = new() { Red = 0.90, Green = 0.80, Blue = 0.72, Alpha = 0.0 }, Chance = 0.5, Intensity = 0.7 },
            LightCategorySetting.Off(LightCategory.Emergency),
            new() { Category = LightCategory.Independent, Color = new() { Red = 0.90, Green = 0.27, Blue = 0.27 }, Chance = 0.5, Intensity = 0.3 },
            new() { Category = LightCategory.Door, Color = new() { Red = 0.63, Green = 0.13, Blue = 0.13 }, Chance = 0.5, ChanceBroken = 0.5, Intensity = 0.5 },
            new() { Category = LightCategory.Sign, Color = new() { Red = 0.66, Green = 0.13, Blue = 0.13 }, Chance = 0.5, ChanceBroken = 0.5, Intensity = 0.3 },
            new() { Category = LightCategory.DoorImportant, Color = new() { Red = 0.90, Green = 0.83, Blue = 0.83 }, Chance = 1.0, Intensity = 0.8 },
        }
    };

    /// <summary>
    /// Deep crimson, based on vanilla Monochrome_Red_R7D1 (PID 83)
    /// </summary>
    public static readonly LightSettings RedTheme_Deep = new()
    {
        PersistentId = Generator.GetPersistentId(),
        CategorySettings = new List<LightCategorySetting>
        {
            new() { Category = LightCategory.General, Color = new() { Red = 0.90, Green = 0.20, Blue = 0.20 }, Chance = 0.5, ChanceBroken = 0.5, Intensity = 1.0 },
            new() { Category = LightCategory.General, Color = new() { Red = 0.75, Green = 0.12, Blue = 0.12 }, Chance = 0.5, ChanceBroken = 0.5, Intensity = 0.8 },
            new() { Category = LightCategory.Special, Color = new() { Red = 0.90, Green = 0.76, Blue = 0.76, Alpha = 0.0 }, Chance = 0.5, Intensity = 1.0 },
            new() { Category = LightCategory.Special, Color = new() { Red = 0.82, Green = 0.60, Blue = 0.58, Alpha = 0.0 }, Chance = 0.5, Intensity = 0.8 },
            LightCategorySetting.Off(LightCategory.Emergency),
            new() { Category = LightCategory.Independent, Color = new() { Red = 0.90, Green = 0.20, Blue = 0.20 }, Chance = 0.5, Intensity = 1.0 },
            new() { Category = LightCategory.Door, Color = new() { Red = 0.63, Green = 0.06, Blue = 0.06 }, Chance = 0.5, ChanceBroken = 0.5, Intensity = 1.0 },
            new() { Category = LightCategory.Sign, Color = new() { Red = 0.66, Green = 0.07, Blue = 0.07 }, Chance = 0.5, ChanceBroken = 0.5, Intensity = 1.0 },
            new() { Category = LightCategory.DoorImportant, Color = new() { Red = 0.68, Green = 0.25, Blue = 0.25 }, Chance = 1.0, Intensity = 0.6 },
        }
    };

    /// <summary>
    /// Warm red with hint of orange
    /// </summary>
    public static readonly LightSettings RedTheme_Warm = new()
    {
        PersistentId = Generator.GetPersistentId(),
        CategorySettings = new List<LightCategorySetting>
        {
            new() { Category = LightCategory.General, Color = new() { Red = 0.90, Green = 0.35, Blue = 0.18 }, Chance = 0.5, ChanceBroken = 0.05, Intensity = 0.55 },
            new() { Category = LightCategory.General, Color = new() { Red = 0.88, Green = 0.22, Blue = 0.22 }, Chance = 0.5, ChanceBroken = 0.05, Intensity = 0.5 },
            new() { Category = LightCategory.Special, Color = new() { Red = 0.85, Green = 0.45, Blue = 0.30, Alpha = 0.0 }, Chance = 0.5, Intensity = 0.6 },
            new() { Category = LightCategory.Special, Color = new() { Red = 0.92, Green = 0.82, Blue = 0.72, Alpha = 0.0 }, Chance = 0.5, Intensity = 0.55 },
            LightCategorySetting.Off(LightCategory.Emergency),
            new() { Category = LightCategory.Independent, Color = new() { Red = 0.90, Green = 0.30, Blue = 0.15 }, Chance = 0.5, Intensity = 0.6 },
            new() { Category = LightCategory.Door, Color = new() { Red = 0.70, Green = 0.20, Blue = 0.10 }, Chance = 0.5, ChanceBroken = 0.05, Intensity = 0.7 },
            new() { Category = LightCategory.Sign, Color = new() { Red = 0.75, Green = 0.22, Blue = 0.10 }, Chance = 0.5, ChanceBroken = 0.05, Intensity = 0.7 },
            new() { Category = LightCategory.DoorImportant, Color = new() { Red = 0.90, Green = 0.83, Blue = 0.83 }, Chance = 1.0, Intensity = 0.7 },
        }
    };

    public static List<LightSettings> RedThemeLights { get; } = new()
    {
        RedTheme_Bright,
        RedTheme_Dark,
        RedTheme_Flickering,
        RedTheme_Deep,
        RedTheme_Warm,
    };
    #endregion

    #region Green Theme Lights
    /// <summary>
    /// Vibrant blue-green, based on vanilla Monochrome_Green (PID 22)
    /// </summary>
    public static readonly LightSettings GreenTheme_Bright = new()
    {
        PersistentId = Generator.GetPersistentId(),
        CategorySettings = new List<LightCategorySetting>
        {
            new() { Category = LightCategory.General, Color = new() { Red = 0.35, Green = 0.68, Blue = 0.55 }, Chance = 0.5, ChanceBroken = 0.05, Intensity = 0.7 },
            new() { Category = LightCategory.General, Color = new() { Red = 0.22, Green = 0.55, Blue = 0.58 }, Chance = 0.5, ChanceBroken = 0.05, Intensity = 0.6 },
            new() { Category = LightCategory.Special, Color = new() { Red = 0.65, Green = 0.85, Blue = 0.78, Alpha = 0.0 }, Chance = 0.5, Intensity = 0.7 },
            new() { Category = LightCategory.Special, Color = new() { Red = 0.78, Green = 0.88, Blue = 0.84, Alpha = 0.0 }, Chance = 0.5, Intensity = 0.6 },
            LightCategorySetting.Off(LightCategory.Emergency),
            new() { Category = LightCategory.Independent, Color = new() { Red = 0.18, Green = 0.58, Blue = 0.42 }, Chance = 0.5, Intensity = 0.7 },
            new() { Category = LightCategory.Door, Color = new() { Red = 0.75, Green = 0.85, Blue = 0.82 }, Chance = 0.5, ChanceBroken = 0.05, Intensity = 0.7 },
            new() { Category = LightCategory.Sign, Color = new() { Red = 0.38, Green = 0.75, Blue = 0.60 }, Chance = 0.5, ChanceBroken = 0.05, Intensity = 0.7 },
            new() { Category = LightCategory.DoorImportant, Color = new() { Red = 0.74, Green = 0.83, Blue = 0.78 }, Chance = 1.0, Intensity = 0.7 },
        }
    };

    /// <summary>
    /// Dark muted green, based on vanilla Monochrome_Green_R4C2 (PID 98)
    /// </summary>
    public static readonly LightSettings GreenTheme_Dark = new()
    {
        PersistentId = Generator.GetPersistentId(),
        CategorySettings = new List<LightCategorySetting>
        {
            new() { Category = LightCategory.General, Color = new() { Red = 0.13, Green = 0.28, Blue = 0.20 }, Chance = 0.3, ChanceBroken = 0.3, Intensity = 0.25 },
            new() { Category = LightCategory.General, Color = new() { Red = 0.10, Green = 0.22, Blue = 0.25 }, Chance = 0.3, ChanceBroken = 0.3, Intensity = 0.2 },
            new() { Category = LightCategory.Special, Color = new() { Red = 0.30, Green = 0.40, Blue = 0.38, Alpha = 0.0 }, Chance = 0.5, Intensity = 0.25 },
            new() { Category = LightCategory.Special, Color = new() { Red = 0.45, Green = 0.55, Blue = 0.52, Alpha = 0.0 }, Chance = 0.5, Intensity = 0.2 },
            LightCategorySetting.Off(LightCategory.Emergency),
            new() { Category = LightCategory.Independent, Color = new() { Red = 0.11, Green = 0.35, Blue = 0.26 }, Chance = 0.5, Intensity = 0.25 },
            new() { Category = LightCategory.Door, Color = new() { Red = 0.75, Green = 0.85, Blue = 0.82 }, Chance = 0.5, ChanceBroken = 0.05, Intensity = 0.5 },
            new() { Category = LightCategory.Sign, Color = new() { Red = 0.22, Green = 0.42, Blue = 0.35 }, Chance = 0.3, ChanceBroken = 1.0, Intensity = 0.25 },
            new() { Category = LightCategory.DoorImportant, Color = new() { Red = 0.74, Green = 0.83, Blue = 0.78 }, Chance = 1.0, Intensity = 0.5 },
        }
    };

    /// <summary>
    /// Flickering eerie green
    /// </summary>
    public static readonly LightSettings GreenTheme_Flickering = new()
    {
        PersistentId = Generator.GetPersistentId(),
        CategorySettings = new List<LightCategorySetting>
        {
            new() { Category = LightCategory.General, Color = new() { Red = 0.28, Green = 0.60, Blue = 0.48 }, Chance = 0.5, ChanceBroken = 0.5, Intensity = 0.4 },
            new() { Category = LightCategory.General, Color = new() { Red = 0.20, Green = 0.50, Blue = 0.52 }, Chance = 0.5, ChanceBroken = 0.5, Intensity = 0.35 },
            new() { Category = LightCategory.Special, Color = new() { Red = 0.50, Green = 0.75, Blue = 0.68, Alpha = 0.0 }, Chance = 0.5, ChanceBroken = 0.3, Intensity = 0.5 },
            new() { Category = LightCategory.Special, Color = new() { Red = 0.72, Green = 0.82, Blue = 0.78, Alpha = 0.0 }, Chance = 0.5, ChanceBroken = 0.3, Intensity = 0.45 },
            LightCategorySetting.Off(LightCategory.Emergency),
            new() { Category = LightCategory.Independent, Color = new() { Red = 0.18, Green = 0.50, Blue = 0.38 }, Chance = 0.5, ChanceBroken = 0.5, Intensity = 0.35 },
            new() { Category = LightCategory.Door, Color = new() { Red = 0.65, Green = 0.80, Blue = 0.75 }, Chance = 0.5, ChanceBroken = 0.5, Intensity = 0.45 },
            new() { Category = LightCategory.Sign, Color = new() { Red = 0.32, Green = 0.65, Blue = 0.52 }, Chance = 0.5, ChanceBroken = 0.5, Intensity = 0.4 },
            new() { Category = LightCategory.DoorImportant, Color = new() { Red = 0.74, Green = 0.83, Blue = 0.78 }, Chance = 1.0, Intensity = 0.6 },
        }
    };

    /// <summary>
    /// Cold blue-green tint
    /// </summary>
    public static readonly LightSettings GreenTheme_Cold = new()
    {
        PersistentId = Generator.GetPersistentId(),
        CategorySettings = new List<LightCategorySetting>
        {
            new() { Category = LightCategory.General, Color = new() { Red = 0.20, Green = 0.52, Blue = 0.55 }, Chance = 0.5, ChanceBroken = 0.05, Intensity = 0.5 },
            new() { Category = LightCategory.General, Color = new() { Red = 0.15, Green = 0.42, Blue = 0.50 }, Chance = 0.5, ChanceBroken = 0.05, Intensity = 0.45 },
            new() { Category = LightCategory.Special, Color = new() { Red = 0.42, Green = 0.70, Blue = 0.72, Alpha = 0.0 }, Chance = 0.5, Intensity = 0.55 },
            new() { Category = LightCategory.Special, Color = new() { Red = 0.80, Green = 0.88, Blue = 0.90, Alpha = 0.0 }, Chance = 0.5, Intensity = 0.5 },
            LightCategorySetting.Off(LightCategory.Emergency),
            new() { Category = LightCategory.Independent, Color = new() { Red = 0.15, Green = 0.45, Blue = 0.50 }, Chance = 0.5, Intensity = 0.45 },
            new() { Category = LightCategory.Door, Color = new() { Red = 0.60, Green = 0.78, Blue = 0.80 }, Chance = 0.5, ChanceBroken = 0.05, Intensity = 0.55 },
            new() { Category = LightCategory.Sign, Color = new() { Red = 0.25, Green = 0.58, Blue = 0.60 }, Chance = 0.5, ChanceBroken = 0.05, Intensity = 0.5 },
            new() { Category = LightCategory.DoorImportant, Color = new() { Red = 0.74, Green = 0.83, Blue = 0.78 }, Chance = 1.0, Intensity = 0.6 },
        }
    };

    /// <summary>
    /// Saturated toxic green with blue tint
    /// </summary>
    public static readonly LightSettings GreenTheme_Toxic = new()
    {
        PersistentId = Generator.GetPersistentId(),
        CategorySettings = new List<LightCategorySetting>
        {
            new() { Category = LightCategory.General, Color = new() { Red = 0.22, Green = 0.75, Blue = 0.35 }, Chance = 0.5, ChanceBroken = 0.1, Intensity = 0.55 },
            new() { Category = LightCategory.General, Color = new() { Red = 0.35, Green = 0.65, Blue = 0.25 }, Chance = 0.5, ChanceBroken = 0.1, Intensity = 0.5 },
            new() { Category = LightCategory.Special, Color = new() { Red = 0.45, Green = 0.80, Blue = 0.50, Alpha = 0.0 }, Chance = 0.5, Intensity = 0.6 },
            new() { Category = LightCategory.Special, Color = new() { Red = 0.75, Green = 0.85, Blue = 0.65, Alpha = 0.0 }, Chance = 0.5, Intensity = 0.55 },
            LightCategorySetting.Off(LightCategory.Emergency),
            new() { Category = LightCategory.Independent, Color = new() { Red = 0.20, Green = 0.65, Blue = 0.30 }, Chance = 0.5, Intensity = 0.5 },
            new() { Category = LightCategory.Door, Color = new() { Red = 0.42, Green = 0.72, Blue = 0.45 }, Chance = 0.5, ChanceBroken = 0.1, Intensity = 0.6 },
            new() { Category = LightCategory.Sign, Color = new() { Red = 0.28, Green = 0.75, Blue = 0.38 }, Chance = 0.5, ChanceBroken = 0.1, Intensity = 0.55 },
            new() { Category = LightCategory.DoorImportant, Color = new() { Red = 0.74, Green = 0.83, Blue = 0.78 }, Chance = 1.0, Intensity = 0.6 },
        }
    };

    public static List<LightSettings> GreenThemeLights { get; } = new()
    {
        GreenTheme_Bright,
        GreenTheme_Dark,
        GreenTheme_Flickering,
        GreenTheme_Cold,
        GreenTheme_Toxic,
    };
    #endregion

    #region Orange Theme Lights
    /// <summary>
    /// Bright warm orange
    /// </summary>
    public static readonly LightSettings OrangeTheme_Bright = new()
    {
        PersistentId = Generator.GetPersistentId(),
        CategorySettings = new List<LightCategorySetting>
        {
            new() { Category = LightCategory.General, Color = new() { Red = 0.90, Green = 0.55, Blue = 0.15 }, Chance = 0.5, ChanceBroken = 0.05, Intensity = 0.6 },
            new() { Category = LightCategory.General, Color = new() { Red = 0.92, Green = 0.68, Blue = 0.30 }, Chance = 0.5, ChanceBroken = 0.05, Intensity = 0.55 },
            new() { Category = LightCategory.Special, Color = new() { Red = 0.92, Green = 0.70, Blue = 0.40, Alpha = 0.0 }, Chance = 0.5, Intensity = 0.7 },
            new() { Category = LightCategory.Special, Color = new() { Red = 0.94, Green = 0.88, Blue = 0.75, Alpha = 0.0 }, Chance = 0.5, Intensity = 0.6 },
            LightCategorySetting.Off(LightCategory.Emergency),
            new() { Category = LightCategory.Independent, Color = new() { Red = 0.90, Green = 0.50, Blue = 0.12 }, Chance = 0.5, Intensity = 0.65 },
            new() { Category = LightCategory.Door, Color = new() { Red = 0.85, Green = 0.50, Blue = 0.15 }, Chance = 0.5, ChanceBroken = 0.05, Intensity = 0.7 },
            new() { Category = LightCategory.Sign, Color = new() { Red = 0.88, Green = 0.52, Blue = 0.13 }, Chance = 0.5, ChanceBroken = 0.05, Intensity = 0.7 },
            new() { Category = LightCategory.DoorImportant, Color = new() { Red = 0.92, Green = 0.85, Blue = 0.72 }, Chance = 1.0, Intensity = 0.7 },
        }
    };

    /// <summary>
    /// Deep amber, moodier and darker
    /// </summary>
    public static readonly LightSettings OrangeTheme_Amber = new()
    {
        PersistentId = Generator.GetPersistentId(),
        CategorySettings = new List<LightCategorySetting>
        {
            new() { Category = LightCategory.General, Color = new() { Red = 0.85, Green = 0.45, Blue = 0.10 }, Chance = 0.5, ChanceBroken = 0.05, Intensity = 0.35 },
            new() { Category = LightCategory.General, Color = new() { Red = 0.80, Green = 0.35, Blue = 0.08 }, Chance = 0.5, ChanceBroken = 0.05, Intensity = 0.3 },
            new() { Category = LightCategory.Special, Color = new() { Red = 0.88, Green = 0.58, Blue = 0.28, Alpha = 0.0 }, Chance = 0.5, Intensity = 0.5 },
            new() { Category = LightCategory.Special, Color = new() { Red = 0.88, Green = 0.75, Blue = 0.55, Alpha = 0.0 }, Chance = 0.5, Intensity = 0.45 },
            LightCategorySetting.Off(LightCategory.Emergency),
            new() { Category = LightCategory.Independent, Color = new() { Red = 0.82, Green = 0.40, Blue = 0.08 }, Chance = 0.5, ChanceBroken = 0.05, Intensity = 0.5 },
            new() { Category = LightCategory.Door, Color = new() { Red = 0.80, Green = 0.42, Blue = 0.10 }, Chance = 0.5, ChanceBroken = 0.05, Intensity = 0.6 },
            new() { Category = LightCategory.Sign, Color = new() { Red = 0.82, Green = 0.44, Blue = 0.10 }, Chance = 0.5, ChanceBroken = 0.05, Intensity = 0.55 },
            new() { Category = LightCategory.DoorImportant, Color = new() { Red = 0.90, Green = 0.82, Blue = 0.68 }, Chance = 1.0, Intensity = 0.6 },
        }
    };

    /// <summary>
    /// Flickering orange, unsettling atmosphere
    /// </summary>
    public static readonly LightSettings OrangeTheme_Flickering = new()
    {
        PersistentId = Generator.GetPersistentId(),
        CategorySettings = new List<LightCategorySetting>
        {
            new() { Category = LightCategory.General, Color = new() { Red = 0.88, Green = 0.50, Blue = 0.13 }, Chance = 0.5, ChanceBroken = 0.4, Intensity = 0.45 },
            new() { Category = LightCategory.General, Color = new() { Red = 0.90, Green = 0.40, Blue = 0.10 }, Chance = 0.5, ChanceBroken = 0.4, Intensity = 0.4 },
            new() { Category = LightCategory.Special, Color = new() { Red = 0.90, Green = 0.65, Blue = 0.35, Alpha = 0.0 }, Chance = 0.5, ChanceBroken = 0.3, Intensity = 0.55 },
            new() { Category = LightCategory.Special, Color = new() { Red = 0.90, Green = 0.82, Blue = 0.68, Alpha = 0.0 }, Chance = 0.5, ChanceBroken = 0.3, Intensity = 0.5 },
            LightCategorySetting.Off(LightCategory.Emergency),
            new() { Category = LightCategory.Independent, Color = new() { Red = 0.85, Green = 0.45, Blue = 0.10 }, Chance = 0.5, ChanceBroken = 0.4, Intensity = 0.4 },
            new() { Category = LightCategory.Door, Color = new() { Red = 0.82, Green = 0.48, Blue = 0.14 }, Chance = 0.5, ChanceBroken = 0.3, Intensity = 0.5 },
            new() { Category = LightCategory.Sign, Color = new() { Red = 0.85, Green = 0.50, Blue = 0.12 }, Chance = 0.5, ChanceBroken = 0.4, Intensity = 0.45 },
            new() { Category = LightCategory.DoorImportant, Color = new() { Red = 0.90, Green = 0.82, Blue = 0.68 }, Chance = 1.0, Intensity = 0.65 },
        }
    };

    /// <summary>
    /// Warm cream white — provides contrast to orange zones
    /// </summary>
    public static readonly LightSettings OrangeTheme_Cream = new()
    {
        PersistentId = Generator.GetPersistentId(),
        CategorySettings = new List<LightCategorySetting>
        {
            new() { Category = LightCategory.General, Color = new() { Red = 0.95, Green = 0.90, Blue = 0.75 }, Chance = 0.5, ChanceBroken = 0.05, Intensity = 0.5 },
            new() { Category = LightCategory.General, Color = new() { Red = 0.95, Green = 0.85, Blue = 0.70 }, Chance = 0.5, ChanceBroken = 0.05, Intensity = 0.45 },
            new() { Category = LightCategory.Special, Color = new() { Red = 0.96, Green = 0.92, Blue = 0.80, Alpha = 0.0 }, Chance = 0.5, Intensity = 0.6 },
            new() { Category = LightCategory.Special, Color = new() { Red = 0.94, Green = 0.93, Blue = 0.88, Alpha = 0.0 }, Chance = 0.5, Intensity = 0.55 },
            LightCategorySetting.Off(LightCategory.Emergency),
            new() { Category = LightCategory.Independent, Color = new() { Red = 0.94, Green = 0.88, Blue = 0.72 }, Chance = 0.5, Intensity = 0.55 },
            new() { Category = LightCategory.Door, Color = new() { Red = 0.92, Green = 0.88, Blue = 0.78 }, Chance = 0.5, ChanceBroken = 0.05, Intensity = 0.6 },
            new() { Category = LightCategory.Sign, Color = new() { Red = 0.94, Green = 0.90, Blue = 0.76 }, Chance = 0.5, ChanceBroken = 0.05, Intensity = 0.55 },
            new() { Category = LightCategory.DoorImportant, Color = new() { Red = 0.95, Green = 0.92, Blue = 0.82 }, Chance = 1.0, Intensity = 0.65 },
        }
    };

    /// <summary>
    /// Pale cool white — neutral contrast to orange zones
    /// </summary>
    public static readonly LightSettings OrangeTheme_PaleWhite = new()
    {
        PersistentId = Generator.GetPersistentId(),
        CategorySettings = new List<LightCategorySetting>
        {
            new() { Category = LightCategory.General, Color = new() { Red = 0.90, Green = 0.92, Blue = 0.88 }, Chance = 0.5, ChanceBroken = 0.05, Intensity = 0.5 },
            new() { Category = LightCategory.General, Color = new() { Red = 0.88, Green = 0.86, Blue = 0.82 }, Chance = 0.5, ChanceBroken = 0.05, Intensity = 0.45 },
            new() { Category = LightCategory.Special, Color = new() { Red = 0.92, Green = 0.94, Blue = 0.90, Alpha = 0.0 }, Chance = 0.5, Intensity = 0.6 },
            new() { Category = LightCategory.Special, Color = new() { Red = 0.92, Green = 0.93, Blue = 0.92, Alpha = 0.0 }, Chance = 0.5, Intensity = 0.55 },
            LightCategorySetting.Off(LightCategory.Emergency),
            new() { Category = LightCategory.Independent, Color = new() { Red = 0.88, Green = 0.90, Blue = 0.86 }, Chance = 0.5, Intensity = 0.55 },
            new() { Category = LightCategory.Door, Color = new() { Red = 0.90, Green = 0.90, Blue = 0.88 }, Chance = 0.5, ChanceBroken = 0.05, Intensity = 0.6 },
            new() { Category = LightCategory.Sign, Color = new() { Red = 0.88, Green = 0.90, Blue = 0.86 }, Chance = 0.5, ChanceBroken = 0.05, Intensity = 0.55 },
            new() { Category = LightCategory.DoorImportant, Color = new() { Red = 0.92, Green = 0.93, Blue = 0.90 }, Chance = 1.0, Intensity = 0.65 },
        }
    };

    public static List<LightSettings> OrangeThemeLights { get; } = new()
    {
        OrangeTheme_Bright,
        OrangeTheme_Amber,
        OrangeTheme_Flickering,
        OrangeTheme_Cream,
        OrangeTheme_PaleWhite,
    };
    #endregion

    #region White Theme Lights
    /// <summary>
    /// Clean off-white, mostly desaturated. The signature look for the Giants theme — reads
    /// as sterile/clinical without leaning into another colour. General/Independent stay
    /// neutral; a faint warm tint is allowed in the rarely-seen Sign category.
    /// </summary>
    public static readonly LightSettings WhiteTheme_Bright = new()
    {
        PersistentId = Generator.GetPersistentId(),
        CategorySettings = new List<LightCategorySetting>
        {
            new() { Category = LightCategory.General, Color = new() { Red = 0.92, Green = 0.92, Blue = 0.90 }, Chance = 0.5, ChanceBroken = 0.05, Intensity = 0.7 },
            new() { Category = LightCategory.General, Color = new() { Red = 0.88, Green = 0.90, Blue = 0.92 }, Chance = 0.5, ChanceBroken = 0.05, Intensity = 0.65 },
            new() { Category = LightCategory.Special, Color = new() { Red = 0.92, Green = 0.93, Blue = 0.92, Alpha = 0.0 }, Chance = 0.5, Intensity = 0.7 },
            new() { Category = LightCategory.Special, Color = new() { Red = 0.95, Green = 0.95, Blue = 0.93, Alpha = 0.0 }, Chance = 0.5, Intensity = 0.65 },
            LightCategorySetting.Off(LightCategory.Emergency),
            new() { Category = LightCategory.Independent, Color = new() { Red = 0.90, Green = 0.90, Blue = 0.88 }, Chance = 0.5, Intensity = 0.65 },
            new() { Category = LightCategory.Door, Color = new() { Red = 0.92, Green = 0.92, Blue = 0.92 }, Chance = 0.5, ChanceBroken = 0.05, Intensity = 0.7 },
            // Rare warm accent in the sign light only
            new() { Category = LightCategory.Sign, Color = new() { Red = 0.94, Green = 0.86, Blue = 0.70 }, Chance = 0.5, ChanceBroken = 0.05, Intensity = 0.65 },
            new() { Category = LightCategory.DoorImportant, Color = new() { Red = 0.94, Green = 0.94, Blue = 0.92 }, Chance = 1.0, Intensity = 0.7 },
        }
    };

    /// <summary>
    /// Cool grey-white. Slightly more blue-leaning than Bright, intensity dialled down a touch.
    /// </summary>
    public static readonly LightSettings WhiteTheme_Cool = new()
    {
        PersistentId = Generator.GetPersistentId(),
        CategorySettings = new List<LightCategorySetting>
        {
            new() { Category = LightCategory.General, Color = new() { Red = 0.82, Green = 0.86, Blue = 0.90 }, Chance = 0.5, ChanceBroken = 0.05, Intensity = 0.6 },
            new() { Category = LightCategory.General, Color = new() { Red = 0.80, Green = 0.84, Blue = 0.88 }, Chance = 0.5, ChanceBroken = 0.05, Intensity = 0.55 },
            new() { Category = LightCategory.Special, Color = new() { Red = 0.85, Green = 0.88, Blue = 0.92, Alpha = 0.0 }, Chance = 0.5, Intensity = 0.6 },
            new() { Category = LightCategory.Special, Color = new() { Red = 0.90, Green = 0.92, Blue = 0.94, Alpha = 0.0 }, Chance = 0.5, Intensity = 0.55 },
            LightCategorySetting.Off(LightCategory.Emergency),
            new() { Category = LightCategory.Independent, Color = new() { Red = 0.80, Green = 0.84, Blue = 0.88 }, Chance = 0.5, Intensity = 0.55 },
            new() { Category = LightCategory.Door, Color = new() { Red = 0.85, Green = 0.88, Blue = 0.90 }, Chance = 0.5, ChanceBroken = 0.05, Intensity = 0.6 },
            new() { Category = LightCategory.Sign, Color = new() { Red = 0.84, Green = 0.86, Blue = 0.90 }, Chance = 0.5, ChanceBroken = 0.05, Intensity = 0.6 },
            new() { Category = LightCategory.DoorImportant, Color = new() { Red = 0.92, Green = 0.93, Blue = 0.95 }, Chance = 1.0, Intensity = 0.65 },
        }
    };

    /// <summary>
    /// Dim grey, low intensity throughout. Reads as power-saving / standby lighting.
    /// </summary>
    public static readonly LightSettings WhiteTheme_Dim = new()
    {
        PersistentId = Generator.GetPersistentId(),
        CategorySettings = new List<LightCategorySetting>
        {
            new() { Category = LightCategory.General, Color = new() { Red = 0.55, Green = 0.55, Blue = 0.55 }, Chance = 0.4, ChanceBroken = 0.2, Intensity = 0.3 },
            new() { Category = LightCategory.General, Color = new() { Red = 0.50, Green = 0.52, Blue = 0.55 }, Chance = 0.4, ChanceBroken = 0.2, Intensity = 0.25 },
            new() { Category = LightCategory.Special, Color = new() { Red = 0.65, Green = 0.65, Blue = 0.65, Alpha = 0.0 }, Chance = 0.5, Intensity = 0.3 },
            new() { Category = LightCategory.Special, Color = new() { Red = 0.70, Green = 0.70, Blue = 0.70, Alpha = 0.0 }, Chance = 0.5, Intensity = 0.28 },
            LightCategorySetting.Off(LightCategory.Emergency),
            new() { Category = LightCategory.Independent, Color = new() { Red = 0.55, Green = 0.55, Blue = 0.53 }, Chance = 0.5, Intensity = 0.3 },
            new() { Category = LightCategory.Door, Color = new() { Red = 0.75, Green = 0.75, Blue = 0.74 }, Chance = 0.5, ChanceBroken = 0.1, Intensity = 0.5 },
            new() { Category = LightCategory.Sign, Color = new() { Red = 0.62, Green = 0.62, Blue = 0.60 }, Chance = 0.5, ChanceBroken = 0.1, Intensity = 0.35 },
            new() { Category = LightCategory.DoorImportant, Color = new() { Red = 0.88, Green = 0.88, Blue = 0.86 }, Chance = 1.0, Intensity = 0.55 },
        }
    };

    /// <summary>
    /// Flickering off-white. Same neutral palette as Bright but unstable.
    /// </summary>
    public static readonly LightSettings WhiteTheme_Flickering = new()
    {
        PersistentId = Generator.GetPersistentId(),
        CategorySettings = new List<LightCategorySetting>
        {
            new() { Category = LightCategory.General, Color = new() { Red = 0.90, Green = 0.90, Blue = 0.88 }, Chance = 0.5, ChanceBroken = 0.5, Intensity = 0.5 },
            new() { Category = LightCategory.General, Color = new() { Red = 0.86, Green = 0.88, Blue = 0.90 }, Chance = 0.5, ChanceBroken = 0.5, Intensity = 0.45 },
            new() { Category = LightCategory.Special, Color = new() { Red = 0.90, Green = 0.91, Blue = 0.90, Alpha = 0.0 }, Chance = 0.5, ChanceBroken = 0.3, Intensity = 0.55 },
            new() { Category = LightCategory.Special, Color = new() { Red = 0.94, Green = 0.94, Blue = 0.92, Alpha = 0.0 }, Chance = 0.5, ChanceBroken = 0.3, Intensity = 0.5 },
            LightCategorySetting.Off(LightCategory.Emergency),
            new() { Category = LightCategory.Independent, Color = new() { Red = 0.88, Green = 0.88, Blue = 0.86 }, Chance = 0.5, ChanceBroken = 0.5, Intensity = 0.45 },
            new() { Category = LightCategory.Door, Color = new() { Red = 0.90, Green = 0.90, Blue = 0.88 }, Chance = 0.5, ChanceBroken = 0.5, Intensity = 0.5 },
            new() { Category = LightCategory.Sign, Color = new() { Red = 0.88, Green = 0.90, Blue = 0.92 }, Chance = 0.5, ChanceBroken = 0.5, Intensity = 0.5 },
            new() { Category = LightCategory.DoorImportant, Color = new() { Red = 0.94, Green = 0.94, Blue = 0.92 }, Chance = 1.0, Intensity = 0.6 },
        }
    };

    /// <summary>
    /// Warm cream-white with a faint amber accent on the sign light. The sole "with colour"
    /// variant in the pool — kept rare so the theme reads predominantly neutral.
    /// </summary>
    public static readonly LightSettings WhiteTheme_Warm = new()
    {
        PersistentId = Generator.GetPersistentId(),
        CategorySettings = new List<LightCategorySetting>
        {
            new() { Category = LightCategory.General, Color = new() { Red = 0.92, Green = 0.90, Blue = 0.84 }, Chance = 0.5, ChanceBroken = 0.05, Intensity = 0.65 },
            new() { Category = LightCategory.General, Color = new() { Red = 0.93, Green = 0.91, Blue = 0.86 }, Chance = 0.5, ChanceBroken = 0.05, Intensity = 0.6 },
            new() { Category = LightCategory.Special, Color = new() { Red = 0.94, Green = 0.92, Blue = 0.86, Alpha = 0.0 }, Chance = 0.5, Intensity = 0.65 },
            new() { Category = LightCategory.Special, Color = new() { Red = 0.95, Green = 0.94, Blue = 0.90, Alpha = 0.0 }, Chance = 0.5, Intensity = 0.6 },
            LightCategorySetting.Off(LightCategory.Emergency),
            new() { Category = LightCategory.Independent, Color = new() { Red = 0.92, Green = 0.90, Blue = 0.84 }, Chance = 0.5, Intensity = 0.6 },
            new() { Category = LightCategory.Door, Color = new() { Red = 0.92, Green = 0.90, Blue = 0.86 }, Chance = 0.5, ChanceBroken = 0.05, Intensity = 0.65 },
            // Rare amber accent
            new() { Category = LightCategory.Sign, Color = new() { Red = 0.92, Green = 0.72, Blue = 0.40 }, Chance = 0.5, ChanceBroken = 0.05, Intensity = 0.65 },
            new() { Category = LightCategory.DoorImportant, Color = new() { Red = 0.94, Green = 0.92, Blue = 0.88 }, Chance = 1.0, Intensity = 0.7 },
        }
    };

    public static List<LightSettings> WhiteThemeLights { get; } = new()
    {
        WhiteTheme_Bright,
        WhiteTheme_Cool,
        WhiteTheme_Dim,
        WhiteTheme_Flickering,
        WhiteTheme_Warm,
    };
    #endregion

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

    #region Alarm In Progress lights

    public static readonly LightSettings LightsOff_ExceptSecurityDoor = new()
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

            LightCategorySetting.SecurityDoor_White,
        }
    };

    public static readonly LightSettings ErrorFlashOn = new()
    {
        PersistentId = Generator.GetPersistentId(),
        CategorySettings = new List<LightCategorySetting>()
        {
            // LightCategorySetting.Off(LightCategory.General),
            new()
            {
                Color = new() { Red = 1.0, Green = 0.27, Blue = 0.27 },
                Category = LightCategory.General,
                Chance = 0.1,
                Intensity = 0.9,
                On = true
            },

            LightCategorySetting.Off(LightCategory.Special),

            new()
            {
                Color = new() { Red = 1.0, Green = 0.27, Blue = 0.27 },
                Category = LightCategory.Emergency,
                Chance = 1.0,
                Intensity = 0.9,
                On = true
            },

            LightCategorySetting.Off(LightCategory.Independent),
            LightCategorySetting.Off(LightCategory.Door),
            LightCategorySetting.Off(LightCategory.Sign),

            LightCategorySetting.SecurityDoor_White,
        }
    };

    public static readonly LightSettings AlarmCycling_Amber = new()
    {
        PersistentId = Generator.GetPersistentId(),
        CategorySettings = new List<LightCategorySetting>()
        {
            new()
            {
                Color = new() { Red = 1.0, Green = 0.6, Blue = 0.15 },
                Category = LightCategory.General,
                Chance = 0.1,
                Intensity = 0.9,
                On = true
            },

            new()
            {
                Color = new() { Red = 1.0, Green = 0.6, Blue = 0.15 },
                Category = LightCategory.Special,
                Chance = 0.9,
                Intensity = 0.7,
                On = true
            },

            LightCategorySetting.Off(LightCategory.Emergency),
            LightCategorySetting.Off(LightCategory.Independent),
            LightCategorySetting.Off(LightCategory.Door),
            LightCategorySetting.Off(LightCategory.Sign),

            LightCategorySetting.SecurityDoor_White,
        }
    };

    #endregion

    public new static void SaveStatic()
    {
        AuxiliaryPower.Persist();

        LightsOff.Persist();
        LightsOff_ExceptSecurityDoor.Persist();

        ErrorFlashOn.Persist();
        AlarmCycling_Amber.Persist();

        #region Red Theme
        foreach (var light in RedThemeLights) light.Persist();
        #endregion

        #region Green Theme
        foreach (var light in GreenThemeLights) light.Persist();
        #endregion

        #region Orange Theme
        foreach (var light in OrangeThemeLights) light.Persist();
        #endregion

        #region White Theme
        foreach (var light in WhiteThemeLights) light.Persist();
        #endregion
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
