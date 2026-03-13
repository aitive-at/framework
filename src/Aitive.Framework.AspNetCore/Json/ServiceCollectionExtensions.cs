using Microsoft.AspNetCore.Http.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Aitive.Framework.AspNetCore.Json;

public static class ServiceCollectionExtensions
{
    extension(IServiceCollection services)
    {
        public void ConfigureGlobalJsonOptions()
        {
            services.AddSingleton<
                IConfigureOptions<Microsoft.AspNetCore.Mvc.JsonOptions>,
                MvcJsonOptionsConfiguration
            >();
            services.AddSingleton<
                IConfigureOptions<Microsoft.AspNetCore.Http.Json.JsonOptions>,
                HttpJsonOptionsConfiguration
            >();
        }
    }
}
