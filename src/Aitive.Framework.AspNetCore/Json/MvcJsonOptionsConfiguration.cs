using Aitive.Framework.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace Aitive.Framework.AspNetCore.Json;

public sealed class MvcJsonOptionsConfiguration
    : IConfigureOptions<Microsoft.AspNetCore.Mvc.JsonOptions>
{
    private readonly IServiceProvider _serviceProvider;

    public MvcJsonOptionsConfiguration(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public void Configure(JsonOptions options)
    {
        options.JsonSerializerOptions.ApplyDefaults();
        options.JsonSerializerOptions.ConfigureFromServices(_serviceProvider);
    }
}
