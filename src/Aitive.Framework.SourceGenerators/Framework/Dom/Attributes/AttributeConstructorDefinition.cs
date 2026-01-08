namespace Aitive.Framework.SourceGenerators.Framework.Dom.Attributes;

/// <summary>
/// Defines a constructor for the attribute.
/// </summary>
public sealed class AttributeConstructorDefinition(string? name = null)
{
    /// <summary>
    /// Optional name to identify this constructor when reading.
    /// </summary>
    public string? Name { get; } = name;

    public IReadOnlyList<AttributeParameterDefinition> Parameters => _params;
    private readonly List<AttributeParameterDefinition> _params = new();

    public AttributeConstructorDefinition WithParameter(
        string name,
        AttributeParameterType type,
        object? defaultValue = null,
        bool isParams = false
    )
    {
        _params.Add(new AttributeParameterDefinition(name, type, defaultValue, isParams));
        return this;
    }
}
