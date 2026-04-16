using System.Text.Json;
using System.Text.Json.Serialization;
using Aitive.Framework.Collections;
using Aitive.Framework.Io;
using Aitive.Framework.Json.Converters;
using Microsoft.Extensions.DependencyInjection;

namespace Aitive.Framework.Json;

public static class ServiceCollectionExtensions
{
    extension(IServiceCollection services)
    {
        public void AddJsonSerializerOptions()
        {
            services.AddSingleton<JsonSerializerOptions>(sp =>
            {
                var result = new JsonSerializerOptions(JsonSerializerDefaults.Web);

                result.ApplyDefaults();
                result.ConfigureFromServices(sp);

                return result;
            });
        }

        public void AddDefaultJsonConverters()
        {
            services.AddJsonConverter<FlagsEnumArrayJsonConverterFactory>();
            services.AddJsonConverter<OptionalJsonConverterFactory>();
            services.AddJsonConverter<ResultJsonConverterFactory>();
            services.AddJsonConverter<LanguageCultureInfoJsonConverter>();
            services.AddJsonConverter<CountryRegionInfoJsonConverter>();
            services.AddJsonConverter<PathJsonConverter>();
        }

        public IServiceCollection AddJsonConverter<T>()
            where T : JsonConverter
        {
            services.AddSingleton<JsonConverter, T>();
            return services;
        }

        public IServiceCollection AddJsonConverter<T>(Func<IServiceProvider, T> factory)
            where T : JsonConverter
        {
            services.AddSingleton<JsonConverter>(factory);
            return services;
        }
    }
}
