namespace AutogenRundown.Managers;

public class WatermarkManager
{
    private static PUI_Watermark? puiWatermark = null;

    private static string displayText { get; set; } = $"<size=14>\nAR v{Plugin.Version}</size>";

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

    public static void SetRundown(PluginRundown rundown)
    {
        SetWatermark(rundown switch
        {
            PluginRundown.Daily => $"Daily <color=orange>{Generator.DailySeed}</color>",
            PluginRundown.Weekly => $"Weekly <color=orange>{Generator.WeeklySeed}</color>",
            PluginRundown.Monthly => $"Monthly <color=orange>{Generator.MonthlySeed}</color>",
            _ => "-"
        });
    }
}
