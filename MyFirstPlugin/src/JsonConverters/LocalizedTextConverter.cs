using Localization;
using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using UnityEngine;

namespace MyFirstPlugin.JsonConverters
{
    internal class LocalizedTextConverter : JsonConverter<LocalizedText>
    {
        public override bool HandleNull => false;

        public override LocalizedText Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            switch (reader.TokenType)
            {
                case JsonTokenType.String:
                    var strValue = reader.GetString();
                    
                    return new LocalizedText
                    {
                        Id = 0,
                        UntranslatedText = strValue
                    };
                    

                case JsonTokenType.Number:
                    return new LocalizedText()
                    {
                        Id = reader.GetUInt32(),
                        UntranslatedText = null
                    };

                default:
                    throw new JsonException($"LocalizedTextJson type: {reader.TokenType} is not implemented!");
            }
        }

        public override void Write(Utf8JsonWriter writer, LocalizedText value, JsonSerializerOptions options)
        {
            JsonSerializer.Serialize(writer, value, options);
        }
    }
}