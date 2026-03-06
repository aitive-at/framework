using System.Reflection;
using Aitive.Framework.Functional;
using Aitive.Framework.Reflection;

namespace Aitive.Framework.Configuration;

[AttributeUsage(AttributeTargets.Class)]
public sealed class ConfigurationOptionsAttribute : System.Attribute
{
    public string? Name { get; set; }
}

internal static class ConfigurationOptionsExtensions
{
    extension(Type type)
    {
        internal bool IsConfigurationOptions
        {
            get =>
                type.IsDefaultConstructibleClass
                && type.GetCustomAttribute<ConfigurationOptionsAttribute>() != null;
        }

        internal string ConfigurationSectionName
        {
            get
            {
                var attribute = type.GetCustomAttribute<ConfigurationOptionsAttribute>();

                if (attribute == null)
                {
                    throw new NotSupportedException(
                        $"{type} is not a configuration options section type"
                    );
                }

                if (!string.IsNullOrWhiteSpace(attribute.Name))
                {
                    return attribute.Name;
                }

                return type
                    .Name.Replace("Configuration", string.Empty)
                    .Replace("Options", string.Empty)
                    .Replace("Section", string.Empty);
            }
        }
    }
}
