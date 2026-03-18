using System.Reflection;

namespace Aitive.Framework.Plugins;

[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = false)]
public sealed class PluginAttribute : System.Attribute
{
    public PluginAttribute(string id, string version)
    {
        Id = id;
        Version = version;
    }

    public string Id { get; }

    public string Version { get; }

    public string Description
    {
        get => field ?? Id;
        set;
    }
}

[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
public sealed class PluginDependencyAttribute : System.Attribute
{
    public PluginDependencyAttribute(string id, string? versionRange = null)
    {
        Id = id;
        VersionRange = versionRange;
    }

    public string Id { get; }

    public string? VersionRange { get; }
}

[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
public sealed class PluginPropertyAttribute : System.Attribute
{
    public PluginPropertyAttribute(string key, object value)
    {
        Key = key;
        Value = value;
    }

    public string Key { get; }

    public object Value { get; }
}
