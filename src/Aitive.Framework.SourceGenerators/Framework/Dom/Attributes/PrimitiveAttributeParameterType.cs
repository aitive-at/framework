namespace Aitive.Framework.SourceGenerators.Framework.Dom.Attributes;

public sealed class PrimitiveAttributeParameterType(Type type, string keyword, bool isNullable)
    : AttributeParameterType
{
    public override string ToCSharpString() => isNullable ? $"{keyword}?" : keyword;

    public override bool IsNullable => isNullable;
    public override Type? ClrType => type;

    public override AttributeParameterType Nullable() =>
        new PrimitiveAttributeParameterType(type, keyword, true);
}
