using Aitive.Framework.SourceGenerators.Framework.Dom.Attributes;
using Aitive.Framework.SourceGenerators.Framework.Extensions;
using Aitive.Framework.SourceGenerators.Framework.Output;
using Aitive.Framework.SourceGenerators.Framework.Templating;
using Microsoft.CodeAnalysis;

namespace Aitive.Framework.SourceGenerators.Framework;

public abstract class AttributedTypeSourceGenerator<T>
    : TemplatedSourceGenerator<GeneratorAttributeSyntaxContext>
    where T : class
{
    private AttributeDefinition? _attributeDefinition;

    protected override void OnInitialize(IncrementalGeneratorInitializationContext context)
    {
        _attributeDefinition = AttributeDefinitionBuilder.From<T>();

        EmitAttribute(context, _attributeDefinition);

        var items = context.SyntaxProvider.ForAttributeWithMetadataName(
            _attributeDefinition.FullName,
            OnFilter,
            SafeGenerate
        );

        context.AddSourceFiles(items);
    }

    protected override string? OnGenerate(
        GeneratorAttributeSyntaxContext input,
        CancellationToken cancellationToken
    )
    {
        if (_attributeDefinition == null)
        {
            throw new InvalidOperationException("AttributeDefinition is null");
        }

        var attributes = input.Attributes.Select(a => _attributeDefinition.ReadAs<T>(a)).ToList();

        if (attributes.Count == 0)
        {
            return null;
        }

        return OnGenerate(input, attributes);
    }

    protected override string OnGetOutputPath(GeneratorAttributeSyntaxContext input)
    {
        return ((ITypeSymbol)input.TargetSymbol).CompanionFilename;
    }

    protected virtual string? OnGenerate(
        GeneratorAttributeSyntaxContext input,
        IReadOnlyList<T> attributes
    )
    {
        return OnGenerate(input, attributes[0]);
    }

    protected abstract string? OnGenerate(GeneratorAttributeSyntaxContext input, T attribute);

    protected abstract bool OnFilter(SyntaxNode node, CancellationToken cancellationToken);
}
