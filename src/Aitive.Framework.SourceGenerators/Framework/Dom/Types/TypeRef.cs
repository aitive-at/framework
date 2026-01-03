namespace Aitive.Framework.SourceGenerators.Framework.Dom.Types;

/// <summary>
/// Represents a type reference for attribute parameters/properties.
/// Handles the mapping between source generator types and target project types.
/// </summary>
public abstract class TypeRef
{
    public abstract string ToCSharpString();
    public abstract bool IsNullable { get; }

    // Primitive types (reference types start as non-nullable for C# 8+ nullable context)
    public static TypeRef String => new PrimitiveTypeRef(typeof(string), "string", false);
    public static TypeRef Int => new PrimitiveTypeRef(typeof(int), "int", false);
    public static TypeRef Long => new PrimitiveTypeRef(typeof(long), "long", false);
    public static TypeRef Bool => new PrimitiveTypeRef(typeof(bool), "bool", false);
    public static TypeRef Double => new PrimitiveTypeRef(typeof(double), "double", false);
    public static TypeRef Float => new PrimitiveTypeRef(typeof(float), "float", false);
    public static TypeRef Char => new PrimitiveTypeRef(typeof(char), "char", false);
    public static TypeRef Byte => new PrimitiveTypeRef(typeof(byte), "byte", false);
    public static TypeRef Object => new PrimitiveTypeRef(typeof(object), "object", false);

    // Type (typeof in target -> INamedTypeSymbol in generator)
    public static TypeRef Type(bool nullable = false) => new SymbolTypeRef(nullable);

    // Enum (fully qualified name, e.g., "MyNamespace.MyEnum")
    public static TypeRef Enum(string fullyQualifiedName, bool nullable = false) =>
        new EnumTypeRef(fullyQualifiedName, nullable);

    // Array of any TypeRef
    public static TypeRef Array(TypeRef elementType) => new ArrayTypeRef(elementType);

    // Make nullable
    public abstract TypeRef Nullable();

    // Get the underlying CLR type for reading (null for Type/Enum)
    public abstract Type? ClrType { get; }
}
