namespace Aitive.Framework.SourceGenerators.Framework.Dom;

/// <summary>
/// Defines a generic type parameter with constraints.
/// </summary>
public sealed class TypeParameterDefinition(string name)
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
    public TypeParameterDefinition Class()
    {
        HasClassConstraint = true;
        return this;
    }

    /// <summary>where T : struct</summary>
    public TypeParameterDefinition Struct()
    {
        HasStructConstraint = true;
        return this;
    }

    /// <summary>where T : new()</summary>
    public TypeParameterDefinition New()
    {
        HasNewConstraint = true;
        return this;
    }

    /// <summary>where T : notnull</summary>
    public TypeParameterDefinition NotNull()
    {
        HasNotNullConstraint = true;
        return this;
    }

    /// <summary>where T : unmanaged</summary>
    public TypeParameterDefinition Unmanaged()
    {
        HasUnmanagedConstraint = true;
        return this;
    }

    /// <summary>where T : SomeType (base class or interface)</summary>
    public TypeParameterDefinition Is(string typeConstraint)
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
