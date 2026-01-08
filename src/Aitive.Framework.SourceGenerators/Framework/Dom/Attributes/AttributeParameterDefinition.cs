namespace Aitive.Framework.SourceGenerators.Framework.Dom.Attributes;

public sealed class AttributeParameterDefinition(
    string name,
    AttributeParameterType type,
    object? defaultValue,
    bool isParams
)
{
    public string Name { get; } = name;
    public AttributeParameterType AttributeParameterType { get; } = type;
    public object? DefaultValue { get; } = defaultValue;
    public bool IsParams { get; } = isParams;
    public bool HasDefault => DefaultValue != null || AttributeParameterType.IsNullable;
}
