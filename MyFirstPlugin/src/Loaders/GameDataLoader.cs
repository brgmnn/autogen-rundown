using BepInEx;
using BepInEx.Unity.IL2CPP;
using DropServer;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Il2CppInterop.Runtime;
using MyFirstPlugin.DataBlocks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using UniverseLib.Config;

using Il2CppInterop.Runtime.Runtime;
using BepInEx.Unity.IL2CPP.Hook;
using GTFO.API;
using Il2CppInterop.Runtime.Injection;
using GameData;
using CellMenu;
using HarmonyLib;
using UnityEngine.Diagnostics;
using Newtonsoft.Json.Serialization;
using System.Reflection;
using Localization;

using MyFirstPlugin.Json;
using MyFirstPlugin.src.Json;
using Globals;

namespace MyFirstPlugin.Loaders
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
                    settings.Converters.Add(new Il2CppListConverter<UInt32>());

                    using (var reader = new StreamReader(filePath, Encoding.UTF8))
                    {
                        var json = reader.ReadToEnd();

                        switch (type)
                        {
                            case "RundownDataBlock":
                                var block = JsonConvert.DeserializeObject<RundownDataBlock>(json, settings);
                                RundownDataBlock.AddBlock(block);
                                break;
                        }

                        count++;
                    }
                }

                Plugin.Logger.LogInfo($" - Added {count} partial data of ");
            }

            Plugin.Logger.LogInfo("=== Rundown Loaded ===");

            Global.RundownIdToLoad = 1;
        }
    }
}
