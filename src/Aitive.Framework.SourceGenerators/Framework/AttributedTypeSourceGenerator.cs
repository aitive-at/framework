using Aitive.Framework.SourceGenerators.Framework.Dom.Attributes;
using Aitive.Framework.SourceGenerators.Framework.Extensions;
using Aitive.Framework.SourceGenerators.Framework.Logging;
using Aitive.Framework.SourceGenerators.Framework.Output;
using Aitive.Framework.SourceGenerators.Framework.Templating;
using Microsoft.CodeAnalysis;

namespace Aitive.Framework.SourceGenerators.Framework;

public abstract class AttributedTypeSourceGenerator<T> : TemplatedSourceGenerator
    where T : class
{
    protected override void OnInitialize(
        IncrementalGeneratorInitializationContext context,
        ILogWriter logWriter
    )
    {
        var attributeDefinition = AttributeDefinition.From<T>();
        context.AddMarkerAttribute(attributeDefinition);

        context.GenerateSourceFilesForAttribute<T>(
            OnFilter,
            OnGenerate,
            OnGenerateFilename,
            attributeDefinition
        );
    }

    protected virtual bool OnFilter(T attribute, GeneratorAttributeSyntaxContext context)
    {
        return true;
    }

    protected abstract bool OnGenerate(
        T attribute,
        GeneratorAttributeSyntaxContext input,
        SourceWriter writer,
        ILogWriter log
    );

    protected virtual string OnGenerateFilename(
        T attribute,
        GeneratorAttributeSyntaxContext context
    )
    {
        return ((ITypeSymbol)context.TargetSymbol).CompanionFilename;
    }
}
