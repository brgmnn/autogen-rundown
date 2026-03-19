using AutogenRundown.DataBlocks;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace AutogenRundown.Json;

public class AutogenContractResolver : DefaultContractResolver
{
    private static readonly PersistentIdConverter<DataBlocks.Text> TextConverter = new();

    protected override JsonProperty CreateProperty(
        System.Reflection.MemberInfo member,
        MemberSerialization memberSerialization)
    {
        var property = base.CreateProperty(member, memberSerialization);

        if (member.GetCustomAttributes(typeof(JsonTextIdAttribute), false).Length > 0)
        {
            property.Converter = TextConverter;
        }

        return property;
    }
}
