using Aitive.Framework.SourceGenerators.Framework.Dom.Attributes;

namespace Aitive.Framework.SourceGenerators.Framework.Dom;

public sealed class ParameterDefinition(
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
