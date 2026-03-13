using System.Text.Json;
using System.Text.Json.Serialization;
using Aitive.Framework.Collections;
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
    }
}
