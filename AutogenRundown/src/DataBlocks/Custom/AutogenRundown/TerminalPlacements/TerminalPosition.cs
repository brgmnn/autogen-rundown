using AutogenRundown.DataBlocks.Objectives;

namespace AutogenRundown.DataBlocks.Custom.AutogenRundown.TerminalPlacements;

public record TerminalPosition
{
    public Bulkhead Bulkhead { get; set; } = Bulkhead.Main;

    public string Layer => Bulkhead switch
    {
        Bulkhead.Main => "MainLayer",
        Bulkhead.Extreme => "SecondaryLayer",
        Bulkhead.Overload => "ThirdLayer",
        _ => "MainLayer"
    };

    public int LocalIndex { get; set; } = 0;

    public string Geomorph { get; set; } = "";

    public Vector3 Position { get; set; } = new();

    public Vector3 Rotation { get; set; } = new();

    /// <summary>
    /// We want to check this kind of geomorph string:
    ///     geo_64x64_tech_lab_hub_HA_01_V2(Clone)_ID_22_22
    ///
    /// And see if it matches a geomorph asset prefab string like this:
    ///     Assets/AssetPrefabs/Complex/Tech/Geomorphs/geo_64x64_tech_lab_hub_HA_01_V2.prefab
    /// </summary>
    /// <param name="geo"></param>
    /// <returns></returns>
    public bool HasGeomorphName(string name)
        => name.StartsWith(Path.GetFileNameWithoutExtension(Geomorph));
};
