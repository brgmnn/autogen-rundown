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


    internal class HarmonyGameDataLoader
    {
        //[HarmonyPatch(typeof(RundownDataBlock), nameof())]
        //[HarmonyPostfix]
        //public static void MyPatch(RundownDataBlock __instance, bool __result)
        //{
        //    Plugin.Logger.LogInfo($"=== RundownDataBlock ARE WE LOADED???? {__result} ===");
        //}

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

    internal static class GameDataLoader
    {
        public static event GameDataContentLoadEvent OnGameDataContentLoad;
        public static event GameDataContentLoadedDelegate OnGameDataContentLoaded;

        private unsafe delegate IntPtr GetFileContentsDel(Il2CppMethodInfo* methodInfo);

        private static string _BasePathToDump;
        private static INativeDetour _Detour; //To Prevent GC Error
        private static GetFileContentsDel _Original;

        public static unsafe void Patch()
        {
            return;

            _BasePathToDump = Path.Combine(Paths.PluginPath, "MyFirstPlugin", "Datablocks");

            var method = Il2CppAPI.GetIl2CppMethod<DBBase>(
                nameof(DBBase.GetFileContents),
                typeof(string).FullName,
                isGeneric: false,
                Array.Empty<string>());

            Plugin.Logger.LogInfo($"Got here: {nameof(DBBase.GetFileContents)} {typeof(string).FullName}");

            //_Detour = INativeDetour.CreateAndApply((nint)method, Dtor_GetFileContents, out _Original);
        }

        public static unsafe IntPtr Dtor_GetFileContents(Il2CppMethodInfo* methodInfo)
        {
            var originalResult = _Original.Invoke(methodInfo);

            Plugin.Logger.LogInfo("--- LoadApply called ---");

            return originalResult;
        }

        private static Stream GetContentStream(IntPtr originalContentPtr)
        {
            //var fileName = datablock.BinaryFileName;
            //var jsonFileName = $"{fileName}.json";
            var originalJson = IL2CPP.Il2CppStringToManaged(originalContentPtr);

            //string filePath = Path.Combine(ConfigManager.GameDataPath, jsonFileName);
            //if (File.Exists(filePath))
            //{
            //    Log.Verbose($"Opening filestream of [{fileName}] from disk...");
            //    Log.Verbose(filePath);
            //    return PathUtil.OpenUtf8Stream(filePath);
            //}
            //else
            //{
                //Log.Verbose($"No file found at [{fileName}]");
                return new MemoryStream(Encoding.UTF8.GetBytes(originalJson));
            //}
        }

        internal static void Invoke_OnGameDataContentLoad(string datablockName, Stream jsonContentStream, in List<string> jsonItemsToInject)
        {
            if (OnGameDataContentLoad != null)
            {
                OnGameDataContentLoad.Invoke(datablockName, new StreamReader(jsonContentStream).ReadToEnd(), in jsonItemsToInject);
            }
        }

        internal static void Invoke_OnGameDataContentLoaded(string datablockName, string jsonContent)
        {
            OnGameDataContentLoaded?.Invoke(datablockName, jsonContent);
        }
    }
}
