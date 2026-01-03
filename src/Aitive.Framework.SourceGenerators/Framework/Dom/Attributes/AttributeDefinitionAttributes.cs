namespace Aitive.Framework.SourceGenerators.Framework.Dom.Attributes;

/// <summary>
/// Marks a class as an attribute specification. Apply to your spec class.
/// The class structure defines the generated attribute's shape.
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public sealed class GeneratedAttributeAttribute : Attribute
{
    public string? Namespace { get; set; }
    public AttributeTargets Targets { get; set; } = AttributeTargets.All;
    public bool AllowMultiple { get; set; }
    public bool Inherited { get; set; } = true;
}

/// <summary>
/// Marks a property/parameter as a Type reference.
/// In the generated attribute: System.Type
/// When reading: INamedTypeSymbol (stored in object property)
/// </summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Parameter)]
public sealed class TypeSymbolAttribute : Attribute { }

/// <summary>
/// Marks a property/parameter as an enum that exists only in target project.
/// In the generated attribute: the specified enum type
/// When reading: EnumValue (stored in object property)
/// </summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Parameter)]
public sealed class EnumSymbolAttribute(string fullTypeName) : Attribute
{
    public string FullTypeName { get; } = fullTypeName;
}

/// <summary>
/// Defines a generic type parameter for the attribute.
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public sealed class TypeParameterAttribute(string name) : Attribute
{
    public string Name { get; } = name;
    public bool IsClass { get; set; }
    public bool IsStruct { get; set; }
    public bool HasNew { get; set; }
    public bool IsNotNull { get; set; }
    public bool IsUnmanaged { get; set; }
    public string[]? BaseTypes { get; set; }
}

/// <summary>
/// Marks a constructor as a named constructor variant (for multiple constructor support).
/// </summary>
[AttributeUsage(AttributeTargets.Constructor)]
public sealed class ConstructorNameAttribute(string name) : Attribute
{
    public string Name { get; } = name;
}
