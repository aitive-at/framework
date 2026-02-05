using System.Text.Json;
using System.Text.Json.Serialization;
using Aitive.Framework.Collections;

namespace Aitive.Framework.Json;

public static class JsonSerializerOptionsExtensions
{
    extension(JsonSerializerOptions)
    {
        public static JsonSerializerOptions CreateDefault(IEnumerable<JsonConverter> converters)
        {
            var newOptions = new JsonSerializerOptions(JsonSerializerDefaults.Web);

            newOptions.Converters.AddAll(converters);
            newOptions.AllowOutOfOrderMetadataProperties = true;
            newOptions.NumberHandling = JsonNumberHandling.AllowReadingFromString;
            newOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
            newOptions.PropertyNameCaseInsensitive = true;
            newOptions.RespectNullableAnnotations = true;
            newOptions.WriteIndented = true;
            newOptions.UnknownTypeHandling = JsonUnknownTypeHandling.JsonElement;
            newOptions.UnmappedMemberHandling = JsonUnmappedMemberHandling.Skip;
            newOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;

            return newOptions;
        }
    }

    extension(JsonSerializerOptions options)
    {
        public JsonSerializerOptions WithConverters(
            JsonConverter converter,
            params JsonConverter[] converters
        )
        {
            IReadOnlyList<JsonConverter> list = [converter, .. converters];

            return options.WithConverters(list);
        }

        public JsonSerializerOptions WithConverters(IReadOnlyList<JsonConverter> converters)
        {
            var newOptions = new JsonSerializerOptions(options);
            newOptions.Converters.AddAll(converters);

            return newOptions;
        }

        public JsonSerializerOptions WithoutConverterInstance(JsonConverter converter)
        {
            return options.WithoutConverters(c => c != converter);
        }

        public JsonSerializerOptions WithoutConverters(Func<JsonConverter, bool> predicate)
        {
            var targetConverters = options.Converters.Where(predicate);

            var newOptions = new JsonSerializerOptions(options);
            newOptions.Converters.Clear();
            newOptions.Converters.AddAll(targetConverters);

            return newOptions;
        }

        public JsonSerializerOptions WithoutConverters()
        {
            var newOptions = new JsonSerializerOptions(options);
            newOptions.Converters.Clear();

            return newOptions;
        }
    }
}
