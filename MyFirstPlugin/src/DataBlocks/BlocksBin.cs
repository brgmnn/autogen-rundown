using BepInEx;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace MyFirstPlugin.DataBlocks
{
    internal static class Bins
    {
        public static BlocksBin<Rundown> Rundowns { get; private set; }
            = new BlocksBin<Rundown>();
        public static BlocksBin<LevelLayout> LevelLayouts { get; private set; }
            = new BlocksBin<LevelLayout>();
        public static BlocksBin<WardenObjective> WardenObjectives { get; private set; }
            = new BlocksBin<WardenObjective>();

        public static void Save()
        {
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
        public UInt32 LastPersistentId { get; set; } = 0;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="block"></param>
        public void AddBlock(T block)
        {
            Blocks.Add(block);
            LastPersistentId = Math.Max(LastPersistentId, block.PersistentId);
        }

        /// <summary>
        /// Gets a block with a given Id, otherwise null.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public T? Find(UInt32 id) => Blocks.Find(b => b.PersistentId == id);

        /// <summary>
        /// Saves the data block to disk, serializing as JSON
        /// </summary>
        /// <param name="path"></param>
        public void Save(string name)
        {
            JsonSerializer serializer = new JsonSerializer();
            serializer.Formatting = Formatting.Indented;

            //var dir = Path.Combine(Paths.PluginPath, "MyFirstPlugin", "Datablocks", Generator.Seed);

            var version = CellBuildData.GetRevision();

            var dir = Path.Combine(Paths.BepInExRootPath, "GameData", $"{version}");
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
