using BepInEx;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Text;
using GameData;

using AutogenRundown.Json;
using AutogenRundown.src.Json;
using Globals;

namespace AutogenRundown.Loaders
{
    using DBBase = GameDataBlockBase<EnemyDataBlock>;

    public delegate void GameDataContentLoadEvent(string datablockName, string jsonContent, in List<string> jsonItemsToInject);
    public delegate void GameDataContentLoadedDelegate(string datablockName, string jsonContent);


    internal class GameDataLoader
    {
        public static void Load()
        {
            var dir = Path.Combine(Paths.PluginPath, "MyFirstPlugin", "Datablocks", Generator.Seed);

            if (Directory.Exists(dir))
            {
                int count = 0;

                foreach (var filePath in Directory.GetFiles(dir, "*.json"))
                {
                    var filename = Path.GetFileName(filePath);
                    var parts = filename.Split("__");

                    var type = parts[0];
                    var pid = parts[1];
                    var name = parts[2];

                    // Skip invalid files or non-existant files
                    if (type == "None" || !File.Exists(filePath))
                    {
                        continue;
                    }

                    Plugin.Logger.LogInfo($"=== Trying to add block {filename}");

                    var settings = new JsonSerializerSettings();
                    settings.Converters.Add(new UInt32Converter());
                    settings.Converters.Add(new LocalizedTextConverter());
                    settings.Converters.Add(new Il2CppListConverter<JObject>());
                    settings.Converters.Add(new Il2CppListConverter<ExpeditionInTierData>());
                    settings.Converters.Add(new Il2CppListConverter<DimensionInExpeditionData>());
                    settings.Converters.Add(new Il2CppListConverter<ExpeditionZoneData>());
                    settings.Converters.Add(new Il2CppListConverter<WardenObjectiveEventData>());
                    settings.Converters.Add(new Il2CppListConverter<LevelEventData>());
                    settings.Converters.Add(new Il2CppListConverter<TerminalPlacementData>());
                    settings.Converters.Add(new Il2CppListConverter<FunctionPlacementData>());
                    settings.Converters.Add(new Il2CppListConverter<DumbwaiterPlacementData>());
                    settings.Converters.Add(new Il2CppListConverter<StaticSpawnDataContainer>());
                    settings.Converters.Add(new Il2CppListConverter<ZonePlacementData>());
                    settings.Converters.Add(new Il2CppListConverter<UInt32>());

                    using (var reader = new StreamReader(filePath, Encoding.UTF8))
                    {
                        var json = reader.ReadToEnd();

                        switch (type)
                        {
                            case "RundownDataBlock":
                                {
                                    var block = JsonConvert.DeserializeObject<RundownDataBlock>(json, settings);
                                    RundownDataBlock.AddBlock(block);
                                    break;
                                }

                            case "LevelLayoutDataBlock":
                                {
                                    var block = JsonConvert.DeserializeObject<LevelLayoutDataBlock>(json, settings);
                                    LevelLayoutDataBlock.AddBlock(block);
                                    break;
                                }
                        }

                        count++;
                    }
                }

                Plugin.Logger.LogInfo($" - Added {count} partial data of ");
            }

            //RundownDataBlock.Move(31, 31000);
            //RundownDataBlock.Move(1, 31);

            Plugin.Logger.LogInfo("=== Rundown Loaded ===");

            //Global.RundownIdToLoad = 1;
        }
    }
}
