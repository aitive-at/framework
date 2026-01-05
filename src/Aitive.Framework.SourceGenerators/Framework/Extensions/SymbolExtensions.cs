using Aitive.Framework.SourceGenerators.Framework.Dom;
using Microsoft.CodeAnalysis;

namespace Aitive.Framework.SourceGenerators.Framework.Extensions;

internal static class SymbolExtensions
{
    extension(ITypeSymbol symbol)
    {
        internal string FullName =>
            symbol.ToDisplayString(
                new SymbolDisplayFormat(
                    typeQualificationStyle: SymbolDisplayTypeQualificationStyle.NameAndContainingTypesAndNamespaces
                )
            );

        internal string FullNamespace
        {
            get
            {
                var result = new List<string>();
                var currentParent = symbol.ContainingNamespace;

                while (currentParent != null && !string.IsNullOrWhiteSpace(currentParent.Name))
                {
                    result.Add(currentParent.Name);
                    currentParent = currentParent.ContainingNamespace;
                }

                return string.Join(".", result.AsEnumerable().Reverse());
            }
        }

        internal string DeclarationName =>
            symbol.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat);

        internal string CompanionFilename => symbol.FullName + ".g.cs";

        internal string LocalReferenceName =>
            symbol.ToDisplayString(
                new SymbolDisplayFormat(
                    SymbolDisplayGlobalNamespaceStyle.Omitted,
                    SymbolDisplayTypeQualificationStyle.NameOnly,
                    SymbolDisplayGenericsOptions.IncludeTypeParameters,
                    miscellaneousOptions: SymbolDisplayMiscellaneousOptions.None
                )
            );

        internal string GlobalReferenceName => $"global::{symbol.ReferenceName}";

        internal string ReferenceName =>
            symbol.ToDisplayString(
                new SymbolDisplayFormat(
                    SymbolDisplayGlobalNamespaceStyle.Omitted,
                    SymbolDisplayTypeQualificationStyle.NameAndContainingTypesAndNamespaces,
                    SymbolDisplayGenericsOptions.IncludeTypeParameters,
                    propertyStyle: SymbolDisplayPropertyStyle.NameOnly,
                    miscellaneousOptions: SymbolDisplayMiscellaneousOptions.None
                )
            );

        internal TypedValue ToTypedValue(
            string name,
            Accessibility accessibility = Accessibility.Public
        ) => new TypedValue(name, new TypeName(symbol), accessibility.ToCsharpString());

        internal TypeDeclaration TypeDeclaration =>
            new TypeDeclaration(
                symbol.Name,
                symbol.FullNamespace,
                symbol.DeclaredAccessibility.ToCsharpString()
            );
    }
}
