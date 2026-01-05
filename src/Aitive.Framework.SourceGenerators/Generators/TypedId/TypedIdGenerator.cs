using System.Reflection.Metadata;
using System.Text;
using Aitive.Framework.SourceGenerators.Framework;
using Aitive.Framework.SourceGenerators.Framework.Dom;
using Aitive.Framework.SourceGenerators.Framework.Dom.Attributes;
using Aitive.Framework.SourceGenerators.Framework.Extensions;
using Aitive.Framework.SourceGenerators.Framework.Output;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Aitive.Framework.SourceGenerators.Generators.TypedId;

[GeneratedAttribute(Namespace = WellKnownNamespaces.GeneratedCodeNamespace)]
public sealed class TypedIdAttribute
{
    public bool GenerateJsonConverter { get; set; } = true;

    public bool GenerateTypeConverter { get; set; } = true;

    public bool ImplementComparisons { get; set; } = true;

    public bool ImplementCasts { get; set; } = true;

    public string Separator { get; set; } = "/";
}

public sealed record TypedIdModel(
    TypeDeclaration Declaration,
    IReadOnlyList<TypedValue> Values,
    TypedIdAttribute Attribute
)
{
    public int Count => Values.Count;
}

[Generator]
public class TypedIdGenerator : AttributedTypeSourceGenerator<TypedIdAttribute>
{
    protected override string? OnGenerate(
        GeneratorAttributeSyntaxContext input,
        TypedIdAttribute attribute
    )
    {
        var classDeclaration = (RecordDeclarationSyntax)input.TargetNode;
        var classSymbol = (INamedTypeSymbol)input.TargetSymbol;

        if (classDeclaration.ParameterList == null)
        {
            return "No properties specified";
        }

        var properties = classDeclaration
            .ParameterList.Parameters.Select(p => new
            {
                Name = p.Identifier.Text,
                Type = p.Type != null ? input.SemanticModel.GetTypeInfo(p.Type).Type : null,
            })
            .Where(t => t.Type != null)
            .Select(t => t.Type!.ToTypedValue(t.Name, classSymbol.DeclaredAccessibility))
            .ToList();

        var model = new TypedIdModel(classSymbol.TypeDeclaration, properties, attribute);

        return RenderTemplate("TypedId", model);
    }

    protected override bool OnFilter(SyntaxNode node, CancellationToken cancellationToken)
    {
        return node is RecordDeclarationSyntax && node.IsKind(SyntaxKind.RecordStructDeclaration);
    }
}
