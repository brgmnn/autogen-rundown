using AutogenRundown.DataBlocks;
using BepInEx;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace AutogenRundown;

public class ComplexResourceSet
{
    private const string FileName = "GameData_ComplexResourceSetDataBlock_bin.json";

    private JObject resourceSet;

    /// <summary>
    ///
    /// </summary>
    /// <exception cref="Exception"></exception>
    public ComplexResourceSet()
    {
        var from = Path.Combine(Paths.PluginPath, Plugin.Name, FileName);

        using var sourceFile = File.OpenText(from);
        using var reader = new JsonTextReader(sourceFile);

        resourceSet = (JObject)JToken.ReadFrom(reader);

        if (resourceSet["Blocks"] == null)
        {
            Plugin.Logger.LogFatal("No complex resource set data blocks found");
            throw new Exception("No complex resource set data blocks found");
        }
    }

    /// <summary>
    ///
    /// </summary>
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

    /// <summary>
    ///
    /// </summary>
    /// <param name="persistentId"></param>
    /// <param name="group"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    public JArray GetPrefabs(int persistentId, string group)
    {
        var blocks = resourceSet["Blocks"]!.OfType<JObject>();
        var complexResource = blocks.First(block => (int?)block["persistentID"] == persistentId);

        if (complexResource[group] == null)
        {
            Plugin.Logger.LogFatal("No complex resource set data blocks found");
            throw new Exception("No complex resource set data blocks found");
        }

        return (JArray)complexResource[group]!;
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="complex"></param>
    /// <param name="group"></param>
    /// <returns></returns>
    public JArray GetPrefabs(Complex complex, string group) => GetPrefabs((int)complex, group);

    /// <summary>
    ///
    /// </summary>
    /// <param name="complex"></param>
    /// <param name="group"></param>
    /// <param name="prefab"></param>
    public void AddPrefab(Complex complex, string group, Prefab prefab)
        => GetPrefabs(complex, group).Insert(0, JObject.FromObject(prefab));

    public static void Setup()
    {
        var resourceSet = new ComplexResourceSet();

        ///
        /// Tech (Datacenter / Lab) custom geomorphs
        ///
        {
            resourceSet.AddPrefab(Complex.Tech, "CustomGeomorphs_Exit_1x1", new Prefab()
            {
                Asset = "Assets/Prefabs/Geomorph/Tech/geo_datacenter_FA_exit_01.prefab",
                SubComplex = SubComplex.DataCenter
            });

            resourceSet.AddPrefab(Complex.Tech, "ElevatorShafts_1x1", new Prefab()
            {
                Asset = "Assets/Prefabs/Geomorph/Tech/geo_datacenter_FA_elevator_shaft_01.prefab",
                SubComplex = SubComplex.DataCenter
            });
        }

        ///
        /// Service (Floodways / Gardens) custom geomorph updates
        ///
        {
            resourceSet.AddPrefab(Complex.Tech, "CustomGeomorphs_Objective_1x1", new Prefab()
            {
                Asset = "Assets/Prefabs/Geomorph/Service/geo_floodways_FA_reactor_01.prefab",
                SubComplex = SubComplex.Floodways
            });
        }

        resourceSet.WriteFile();
    }
}
