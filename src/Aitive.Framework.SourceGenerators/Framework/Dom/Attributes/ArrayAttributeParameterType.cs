namespace Aitive.Framework.SourceGenerators.Framework.Dom.Attributes;

public sealed class ArrayAttributeParameterType(AttributeParameterType elementType)
    : AttributeParameterType
{
    public AttributeParameterType ElementAttributeParameterType { get; } = elementType;

    public override string ToCSharpString() =>
        $"{ElementAttributeParameterType.ToCSharpString()}[]";

    public override bool IsNullable => true; // Arrays are reference types
    public override Type? ClrType => null;

    public override AttributeParameterType Nullable() => this; // Arrays are already nullable
}
