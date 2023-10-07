using Localization;
using Newtonsoft.Json;

namespace AutogenRundown.Json
{
    public class LocalizedTextConverter : JsonConverter<LocalizedText>
    {
        public override LocalizedText? ReadJson(JsonReader reader, Type objectType, LocalizedText? existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            switch (reader.TokenType)
            {
                case JsonToken.String:
                    return new LocalizedText
                    {
                        Id = 0u,
                        UntranslatedText = (string)(reader.Value ?? "")
                    };

                case JsonToken.Integer:
                    return new LocalizedText
                    {
                        Id = Convert.ToUInt32(reader.Value ?? 0),
                        UntranslatedText = null
                    };

                default:
                    return null;
            }
        }

        public override void WriteJson(JsonWriter writer, LocalizedText? value, JsonSerializer serializer)
        {
            writer.WriteValue(value?.UntranslatedText);
        }
    }
}
