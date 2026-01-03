namespace Aitive.Framework.SourceGenerators.Framework.Dom.Types;

public sealed class SymbolTypeRef(bool nullable) : TypeRef
{
    public override string ToCSharpString() => nullable ? "System.Type?" : "System.Type";

    public override bool IsNullable => nullable;
    public override Type? ClrType => null; // Maps to INamedTypeSymbol

    public override TypeRef Nullable() => new SymbolTypeRef(true);
}