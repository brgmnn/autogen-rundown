using System;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace AutogenRundown.JsonConverters
{
    using Il2CppCollections = Il2CppSystem.Collections.Generic;

    internal class Il2CppListConverterFactory : JsonConverterFactory
    {
        public override bool CanConvert(Type typeToConvert)
        {
            if (!typeToConvert.IsGenericType)
            {
                return false;
            }

            if (typeToConvert.GetGenericTypeDefinition() != typeof(Il2CppCollections.List<>))
            {
                return false;
            }

            return true;
        }

        public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
        {
            Type innerType = typeToConvert.GetGenericArguments()[0];

            JsonConverter converter = (JsonConverter)Activator.CreateInstance(
                typeof(Il2CppListConverter<>).MakeGenericType(
                    new Type[] { innerType }),
                BindingFlags.Instance | BindingFlags.Public,
                binder: null,
                args: null,
                culture: null);

            return converter;
        }
    }
}