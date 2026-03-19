using AutogenRundown.DataBlocks;
using Newtonsoft.Json;

namespace AutogenRundown.Json;

public class PersistentIdConverter<T> : JsonConverter<T> where T : DataBlock<T>
{
    public override T ReadJson(JsonReader reader, Type objectType, T? existingValue, bool hasExistingValue, JsonSerializer serializer)
    {
        throw new NotSupportedException("Deserialization is not supported");
    }

    public override void WriteJson(JsonWriter writer, T? value, JsonSerializer serializer)
    {
        writer.WriteValue(value?.PersistentId ?? 0);
    }
}
