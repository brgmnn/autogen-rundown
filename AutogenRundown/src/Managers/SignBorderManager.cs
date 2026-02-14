using AutogenRundown.Components;
using UnityEngine;

namespace AutogenRundown.Managers;

public static class SignBorderManager
{
    private static readonly Dictionary<int, Color> borderColors = new();
    private static readonly List<SignBorder> borders = new();

    public static void SetBorderColor(int zoneNumber, Color color)
    {
        borderColors[zoneNumber] = color;

        foreach (var border in borders)
        {
            if (border.ZoneNumber == zoneNumber)
            {
                border.SetColor(color);
                border.SetVisible(true);
            }
        }
    }

    public static void RemoveBorderColor(int zoneNumber)
    {
        borderColors.Remove(zoneNumber);

        foreach (var border in borders)
        {
            if (border.ZoneNumber == zoneNumber)
                border.SetVisible(false);
        }
    }

    public static Color? GetBorderColor(int zoneNumber)
    {
        return borderColors.TryGetValue(zoneNumber, out var color) ? color : null;
    }

    internal static void Register(SignBorder border)
    {
        borders.Add(border);
    }

    internal static void Unregister(SignBorder border)
    {
        borders.Remove(border);
    }

    public static void Clear()
    {
        borderColors.Clear();
        borders.Clear();
    }
}
