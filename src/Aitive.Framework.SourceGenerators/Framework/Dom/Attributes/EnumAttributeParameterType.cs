namespace Aitive.Framework.SourceGenerators.Framework.Dom.Attributes;

public sealed class EnumAttributeParameterType(string fullyQualifiedName, bool nullable)
    : AttributeParameterType
{
    public string FullyQualifiedName { get; } = fullyQualifiedName;

    public override string ToCSharpString() =>
        nullable ? $"{FullyQualifiedName}?" : FullyQualifiedName;

    public override bool IsNullable => nullable;
    public override Type? ClrType => null; // Enum might not exist in generator

    public override AttributeParameterType Nullable() =>
        new EnumAttributeParameterType(FullyQualifiedName, true);
}
