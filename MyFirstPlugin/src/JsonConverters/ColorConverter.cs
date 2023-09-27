using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using UnityEngine;

namespace MyFirstPlugin.JsonConverters
{
    internal class ColorConverter : JsonConverter<Color>
    {
        public override bool HandleNull => false;

        public override Color Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var color = new Color();

            switch (reader.TokenType)
            {
                case JsonTokenType.StartObject:
                    while (reader.Read())
                    {
                        if (reader.TokenType == JsonTokenType.EndObject)
                            return color;

                        if (reader.TokenType != JsonTokenType.PropertyName)
                            throw new JsonException("Expected PropertyName token");

                        var propName = reader.GetString();
                        reader.Read();

                        switch (propName.ToLower())
                        {
                            case "r":
                                color.r = reader.GetSingle();
                                break;

                            case "g":
                                color.g = reader.GetSingle();
                                break;

                            case "b":
                                color.b = reader.GetSingle();
                                break;

                            case "a":
                                color.a = reader.GetSingle();
                                break;
                        }
                    }
                    throw new JsonException("Expected EndObject token");

                case JsonTokenType.String:
                    var strValue = reader.GetString().Trim();
                    if (ColorUtility.TryParseHtmlString(strValue, out color))
                    {
                        return color;
                    }
                    throw new JsonException($"Color format is not right: {strValue}");

                default:
                    throw new JsonException($"ColorJson type: {reader.TokenType} is not implemented!");
            }
        }

        public override void Write(Utf8JsonWriter writer, Color value, JsonSerializerOptions options)
        {
            JsonSerializer.Serialize(writer, value, options);
        }
    }
}