namespace Aitive.Framework.SourceGenerators.Framework.Dom.Attributes;

/// <summary>
/// Defines a generic type parameter with constraints.
/// </summary>
public sealed class AttributeTypeParameterDefinition(string name)
{
    public string Name { get; } = name;
    public bool HasClassConstraint { get; private set; }
    public bool HasStructConstraint { get; private set; }
    public bool HasNewConstraint { get; private set; }
    public bool HasNotNullConstraint { get; private set; }
    public bool HasUnmanagedConstraint { get; private set; }
    public IReadOnlyList<string> TypeConstraints => _typeConstraints;

    private readonly List<string> _typeConstraints = new();

    /// <summary>where T : class</summary>
    public AttributeTypeParameterDefinition Class()
    {
        HasClassConstraint = true;
        return this;
    }

    /// <summary>where T : struct</summary>
    public AttributeTypeParameterDefinition Struct()
    {
        HasStructConstraint = true;
        return this;
    }

    /// <summary>where T : new()</summary>
    public AttributeTypeParameterDefinition New()
    {
        HasNewConstraint = true;
        return this;
    }

    /// <summary>where T : notnull</summary>
    public AttributeTypeParameterDefinition NotNull()
    {
        HasNotNullConstraint = true;
        return this;
    }

    /// <summary>where T : unmanaged</summary>
    public AttributeTypeParameterDefinition Unmanaged()
    {
        HasUnmanagedConstraint = true;
        return this;
    }

    /// <summary>where T : SomeType (base class or interface)</summary>
    public AttributeTypeParameterDefinition Is(string typeConstraint)
    {
        _typeConstraints.Add(typeConstraint);
        return this;
    }

    /// <summary>
    /// Generates the constraint clause (e.g., "where T : class, IDisposable, new()").
    /// </summary>
    public string? GenerateConstraintClause()
    {
        var parts = new List<string>();

        if (HasClassConstraint)
        {
            parts.Add("class");
        }

        if (HasStructConstraint)
        {
            parts.Add("struct");
        }

        if (HasUnmanagedConstraint)
        {
            parts.Add("unmanaged");
        }

        if (HasNotNullConstraint)
        {
            parts.Add("notnull");
        }

        parts.AddRange(_typeConstraints);

        if (HasNewConstraint)
        {
            parts.Add("new()");
        }

        return parts.Count > 0 ? $"where {Name} : {string.Join(", ", parts)}" : null;
    }
}
