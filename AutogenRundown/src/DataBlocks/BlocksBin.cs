using AutogenRundown.DataBlocks.Alarms;
using BepInEx;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace AutogenRundown.DataBlocks
{
    internal static class Bins
    {
        public static BlocksBin<ChainedPuzzle> ChainedPuzzles { get; private set; }
            = new BlocksBin<ChainedPuzzle>();
        public static BlocksBin<Rundown> Rundowns { get; private set; }
            = new BlocksBin<Rundown>();
        public static BlocksBin<LevelLayout> LevelLayouts { get; private set; }
            = new BlocksBin<LevelLayout>();
        public static BlocksBin<WardenObjective> WardenObjectives { get; private set; }
            = new BlocksBin<WardenObjective>();

        public static void Save()
        {
            ChainedPuzzle.SaveStatic();
            Rundown.SaveStatic();
            LevelLayout.SaveStatic();
            WardenObjective.SaveStatic();

            ChainedPuzzles.Save("ChainedPuzzleDataBlock");
            Rundowns.Save("RundownDataBlock");
            LevelLayouts.Save("LevelLayoutDataBlock");
            WardenObjectives.Save("WardenObjectiveDataBlock");
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
        public void AddBlock(T block)
        {
            if (!persistentIds.Contains(block.PersistentId))
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
