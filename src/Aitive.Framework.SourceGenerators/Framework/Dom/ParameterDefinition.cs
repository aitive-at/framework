using Aitive.Framework.SourceGenerators.Framework.Dom.Types;

namespace Aitive.Framework.SourceGenerators.Framework.Dom;

public sealed class ParameterDefinition(
    string name,
    TypeRef type,
    object? defaultValue,
    bool isParams
)
{
    public string Name { get; } = name;
    public TypeRef Type { get; } = type;
    public object? DefaultValue { get; } = defaultValue;
    public bool IsParams { get; } = isParams;
    public bool HasDefault => DefaultValue != null || Type.IsNullable;
}
