using BepInEx;
using Newtonsoft.Json;

namespace AutogenRundown.DataBlocks
{
    /// <summary>
    /// Base datablock type that all other data blocks inherit from
    /// </summary>
    public record class DataBlock
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
        public string Name { get; set; }

        /// <summary>
        /// Whether the data block is read by the game or not. Disabled blocks are ignored
        /// </summary>
        [JsonProperty("internalEnabled")]
        public bool Enabled { get; set; } = true;

        [JsonIgnore]
        public string? Filename { get => $"{PersistentId}__{Name?.Replace(' ', '_')}"; }

        /// <summary>
        /// Data block class constructor. Ensures a unique persistent ID is assigned
        /// </summary>
        public DataBlock(uint? id = null)
        {
            PersistentId = id ?? Generator.GetPersistentId();
            Name = PersistentId.ToString();
        }

        /// <summary>
        /// Saves the data block to disk, serializing as JSON
        /// </summary>
        /// <param name="path"></param>
        public void Save()
        {
            JsonSerializer serializer = new JsonSerializer();
            serializer.Formatting = Formatting.Indented;

            var revision = CellBuildData.GetRevision();

            var dir = Path.Combine(Paths.BepInExRootPath, "GameData", $"{revision}", "Custom");
            var path = Path.Combine(dir, $"GameData__{Filename}.json");

            // Ensure the directory exists
            Directory.CreateDirectory(dir);

            using (StreamWriter sw = new StreamWriter(path))
            using (JsonWriter writer = new JsonTextWriter(sw))
            {
                serializer.Serialize(writer, this);
            }
        }

        public static void Setup()
        { }

        public static void SaveStatic()
        { }
    }
}
