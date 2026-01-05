using Aitive.Framework.SourceGenerators.Framework.Dom.Attributes;

namespace Aitive.Framework.SourceGenerators.Framework.Dom;

public sealed class PropertyDefinition(
    string name,
    AttributeParameterType type,
    object? defaultValue
)
{
    public string Name { get; } = name;
    public AttributeParameterType AttributeParameterType { get; } = type;
    public object? DefaultValue { get; } = defaultValue;
}
