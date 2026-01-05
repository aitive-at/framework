using Aitive.Framework.SourceGenerators.Framework.Dom.Attributes;

namespace Aitive.Framework.SourceGenerators.Framework.Dom;

/// <summary>
/// Defines a constructor for the attribute.
/// </summary>
public sealed class ConstructorDefinition(string? name = null)
{
    /// <summary>
    /// Optional name to identify this constructor when reading.
    /// </summary>
    public string? Name { get; } = name;

    public IReadOnlyList<ParameterDefinition> Parameters => _params;
    private readonly List<ParameterDefinition> _params = new();

    public ConstructorDefinition WithParameter(
        string name,
        AttributeParameterType type,
        object? defaultValue = null,
        bool isParams = false
    )
    {
        _params.Add(new ParameterDefinition(name, type, defaultValue, isParams));
        return this;
    }
}
