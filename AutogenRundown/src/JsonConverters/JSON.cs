//using MTFO.Ext.PartialData.JsonConverters;
using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace AutogenRundown.JsonConverters
{
    internal static class JSON
    {
        private readonly static JsonSerializerOptions _Setting = CreateSetting();

        static JSON()
        {
            _Setting = CreateSetting();
            _Setting.Converters.Add(new PersistentIDConverter());
        }

        private static JsonSerializerOptions CreateSetting()
        {
            var setting = new JsonSerializerOptions()
            {
                ReadCommentHandling = JsonCommentHandling.Skip,
                IncludeFields = true,
                AllowTrailingCommas = true,
                WriteIndented = true
            };
            setting.Converters.Add(new Il2CppListConverterFactory());
            setting.Converters.Add(new ColorConverter());
            setting.Converters.Add(new JsonStringEnumConverter());
            setting.Converters.Add(new LocalizedTextConverter());

            return setting;
        }

        public static T Deserialize<T>(string json)
        {
            return JsonSerializer.Deserialize<T>(json, _Setting);
        }

        public static object Deserialize(string json, Type type)
        {
            return JsonSerializer.Deserialize(json, type, _Setting);
        }
    }
}