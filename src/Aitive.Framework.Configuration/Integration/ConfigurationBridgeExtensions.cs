using System.Globalization;
using System.Text.Json.Nodes;
using Microsoft.Extensions.Configuration;

namespace Aitive.Framework.Configuration.Integration;

public static class ConfigurationBridgeExtensions
{
    extension(IConfiguration configuration)
    {
        public JsonObject ToJsonObject()
        {
            var result = new JsonObject();
            ProcessSection(configuration, result);
            return result;
        }
    }

    private static void ProcessSection(IConfiguration configuration, JsonObject target)
    {
        foreach (var child in configuration.GetChildren())
        {
            target[child.Key] = child.GetChildren().Any() switch
            {
                false => ConvertValue(child.Value),
                true when IsArraySection(child) => BuildArray(child),
                true => BuildObject(child),
            };
        }
    }

    private static JsonObject BuildObject(IConfigurationSection section)
    {
        var obj = new JsonObject();
        ProcessSection(section, obj);
        return obj;
    }

    private static JsonArray BuildArray(IConfigurationSection section)
    {
        var array = new JsonArray();

        foreach (var element in section.GetChildren().OrderBy(c => int.Parse(c.Key)))
        {
            JsonNode? node = element.GetChildren().Any() switch
            {
                false => ConvertValue(element.Value),
                true when IsArraySection(element) => BuildArray(element),
                true => BuildObject(element),
            };
            array.Add(node);
        }

        return array;
    }

    private static bool IsArraySection(IConfigurationSection section)
    {
        var children = section.GetChildren().ToList();

        if (children is [])
        {
            return false;
        }

        for (var i = 0; i < children.Count; i++)
        {
            if (children[i].Key != i.ToString())
                return false;
        }
        return true;
    }

    private static JsonNode? ConvertValue(string? value) =>
        value switch
        {
            null => null,
            _ when value.Equals("true", StringComparison.OrdinalIgnoreCase) => JsonValue.Create(
                true
            ),
            _ when value.Equals("false", StringComparison.OrdinalIgnoreCase) => JsonValue.Create(
                false
            ),
            _ when long.TryParse(value, out var l) => JsonValue.Create(l),
            _ when IsFloatingPoint(value, out var d) => JsonValue.Create(d),
            _ => JsonValue.Create(value),
        };

    private static bool IsFloatingPoint(string value, out double result)
    {
        result = 0;
        return (value.Contains('.') || value.Contains('e') || value.Contains('E'))
            && double.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out result);
    }
}
