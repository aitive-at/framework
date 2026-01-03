using System.Text.Json.Nodes;

namespace Aitive.Framework.Configuration;

public enum ConfigurationDataMergeMode
{
    Overwrite,
}

public sealed class ConfigurationContext
{
    public JsonObject Data { get; }

    public void SetData(JsonObject data, bool overwrite = false) { }
}
