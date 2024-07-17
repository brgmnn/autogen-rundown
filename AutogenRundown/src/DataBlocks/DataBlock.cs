using Newtonsoft.Json;

namespace AutogenRundown.DataBlocks
{
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
        public string Name { get; set; }

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

        public static void Setup()
        { }

        public static void SaveStatic()
        { }
    }
}
