using BepInEx;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace MyFirstPlugin.DataBlocks
{
    internal static class Bins
    {
        public static BlocksBin<Rundown> rundowns { get; private set; }
            = new BlocksBin<Rundown>();
        public static BlocksBin<LevelLayout> levelLayouts { get; private set; }
            = new BlocksBin<LevelLayout>();
        public static BlocksBin<WardenObjective> wardenObjectives { get; private set; }
            = new BlocksBin<WardenObjective>();

        public static void Save()
        {
            rundowns.Save("RundownDataBlock");
            levelLayouts.Save("LevelLayoutDataBlock");
            wardenObjectives.Save("WardenObjectiveDataBlock");
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

            using (StreamWriter sw = new StreamWriter(path))
            using (JsonWriter writer = new JsonTextWriter(sw))
            {
                serializer.Serialize(writer, this);
            }
        }
    }
}
