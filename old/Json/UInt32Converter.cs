using Newtonsoft.Json;

namespace AutogenRundown.Json
{
    public class UInt32Converter : JsonConverter<UInt32>
    {
        public override UInt32 ReadJson(JsonReader reader, Type objectType, UInt32 existingValue, bool hasExistingValue, JsonSerializer serializer)
            => Convert.ToUInt32(reader.Value ?? 0);

        public override void WriteJson(JsonWriter writer, UInt32 value, JsonSerializer serializer)
            => writer.WriteValue(value);
    }
}
