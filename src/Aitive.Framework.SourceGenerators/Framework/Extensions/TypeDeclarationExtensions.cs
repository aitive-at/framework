using Aitive.Framework.SourceGenerators.Framework.Dom;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Aitive.Framework.SourceGenerators.Framework.Extensions;

internal static class TypeDeclarationExtensions
{
    extension(TypeDeclarationSyntax type)
    {
        internal string? Namespace
        {
            get
            {
                // If we don't have a namespace at all we'll return an empty string
                // This accounts for the "default namespace" case
                var nameSpace = string.Empty;

                // Get the containing syntax node for the type declaration
                // (could be a nested type, for example)
                var potentialNamespaceParent = type.Parent;

                // Keep moving "out" of nested classes etc until we get to a namespace
                // or until we run out of parents
                while (
                    potentialNamespaceParent != null
                    && potentialNamespaceParent is not NamespaceDeclarationSyntax
                    && potentialNamespaceParent is not FileScopedNamespaceDeclarationSyntax
                )
                {
                    potentialNamespaceParent = potentialNamespaceParent.Parent;
                }

                // Build up the final namespace by looping until we no longer have a namespace declaration
                if (potentialNamespaceParent is BaseNamespaceDeclarationSyntax namespaceParent)
                {
                    // We have a namespace. Use that as the type
                    nameSpace = namespaceParent.Name.ToString();

                    // Keep moving "out" of the namespace declarations until we
                    // run out of nested namespace declarations
                    while (true)
                    {
                        if (namespaceParent.Parent is not NamespaceDeclarationSyntax parent)
                        {
                            break;
                        }

                        // Add the outer namespace as a prefix to the final namespace
                        nameSpace = $"{namespaceParent.Name}.{nameSpace}";
                        namespaceParent = parent;
                    }
                }

                // return the final namespace
                return string.IsNullOrEmpty(nameSpace) ? null : nameSpace;
            }
        }

        internal string FullName
        {
            get
            {
                // Get the type name
                string typeName = type.Identifier.Text;

                // Handle nested types
                var parent = type.Parent;
                while (parent is TypeDeclarationSyntax parentType)
                {
                    typeName = $"{parentType.Identifier.Text}.{typeName}";
                    parent = parent.Parent;
                }

                // Find the namespace
                var namespaceDecl = type.Ancestors()
                    .OfType<BaseNamespaceDeclarationSyntax>()
                    .FirstOrDefault();

                return namespaceDecl != null ? $"{namespaceDecl.Name}.{typeName}" : typeName;
            }
        }
    }
}
