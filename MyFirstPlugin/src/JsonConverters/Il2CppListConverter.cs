using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace MyFirstPlugin.JsonConverters
{
    using Il2CppCollections = Il2CppSystem.Collections.Generic;

    internal class Il2CppListConverter<T> : JsonConverter<Il2CppCollections.List<T>>
    {
        public override bool HandleNull => false;

        public override Il2CppCollections.List<T> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var list = new Il2CppCollections.List<T>();
            if (reader.TokenType == JsonTokenType.StartArray)
            {
                var newList = JsonSerializer.Deserialize<List<T>>(ref reader, options);
                foreach (var item in newList)
                {
                    list.Add(item);
                }
                return list;
            }
            return null;
        }

        public override void Write(Utf8JsonWriter writer, Il2CppCollections.List<T> value, JsonSerializerOptions options)
        {
            writer.WriteStartArray();
            foreach (var i in value)
            {
                JsonSerializer.Serialize(writer, i, options);
            }
            writer.WriteEndArray();
        }
    }
}