using UnityEngine;

namespace AutogenRundown.Managers;

public class RundownSelection
{

    // public static readonly Color MenuVisuals_MonthlyE = new()
    // {
    //     Alpha = 1.0,
    //     Red = 0.1509804,
    //     Green = 0.4190196,
    //     Blue = 0.858823538,
    // };
    //
    // public static readonly Color MenuVisuals_WeeklyE = new()
    // {
    //     Alpha = 1.0,
    //     Red = 0.158823538,
    //     Green = 0.8509804,
    //     Blue = 0.4190196,
    // };
    //
    // public static readonly Color MenuVisuals_DailyE = new()
    // {
    //     Alpha = 1.0,
    //     Red = 0.8509804,
    //     Green = 0.4190196,
    //     Blue = 0.158823538,
    // };



    // Daily
    public static readonly RundownSelection R1 = new()
    {
        Enabled = true,
        UnityName = "Rundown_Surface_SelectionALT_R1",
        RadarColor = new Color { r = 0.8509804f, g = 0.4190196f, b = 0.158823538f, a = 1.0f }
    };

    // Weekly
    public static readonly RundownSelection R2 = new()
    {
        Enabled = true,
        UnityName = "Rundown_Surface_SelectionALT_R2",
        RadarColor = new Color { r = 0.158823538f, g = 0.8509804f, b = 0.4190196f, a = 1.0f }
    };

    public static readonly RundownSelection R3 = new()
    {
        Enabled = false,
        UnityName = "Rundown_Surface_SelectionALT_R3"
    };

    public static readonly RundownSelection R4 = new()
    {
        Enabled = false,
        UnityName = "Rundown_Surface_SelectionALT_R4"
    };

    public static readonly RundownSelection R5 = new()
    {
        Enabled = false,
        UnityName = "Rundown_Surface_SelectionALT_R5"
    };

    public static readonly RundownSelection R6 = new()
    {
        Enabled = false,
        UnityName = "Rundown_Surface_SelectionALT_R6"
    };

    // Monthly
    public static readonly RundownSelection R7 = new()
    {
        Enabled = true,
        UnityName = "Rundown_Surface_SelectionALT_R7",
        RadarColor = new Color { r = 0.158823538f, g = 0.4190196f, b = 0.8509804f, a = 1.0f }
    };

    // Seasonal
    public static readonly RundownSelection R8 = new()
    {
        Enabled = true,
        UnityName = "Rundown_Surface_SelectionALT_R8",
        RadarColor = new Color { r = 0.9509804f, g = 0.158823538f, b = 0.158823538f, a = 1.0f }
    };

    public bool Enabled { get; set; } = false;

    public string UnityName { get; set; } = "";

    public Color? RadarColor { get; set; } = null;
}
