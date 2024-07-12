using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace AutogenRundown.DataBlocks
{
    public class ObjectiveLayerData
    {
        /// <summary>
        /// Local zone index where the bulkhead entrace will be
        /// </summary>
        public List<int> ZonesWithBulkheadEntrance { get; set; } = new List<int>();

        public List<BulkheadDoorPlacementData> BulkheadDoorControllerPlacements { get; set; }
            = new List<BulkheadDoorPlacementData>();

        public List<List<ZonePlacementData>> BulkheadKeyPlacements { get; set; }
            = new List<List<ZonePlacementData>>();

        public WardenObjectiveLayerData ObjectiveData { get; set; }
            = new WardenObjectiveLayerData();

        public List<WardenObjectiveLayerData> ChainedObjectiveData { get; set; }
            = new List<WardenObjectiveLayerData>();

        // We don't set artifacts
        public JObject ArtifactData = new JObject
        {
            ["ArtifactAmountMulti"] = 1.0,
            ["ArtifactLayerDistributionDataID"] = 1,
            ["ArtifactZoneDistributions"] = new JArray()
        };
    }
}
