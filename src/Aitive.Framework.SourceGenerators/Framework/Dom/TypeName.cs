using Aitive.Framework.SourceGenerators.Framework.Extensions;
using Microsoft.CodeAnalysis;

namespace Aitive.Framework.SourceGenerators.Framework.Dom;

public sealed record TypeName(string Raw, string Reference, string Declaration, string Namespace)
{
    private static void Alias<T>(string alias) { }

    public TypeName(ITypeSymbol symbol)
        : this(symbol.Name, symbol.LocalReferenceName, symbol.DeclarationName, symbol.FullNamespace)
    { }

    public string FullReferenceName => $"{Namespace}.{Reference}";

    public string GlobalReferenceName => $"global::{FullReferenceName}";
}
