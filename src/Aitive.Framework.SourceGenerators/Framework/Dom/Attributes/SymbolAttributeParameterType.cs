namespace Aitive.Framework.SourceGenerators.Framework.Dom.Attributes;

public sealed class SymbolAttributeParameterType(bool nullable) : AttributeParameterType
{
    public override string ToCSharpString() => nullable ? "System.Type?" : "System.Type";

    public override bool IsNullable => nullable;
    public override Type? ClrType => null; // Maps to INamedTypeSymbol

    public override AttributeParameterType Nullable() => new SymbolAttributeParameterType(true);
}
