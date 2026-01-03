namespace Aitive.Framework.SourceGenerators.Framework.Dom.Types;

public sealed class PrimitiveTypeRef(Type type, string keyword, bool isNullable) : TypeRef
{
    public override string ToCSharpString() => isNullable ? $"{keyword}?" : keyword;

    public override bool IsNullable => isNullable;
    public override Type? ClrType => type;

    public override TypeRef Nullable() => new PrimitiveTypeRef(type, keyword, true);
}