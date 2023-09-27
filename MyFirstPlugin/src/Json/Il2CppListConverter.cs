using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace MyFirstPlugin.src.Json
{
    using Il2CppCollections = Il2CppSystem.Collections.Generic;

    internal class Il2CppListConverter<T> : JsonConverter<Il2CppCollections.List<T>>
    {
        public override Il2CppCollections.List<T>? ReadJson(
            JsonReader reader, 
            Type objectType, 
            Il2CppCollections.List<T>? existingValue, 
            bool hasExistingValue, 
            JsonSerializer serializer)
        {
            var list = new Il2CppCollections.List<T>();

            if (reader.TokenType == JsonToken.StartArray)
            {
                var newList = serializer.Deserialize<List<T>>(reader);

                foreach (var item in newList)
                {
                    list.Add(item);
                }

                return list;
            }

            return null;
        }

        public override void WriteJson(JsonWriter writer, Il2CppCollections.List<T>? value, JsonSerializer serializer)
        {
            writer.WriteStartArray();

            foreach (var v in value)
            {
                serializer.Serialize(writer, v);
            }

            writer.WriteEndArray();
        }
    }
}
