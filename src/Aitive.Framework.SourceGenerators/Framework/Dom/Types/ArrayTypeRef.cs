namespace Aitive.Framework.SourceGenerators.Framework.Dom.Types;

public sealed class ArrayTypeRef(TypeRef elementType) : TypeRef
{
    public TypeRef ElementType { get; } = elementType;

    public override string ToCSharpString() => $"{ElementType.ToCSharpString()}[]";

    public override bool IsNullable => true; // Arrays are reference types
    public override Type? ClrType => null;

    public override TypeRef Nullable() => this; // Arrays are already nullable
}