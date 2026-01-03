using System.Text.Json.Nodes;
using Microsoft.Extensions.Configuration;

namespace Aitive.Framework.Configuration.Integration;

public class DefaultConfigurationBridge : IConfigurationMiddleware
{
    private readonly IConfiguration _configuration;
    private readonly JsonObject _jsonConfiguration;

    public DefaultConfigurationBridge(IConfiguration configuration)
    {
        _configuration = configuration;
        _jsonConfiguration = configuration.ToJsonObject();
    }

    public void Invoke(ConfigurationContext input, Action next) { }

    private JsonObject ToObject(IConfiguration configuration)
    {
        return new();
    }
}
