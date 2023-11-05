using AutogenRundown.DataBlocks.Alarms;
using AutogenRundown.DataBlocks.Enemies;
using BepInEx;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace AutogenRundown.DataBlocks
{
    internal static class Bins
    {
        public static BlocksBin<BigPickupDistribution> BigPickupDistributions { get; private set; }
            = new BlocksBin<BigPickupDistribution>();
        public static BlocksBin<ChainedPuzzle> ChainedPuzzles { get; private set; }
            = new BlocksBin<ChainedPuzzle>();
        public static BlocksBin<EnemyGroup> EnemyGroups { get; private set; }
            = new BlocksBin<EnemyGroup>();
        public static BlocksBin<EnemyPopulation> EnemyPopulations { get; private set; }
            = new BlocksBin<EnemyPopulation>();
        public static BlocksBin<Fog> Fogs { get; private set; }
            = new BlocksBin<Fog>();
        public static BlocksBin<LevelLayout> LevelLayouts { get; private set; }
            = new BlocksBin<LevelLayout>();
        public static BlocksBin<Rundown> Rundowns { get; private set; }
            = new BlocksBin<Rundown>();
        public static BlocksBin<WardenObjective> WardenObjectives { get; private set; }
            = new BlocksBin<WardenObjective>();
        public static BlocksBin<WavePopulation> WavePopulations { get; private set; }
            = new BlocksBin<WavePopulation>();
        public static BlocksBin<WaveSettings> WaveSettings { get; private set; }
            = new BlocksBin<WaveSettings>();

        /// <summary>
        /// Primarily used to load and set up vanilla data within custom data blocks.
        /// </summary>
        public static void Setup()
        {
            EnemyGroup.Setup();
            EnemyPopulation.Setup();
            WavePopulation.Setup();
            Alarms.WaveSettings.Setup();
        }

        /// <summary>
        /// Saves all data blocks to disk. Also any static data blocks are ususally written here.
        /// </summary>
        public static void Save()
        {
            BigPickupDistribution.SaveStatic();
            ChainedPuzzle.SaveStatic();
            EnemyGroup.SaveStatic();
            EnemyPopulation.SaveStatic();
            Fog.SaveStatic();
            LevelLayout.SaveStatic();
            Rundown.SaveStatic();
            WardenObjective.SaveStatic();
            WavePopulation.SaveStatic();
            Alarms.WaveSettings.SaveStatic();

            BigPickupDistributions.Save("BigPickupDistributionDataBlock");
            ChainedPuzzles.Save("ChainedPuzzleDataBlock");
            EnemyGroups.Save("EnemyGroupDataBlock");
            EnemyPopulations.Save("EnemyPopulationDataBlock");
            Fogs.Save("FogSettingsDataBlock");
            LevelLayouts.Save("LevelLayoutDataBlock");
            Rundowns.Save("RundownDataBlock");
            WardenObjectives.Save("WardenObjectiveDataBlock");
            WavePopulations.Save("SurvivalWavePopulationDataBlock");
            WaveSettings.Save("SurvivalWaveSettingsDataBlock");
        }
    }

    internal class BlocksBin<T> where T : DataBlock
    {
        /// <summary>
        /// Not sure what this is for yet
        /// </summary>
        public List<JObject> Headers { get; set; } = new List<JObject>();

        public List<T> Blocks { get; set; } = new List<T> { };

        [JsonProperty("LastPersistentID")]
        public uint LastPersistentId { get; set; } = 0;

        private HashSet<uint> persistentIds = new HashSet<uint>();

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

        /// <summary>
        /// Gets a block with a given Id, otherwise null.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public T? Find(uint id) => Blocks.Find(b => b.PersistentId == id);

        /// <summary>
        /// Saves the data block to disk, serializing as JSON
        /// </summary>
        /// <param name="path"></param>
        public void Save(string name)
        {
            JsonSerializer serializer = new JsonSerializer();
            serializer.Formatting = Formatting.Indented;

            // Replace with Plugin.GameRevision to avoid interop dependency
            var revision = CellBuildData.GetRevision();

            var dir = Path.Combine(Paths.BepInExRootPath, "GameData", $"{revision}");
            var path = Path.Combine(dir, $"GameData_{name}_bin.json");

            // Ensure the directory exists
            Directory.CreateDirectory(dir);

            using StreamWriter stream = new StreamWriter(path);
            using JsonWriter writer = new JsonTextWriter(stream);

            serializer.Serialize(writer, this);
            stream.Flush();
        }
    }
}
