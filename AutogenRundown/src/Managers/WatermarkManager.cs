namespace AutogenRundown.Managers;

public class WatermarkManager
{
    private const string EmptySeed = "<color=#444444>-</color>";

    private static PUI_Watermark? puiWatermark = null;

    private static string displayText { get; set; } = $"<size=14>{EmptySeed}\nAR v{Plugin.Version}</size>";

    public static void SetInstance(PUI_Watermark instance)
    {
        puiWatermark = instance;
        puiWatermark.m_watermarkText.SetText(displayText);
    }

    public static void SetWatermark(string text)
    {
        displayText = $"<size=14>{text}\nAR v{Plugin.Version}</size>";

        puiWatermark?.m_watermarkText.SetText(displayText);
    }

    public static void ClearRundown()
    {
        SetWatermark(EmptySeed);
    }

    public static void SetRundown(PluginRundown rundown, pActiveExpedition data)
    {
        var tier = data.tier switch
        {
            eRundownTier.TierA => "A",
            eRundownTier.TierB => "B",
            eRundownTier.TierC => "C",
            eRundownTier.TierD => "D",
            eRundownTier.TierE => "E",
            _ => ""
        };
        var level = $"{tier}{data.expeditionIndex + 1}";

        SetWatermark(rundown switch
        {
            PluginRundown.Daily    => $"<color=#6fff70>{level}</color> Daily <color=orange>{Generator.InputDailySeed}</color>",
            PluginRundown.Weekly   => $"<color=#6fff70>{level}</color> Weekly <color=orange>{Generator.InputWeeklySeed}</color>",
            PluginRundown.Monthly  => $"<color=#6fff70>{level}</color> Monthly <color=orange>{Generator.InputMonthlySeed}</color>",
            PluginRundown.Seasonal => $"<color=#6fff70>{level}</color> Season <color=orange>{Generator.SeasonalSeason} {Generator.SeasonalYear}</color>",
            _ => EmptySeed
        });
    }
}
