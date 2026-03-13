using System.Text.Json;
using System.Text.Json.Serialization;
using Aitive.Framework.Collections;
using Microsoft.Extensions.DependencyInjection;

namespace Aitive.Framework.Json;

public static class JsonSerializerOptionsExtensions
{
    extension(JsonSerializerOptions options)
    {
        public void ApplyDefaults()
        {
            options.AllowOutOfOrderMetadataProperties = true;
            options.NumberHandling = JsonNumberHandling.AllowReadingFromString;
            options.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
            options.PropertyNameCaseInsensitive = true;
            options.RespectNullableAnnotations = true;
            options.WriteIndented = true;
            options.UnknownTypeHandling = JsonUnknownTypeHandling.JsonElement;
            options.UnmappedMemberHandling = JsonUnmappedMemberHandling.Skip;
            options.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
        }

        public void ConfigureFromServices(IServiceProvider serviceProvider)
        {
            options.Converters.AddAll(serviceProvider.GetServices<JsonConverter>());

            foreach (var module in serviceProvider.GetServices<IJsonModule>().PossiblyOrdered())
            {
                module.Configure(options);
            }
        }

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
