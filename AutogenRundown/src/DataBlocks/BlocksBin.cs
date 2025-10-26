using AutogenRundown.DataBlocks.Alarms;
using AutogenRundown.DataBlocks.Enemies;
using AutogenRundown.DataBlocks.Markers;
using BepInEx;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace AutogenRundown.DataBlocks;

public static class Bins
{
    public static BlocksBin<BigPickupDistribution> BigPickupDistributions { get; private set; }
        = new BlocksBin<BigPickupDistribution>();
    public static LazyBlocksBin<ChainedPuzzle> ChainedPuzzles { get; private set; }
        = new LazyBlocksBin<ChainedPuzzle>();
    public static BlocksBin<ComplexResourceSet> ComplexResources { get; private set; }
        = new BlocksBin<ComplexResourceSet>();
    public static BlocksBin<ConsumableDistribution> ConsumableDistributions { get; private set; }
        = new BlocksBin<ConsumableDistribution>();
    public static BlocksBin<Dimension> Dimensions { get; private set; }
        = new BlocksBin<Dimension>();
    public static BlocksBin<Enemy_New> Enemy { get; private set; }
        = new BlocksBin<Enemy_New>();
    public static BlocksBin<EnemyBalancing> EnemyBalancing { get; private set; }
        = new BlocksBin<EnemyBalancing>();
    public static BlocksBin<EnemyGroup> EnemyGroups { get; private set; }
        = new BlocksBin<EnemyGroup>();
    public static BlocksBin<EnemyMovement> EnemyMovements { get; private set; }
        = new BlocksBin<EnemyMovement>();
    public static BlocksBin<EnemyPopulation> EnemyPopulations { get; private set; }
        = new BlocksBin<EnemyPopulation>();
    public static BlocksBin<EnemySFX> EnemySFXs { get; private set; }
        = new BlocksBin<EnemySFX>();
    public static BlocksBin<ExpeditionBalance> ExpeditionBalances { get; private set; }
        = new BlocksBin<ExpeditionBalance>();
    public static BlocksBin<Fog> Fogs { get; private set; }
        = new BlocksBin<Fog>();
    public static BlocksBin<GameSetup> GameSetups { get; private set; }
        = new BlocksBin<GameSetup>();
    public static BlocksBin<LevelLayout> LevelLayouts { get; private set; }
        = new BlocksBin<LevelLayout>();

    public static LazyBlocksBin<Light.LightSettings> LightSettings { get; private set; } = new();
    public static BlocksBin<MiningMarker> MiningMarkers { get; private set; }
        = new BlocksBin<MiningMarker>();
    public static BlocksBin<Rundown> Rundowns { get; private set; }
        = new BlocksBin<Rundown>();
    public static BlocksBin<Text> Texts { get; private set; }
        = new BlocksBin<Text>();
    public static BlocksBin<WardenObjective> WardenObjectives { get; private set; }
        = new BlocksBin<WardenObjective>();
    public static BlocksBin<WavePopulation> WavePopulations { get; private set; }
        = new BlocksBin<WavePopulation>();
    public static LazyBlocksBin<WaveSettings> WaveSettings { get; private set; } = new();
    public static BlocksBin<GlobalWaveSettings> GlobalWaveSettings { get; private set; }
        = new BlocksBin<GlobalWaveSettings>();

    /// <summary>
    /// Primarily used to load and set up vanilla data within custom data blocks.
    /// </summary>
    public static void Setup()
    {
        ComplexResourceSet.Setup();
        MiningMarker.Setup();
        Light.LightSettings.Setup();
        Light.LightSettings.SaveStatic();
        Text.Setup();
        Text.SaveStatic();

        Enemies.EnemyBalancing.Setup();
        EnemyMovement.Setup();
        EnemySFX.Setup();
        Enemy_New.Setup();
        EnemyGroup.Setup();
        EnemyPopulation.Setup();
        WavePopulation.Setup();
        Alarms.WaveSettings.Setup();

        ComplexResourceSet.SaveStatic();
        MiningMarker.SaveStatic();

        BigPickupDistribution.SaveStatic();
        ChainedPuzzle.SaveStatic();
        ConsumableDistribution.SaveStatic();
        Dimension.SaveStatic();
        EnemyGroup.SaveStatic();
        EnemyPopulation.SaveStatic();
        ExpeditionBalance.SaveStatic();
        Fog.SaveStatic();
        LevelLayout.SaveStatic();
        Rundown.SaveStatic();
        WardenObjective.SaveStatic();
        WavePopulation.SaveStatic();
        Alarms.WaveSettings.SaveStatic();

        // Mods
        Alarms.GlobalWaveSettings.SaveStatic();
    }

    /// <summary>
    /// Saves all data blocks to disk. Also any static data blocks are ususally written here.
    /// </summary>
    public static void Save()
    {
        ComplexResources.Save("ComplexResourceSet");
        MiningMarkers.Save("MiningMarkers");

        BigPickupDistributions.Save("BigPickupDistribution");
        ChainedPuzzles.Save("ChainedPuzzle");
        ConsumableDistributions.Save("ConsumableDistribution");
        Dimensions.Save("Dimension");
        Enemy.Save("Enemy");
        EnemyBalancing.Save("EnemyBalancing");
        EnemyGroups.Save("EnemyGroup");
        EnemyMovements.Save("EnemyMovement");
        EnemyPopulations.Save("EnemyPopulation");
        EnemySFXs.Save("EnemySFX");
        ExpeditionBalances.Save("ExpeditionBalance");
        Fogs.Save("FogSettings");
        GameSetups.Save("GameSetup");
        LevelLayouts.Save("LevelLayout");
        LightSettings.Save("LightSettings");
        Rundowns.Save("Rundown");
        Texts.Save("Text");
        WardenObjectives.Save("WardenObjective");
        WavePopulations.Save("SurvivalWavePopulation");
        WaveSettings.Save("SurvivalWaveSettings");

        // Mods
        GlobalWaveSettings.Save(
            Path.Combine(
                Paths.BepInExRootPath,
                "plugins",
                "PersistentData",
                "ConfigurableGlobalWaveSettings"),
            "GlobalWaveSettingsDataBlock.json");
    }
}

public class BlocksBin<T> where T : DataBlock
{
    /// <summary>
    /// Not sure what this is for yet
    /// </summary>
    public List<JObject> Headers { get; set; } = new();

    public List<T> Blocks { get; set; } = new();

    [JsonProperty("LastPersistentID")]
    public uint LastPersistentId { get; set; } = 0;

    internal HashSet<uint> persistentIds = new();

    /// <summary>
    ///
    /// </summary>
    /// <param name="block"></param>
    public void AddBlock(T? block)
    {
        if (block is null)
            return;

        // Only add blocks that haven't been seen and aren't persistentId = 0 which signifies
        // an empty block.
        if (!persistentIds.Contains(block.PersistentId) && block.PersistentId != 0)
        {
            Blocks.Add(block);
            persistentIds.Add(block.PersistentId);
            LastPersistentId = Math.Max(LastPersistentId, block.PersistentId);
        }
    }

    public void ReplaceBlock(T? block)
    {
        if (block is null || block.PersistentId == 0)
            return;

        if (persistentIds.Contains(block.PersistentId))
        {
            var existing = Blocks.Find(b => b.PersistentId == block.PersistentId);

            if (existing is not null)
                Blocks.Remove(existing);
        }

        Blocks.Add(block);
        persistentIds.Add(block.PersistentId);
        LastPersistentId = Math.Max(LastPersistentId, block.PersistentId);
    }

    public bool Contains(T? block) => block is not null && Blocks.Contains(block);

    public T? GetBlock(T? block) => Blocks.Find(b => b == block);

    public T? GetBlock(Predicate<T> predicate) => Blocks.Find(predicate);

    /// <summary>
    /// Gets a block with a given Id, otherwise null.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public T? Find(uint id) => Blocks.Find(b => b.PersistentId == id);

    /// <summary>
    /// Saves the data block to disk, serializing as JSON
    /// </summary>
    /// <param name="name"></param>
    public void Save(string name)
    {
        JsonSerializer serializer = new JsonSerializer();
        serializer.Formatting = Formatting.Indented;

        // Replace with Plugin.GameRevision to avoid interop dependency
        var revision = CellBuildData.GetRevision();

        var dir = Path.Combine(Paths.BepInExRootPath, "GameData", $"{revision}");
        var path = Path.Combine(dir, $"GameData_{name}DataBlock_bin.json");

        // Ensure the directory exists
        Directory.CreateDirectory(dir);

        using StreamWriter stream = new StreamWriter(path);
        using JsonWriter writer = new JsonTextWriter(stream);

        serializer.Serialize(writer, this);
        stream.Flush();
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="dir"></param>
    /// <param name="filename"></param>
    public void Save(string dir, string filename)
    {
        JsonSerializer serializer = new JsonSerializer();
        serializer.Formatting = Formatting.Indented;

        var path = Path.Combine(dir, filename);

        // Ensure the directory exists
        Directory.CreateDirectory(dir);

        using StreamWriter stream = new StreamWriter(path);
        using JsonWriter writer = new JsonTextWriter(stream);

        serializer.Serialize(writer, this);
        stream.Flush();
    }
}

/// <summary>
/// Lazy persisting version of BlocksBin.
/// </summary>
/// <typeparam name="T"></typeparam>
public class LazyBlocksBin<T> : BlocksBin<T> where T : DataBlock
{
    /// <summary>
    ///
    /// </summary>
    /// <param name="block"></param>
    public new void AddBlock(T? block)
    {
        if (block is null)
            return;

        // Don't add a block that we already have
        // TODO: do we need to change this to use a HashMap?
        if (Blocks.Contains(block))
        {
            Plugin.Logger.LogWarning($"Already got ChainedPuzzle: {block}");
            return;
        }

        Blocks.Add(block);

        if (block.PersistentId == 0)
            return;

        persistentIds.Add(block.PersistentId);
        LastPersistentId = Math.Max(LastPersistentId, block.PersistentId);
    }

    /// <summary>
    /// Ensure all blocks are assigned an Id
    /// </summary>
    public void Persist()
    {
        foreach (var block in Blocks.Where(block => block.PersistentId == 0))
        {
            block.PersistentId = Generator.GetPersistentId();
            persistentIds.Add(block.PersistentId);
            LastPersistentId = Math.Max(LastPersistentId, block.PersistentId);
        }
    }

    /// <summary>
    /// Saves the data block to disk, serializing as JSON
    /// </summary>
    /// <param name="name"></param>
    public new void Save(string name)
    {
        // Ensure all blocks are assign persistent Id's
        Persist();

        // Call base save
        base.Save(name);
    }
}
