using AutogenRundown.DataBlocks.Objectives;
using Newtonsoft.Json;

namespace AutogenRundown.DataBlocks
{
    internal class BigPickupSpawnData
    {
        [JsonProperty("itemID")]
        public WardenObjectiveItem Item { get; set; } = 0;

        public double Weight { get; set; } = 1.0;
    }
}
