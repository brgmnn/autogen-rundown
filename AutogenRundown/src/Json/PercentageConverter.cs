using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace AutogenRundown.Json;

/// <summary>
/// Usage:
///
/// ```csharp
/// public class MyClass
/// {
///     [JsonConverter(typeof(DoubleOrStringConverter))]
///     public double Value { get; set; }
/// }
/// ```
/// </summary>
public class PercentageConverter : JsonConverter<double>
{
    public override double ReadJson(JsonReader reader, Type objectType, double existingValue, bool hasExistingValue, JsonSerializer serializer)
    {
        var token = JToken.Load(reader);

        if (token.Type == JTokenType.Float || token.Type == JTokenType.Integer)
        {
            return token.Value<double>();
        }
        else if (token.Type == JTokenType.String)
        {
            var stringValue = token.Value<string>();

            // Handle percentage format "1%"
            if (stringValue.EndsWith("%"))
            {
                var numericPart = stringValue.TrimEnd('%');

                if (double.TryParse(numericPart, out var percentValue))
                {
                    return percentValue / 100.0;
                }
            }

            // Try parsing as regular double
            if (double.TryParse(stringValue, out double result))
            {
                return result;
            }
        }

        throw new JsonException($"Unable to convert to double");
    }

    public override void WriteJson(JsonWriter writer, double value, JsonSerializer serializer)
    {
        writer.WriteValue(value);
    }
}
