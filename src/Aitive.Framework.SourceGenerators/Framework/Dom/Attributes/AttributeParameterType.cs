namespace Aitive.Framework.SourceGenerators.Framework.Dom.Attributes;

/// <summary>
/// Represents a type reference for attribute parameters/properties.
/// Handles the mapping between source generator types and target project types.
/// </summary>
public abstract class AttributeParameterType
{
    public abstract string ToCSharpString();
    public abstract bool IsNullable { get; }

    // Primitive types (reference types start as non-nullable for C# 8+ nullable context)
    public static AttributeParameterType String =>
        new PrimitiveAttributeParameterType(typeof(string), "string", false);
    public static AttributeParameterType Int =>
        new PrimitiveAttributeParameterType(typeof(int), "int", false);
    public static AttributeParameterType Long =>
        new PrimitiveAttributeParameterType(typeof(long), "long", false);
    public static AttributeParameterType Bool =>
        new PrimitiveAttributeParameterType(typeof(bool), "bool", false);
    public static AttributeParameterType Double =>
        new PrimitiveAttributeParameterType(typeof(double), "double", false);
    public static AttributeParameterType Float =>
        new PrimitiveAttributeParameterType(typeof(float), "float", false);
    public static AttributeParameterType Char =>
        new PrimitiveAttributeParameterType(typeof(char), "char", false);
    public static AttributeParameterType Byte =>
        new PrimitiveAttributeParameterType(typeof(byte), "byte", false);
    public static AttributeParameterType Object =>
        new PrimitiveAttributeParameterType(typeof(object), "object", false);

    // Type (typeof in target -> INamedTypeSymbol in generator)
    public static AttributeParameterType Type(bool nullable = false) =>
        new SymbolAttributeParameterType(nullable);

    // Enum (fully qualified name, e.g., "MyNamespace.MyEnum")
    public static AttributeParameterType Enum(string fullyQualifiedName, bool nullable = false) =>
        new EnumAttributeParameterType(fullyQualifiedName, nullable);

    // Array of any TypeRef
    public static AttributeParameterType Array(AttributeParameterType elementType) =>
        new ArrayAttributeParameterType(elementType);

    // Make nullable
    public abstract AttributeParameterType Nullable();

    // Get the underlying CLR type for reading (null for Type/Enum)
    public abstract Type? ClrType { get; }
}
