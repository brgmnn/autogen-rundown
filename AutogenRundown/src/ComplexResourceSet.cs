using AutogenRundown.DataBlocks;
using BepInEx;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace AutogenRundown;

public class ComplexResourceSet
{
    private const string FileName = "GameData_ComplexResourceSetDataBlock_bin.json";

    private JObject resourceSet;

    public ComplexResourceSet()
    {
        //var revision = CellBuildData.GetRevision();

        var from = Path.Combine(Paths.PluginPath, Plugin.Name, FileName);
        //var destDir = Path.Combine(Paths.BepInExRootPath, "GameData", $"{revision}");
        //var dest = Path.Combine(destDir, FileName);

        using var sourceFile = File.OpenText(from);
        using var reader = new JsonTextReader(sourceFile);

        resourceSet = (JObject)JToken.ReadFrom(reader);

        if (resourceSet["Blocks"] == null)
            Plugin.Logger.LogFatal("No complex resource set data blocks found");

        // var blocks = (JArray)resourceSet["Blocks"]!;
    }

    private void WriteFile()
    {
        var revision = CellBuildData.GetRevision();

        var from = Path.Combine(Paths.PluginPath, Plugin.Name, FileName);
        var destDir = Path.Combine(Paths.BepInExRootPath, "GameData", $"{revision}");
        var dest = Path.Combine(destDir, FileName);

        // Ensure the directory exists
        Directory.CreateDirectory(destDir);

        // write JSON directly to a file
        using var destFile = File.CreateText(dest);
        using var writer = new JsonTextWriter(destFile);

        resourceSet.WriteTo(writer);
    }

    public JArray GetGeos(int persistentId, string geos)
    {
        var blocks = resourceSet["Blocks"].OfType<JObject>();

        if (blocks == null)
        {
            Plugin.Logger.LogFatal("No complex resource set data blocks found");
        }

        var block = blocks.First(block)
    }

    public static void Setup()
    {
        var resourceSet2 = new ComplexResourceSet();

        resourceSet2.WriteFile();

        const string name = "GameData_ComplexResourceSetDataBlock_bin.json";

        var revision = CellBuildData.GetRevision();

        var from = Path.Combine(Paths.PluginPath, Plugin.Name, name);
        var destDir = Path.Combine(Paths.BepInExRootPath, "GameData", $"{revision}");
        var dest = Path.Combine(destDir, name);

        using var sourceFile = File.OpenText(from);
        using var reader = new JsonTextReader(sourceFile);

        var resourceSet = (JObject)JToken.ReadFrom(reader);

        if (resourceSet["Blocks"] == null)
        {
            Plugin.Logger.LogFatal("No complex resource set data blocks found");
            return;
        }

        var blocks = (JArray)resourceSet["Blocks"]!;

        ///
        /// Tech (Datacenter / Lab) custom geomorphs
        ///
        {
            var techBlock = blocks.OfType<JObject>()
            .First(block => (int?)block["persistentID"] == (int)Complex.Tech);

            if (techBlock?["CustomGeomorphs_Objective_1x1"] == null)
            {
                Plugin.Logger.LogFatal("No Complex.Service resource block found");
                return;
            }

            var exitGeos = (JArray)techBlock["CustomGeomorphs_Exit_1x1"]!;
            exitGeos.Insert(0,
                new JObject
                {
                    ["Prefab"] = "Assets/Prefabs/Geomorph/Tech/geo_datacenter_FA_exit_01.prefab",
                    ["SubComplex"] = (int)SubComplex.DataCenter,
                    ["Shard"] = "S1"
                });

            var elevatorGeos = (JArray)techBlock["ElevatorShafts_1x1"]!;
            elevatorGeos.Insert(0,
                new JObject
                {
                    ["Prefab"] = "Assets/Prefabs/Geomorph/Tech/geo_datacenter_FA_elevator_shaft_01.prefab",
                    ["SubComplex"] = (int)SubComplex.DataCenter,
                    ["Shard"] = "S1"
                });
        }

        ///
        /// Service (Floodways / Gardens) custom geomorph updates
        ///
        {
            var serviceBlock = blocks.OfType<JObject>()
            .First(block => (int?)block["persistentID"] == (int)Complex.Service);

            if (serviceBlock?["CustomGeomorphs_Objective_1x1"] == null)
            {
                Plugin.Logger.LogFatal("No Complex.Service resource block found");
                return;
            }

            var objectiveGeos = (JArray)serviceBlock["CustomGeomorphs_Objective_1x1"]!;

            objectiveGeos.Insert(0,
                new JObject
                {
                    ["Prefab"] = "Assets/Prefabs/Geomorph/Service/geo_floodways_FA_reactor_01.prefab",
                    ["SubComplex"] = (int)SubComplex.Floodways,
                    ["Shard"] = "S1"
                });
        }

        // Ensure the directory exists
        Directory.CreateDirectory(destDir);

        // write JSON directly to a file
        using var destFile = File.CreateText(dest);
        using var writer = new JsonTextWriter(destFile);

        resourceSet.WriteTo(writer);
    }


}
