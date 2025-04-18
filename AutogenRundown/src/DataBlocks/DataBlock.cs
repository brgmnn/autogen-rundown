﻿using BepInEx;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace AutogenRundown.DataBlocks;

/// <summary>
/// Base datablock type that all other data blocks inherit from
/// </summary>
public record DataBlock
{
    /// <summary>
    /// All persistent Ids must be unique
    /// </summary>
    [JsonProperty("persistentID")]
    public uint PersistentId { get; set; }

    /// <summary>
    /// Optional name, useful mostly for debugging
    /// </summary>
    [JsonProperty("name")]
    public string BlockName
    {
        get => $"{PersistentId}_{Name}";
        private set { }
    }

    /// <summary>
    /// This gets mapped to the data block name, collisions are avoided by concatenating the persistent Id with this
    /// </summary>
    [JsonIgnore]
    public string Name { get; set; }

    /// <summary>
    /// Mainly so we can debug blocks easier
    /// </summary>
    [JsonProperty("_comment", NullValueHandling = NullValueHandling.Ignore)]
    public string? Comment { get; set; }

    /// <summary>
    /// Whether the data block is read by the game or not. Disabled blocks are ignored
    /// </summary>
    [JsonProperty("internalEnabled")]
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// Data block class constructor. Ensures a unique persistent ID is assigned
    /// </summary>
    public DataBlock(uint? id = null)
    {
        PersistentId = id ?? Generator.GetPersistentId();
        Name = PersistentId.ToString();
    }

    /// <summary>
    /// Generic function for setting up a BlockBin with the vanilla game data. Can be
    /// called like so:
    /// ```cs
    ///     Setup<GameDataWaveSettings, WaveSettings>(Bins.WaveSettings, "SurvivalWaveSettings");
    /// ```
    /// </summary>
    /// <param name="bin"></param>
    /// <param name="filename"></param>
    /// <typeparam name="TGameData"></typeparam>
    /// <typeparam name="TBlock"></typeparam>
    /// <exception cref="Exception"></exception>
    public static void Setup<TGameData, TBlock>(BlocksBin<TBlock> bin, string filename)
        where TGameData : TBlock
        where TBlock : DataBlock
    {
        var dir = Path.Combine(Paths.PluginPath, Plugin.Name);
        var path = Path.Combine(dir, $"GameData_{filename}DataBlock_bin.json");
        var data = JObject.Parse(File.ReadAllText(path));

        if (data?["Blocks"] == null)
            throw new Exception("Failed to get 'Blocks' property");

        var blocks = data["Blocks"]!.ToObject<List<TGameData>>();

        if (blocks == null)
            throw new Exception($"Failed to parse {filename}");

        foreach (var block in blocks)
            bin.AddBlock(block);
    }

    public static void SaveStatic()
    { }
}
