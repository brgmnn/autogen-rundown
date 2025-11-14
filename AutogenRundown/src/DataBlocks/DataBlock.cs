using BepInEx;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace AutogenRundown.DataBlocks;

/// <summary>
/// Base datablock type that all other data blocks inherit from
/// </summary>
public record DataBlock<T> where T : DataBlock<T>
{
    [JsonIgnore]
    public static Dictionary<uint, T> GtfoBlocks { get; } = new();

    [JsonIgnore]
    public static Dictionary<string, List<uint>> GtfoBlocksLookup { get; } = new();


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
    /// <param name="callback"></param>
    /// <typeparam name="TGameData"></typeparam>
    /// <typeparam name="TBlock"></typeparam>
    /// <exception cref="Exception"></exception>
    protected static void Setup<TGameData>(
        BlocksBin<T> bin,
        string filename,
        Action<T>? callback = null)
        where TGameData : T
        // where T : DataBlock<TBlock>
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
        {
            bin.AddBlock(block);

            GtfoBlocks[block.PersistentId] = block;

            callback?.Invoke(block);
        }

        Plugin.Logger.LogDebug($"Registered base GTFO DataBlocks for {filename} = {GtfoBlocks.Count}");
    }

    public static void SaveStatic()
    { }
}
