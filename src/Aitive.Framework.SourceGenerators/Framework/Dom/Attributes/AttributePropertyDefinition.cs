namespace Aitive.Framework.SourceGenerators.Framework.Dom.Attributes;

public sealed class AttributePropertyDefinition(
    string name,
    AttributeParameterType type,
    object? defaultValue
)
{
    public string Name { get; } = name;
    public AttributeParameterType AttributeParameterType { get; } = type;
    public object? DefaultValue { get; } = defaultValue;
}
