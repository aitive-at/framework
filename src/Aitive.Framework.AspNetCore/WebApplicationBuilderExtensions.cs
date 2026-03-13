using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Diagnostics.Metrics;
using Microsoft.Extensions.Logging;

namespace Aitive.Framework.AspNetCore;

public static class WebApplicationBuilderExtensions
{
    extension(WebApplicationBuilder builder)
    {
        public IReadOnlyDictionary<Type, object> InitialServices =>
            new Dictionary<Type, object>()
            {
                { typeof(IConfiguration), builder.Configuration },
                { typeof(IWebHostEnvironment), builder.Environment },
                { typeof(ILoggingBuilder), builder.Logging },
                { typeof(IMetricsBuilder), builder.Metrics },
                { typeof(WebApplicationBuilder), builder },
            };
    }
}
