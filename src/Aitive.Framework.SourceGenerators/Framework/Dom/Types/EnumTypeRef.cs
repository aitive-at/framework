namespace Aitive.Framework.SourceGenerators.Framework.Dom.Types;

public sealed class EnumTypeRef(string fullyQualifiedName, bool nullable) : TypeRef
{
    public string FullyQualifiedName { get; } = fullyQualifiedName;

    public override string ToCSharpString() =>
        nullable ? $"{FullyQualifiedName}?" : FullyQualifiedName;

    public override bool IsNullable => nullable;
    public override Type? ClrType => null; // Enum might not exist in generator

    public override TypeRef Nullable() => new EnumTypeRef(FullyQualifiedName, true);
}