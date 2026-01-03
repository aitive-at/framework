using Aitive.Framework.SourceGenerators.Framework.Dom.Types;

namespace Aitive.Framework.SourceGenerators.Framework.Dom;

public sealed class PropertyDefinition(string name, TypeRef type, object? defaultValue)
{
    public string Name { get; } = name;
    public TypeRef Type { get; } = type;
    public object? DefaultValue { get; } = defaultValue;
}
