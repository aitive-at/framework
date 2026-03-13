using Aitive.Framework.Json;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.Extensions.Options;

namespace Aitive.Framework.AspNetCore.Json;

public sealed class HttpJsonOptionsConfiguration
    : IConfigureOptions<Microsoft.AspNetCore.Http.Json.JsonOptions>
{
    private readonly IServiceProvider _serviceProvider;

    public HttpJsonOptionsConfiguration(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public void Configure(JsonOptions options)
    {
        options.SerializerOptions.ApplyDefaults();
        options.SerializerOptions.ConfigureFromServices(_serviceProvider);
    }
}
