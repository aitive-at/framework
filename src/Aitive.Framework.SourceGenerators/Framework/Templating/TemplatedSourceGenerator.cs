using Aitive.Framework.SourceGenerators.Framework.Dom.Attributes;
using Aitive.Framework.SourceGenerators.Framework.Output;
using Aitive.Framework.SourceGenerators.Framework.Templating.Shared;
using Microsoft.CodeAnalysis;
using Scriban;
using Scriban.Runtime;

namespace Aitive.Framework.SourceGenerators.Framework.Templating;

public abstract class TemplatedSourceGenerator<T> : IIncrementalGenerator
{
    private TemplateEngine? _templateEngine;
    private TemplateContext? _templateContext;

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        try
        {
            _templateEngine = OnCreateTemplateEngine();
            _templateContext = OnCreateTemplateContext();

            OnInitialize(context);
        }
        catch (Exception ex)
        {
            context.RegisterPostInitializationOutput(postContext =>
            {
                postContext.AddSource("__Errors.g.cs", ex.ToString());
            });
        }
    }

    protected abstract void OnInitialize(IncrementalGeneratorInitializationContext context);

    protected SourceFile? SafeGenerate(T input, CancellationToken cancellationToken)
    {
        var path = OnGetOutputPath(input);

        try
        {
            var sourceCode = OnGenerate(input, cancellationToken);

            if (sourceCode != null)
            {
                return new SourceFile(path, sourceCode);
            }

            return null;
        }
        catch (Exception ex)
        {
            return new SourceFile(path, ex.ToString());
        }
    }

    protected abstract string OnGetOutputPath(T input);

    protected abstract string? OnGenerate(T input, CancellationToken cancellationToken);

    protected void EmitAttribute(
        IncrementalGeneratorInitializationContext context,
        AttributeDefinition definition
    )
    {
        context.RegisterPostInitializationOutput(postContext =>
        {
            postContext.AddEmbeddedAttributeDefinition();

            postContext.AddSource(definition.Filename, definition.ToString());
        });
    }

    protected string RenderTemplate(string name, object? model = null)
    {
        var template = TemplateEngine.GetTemplate(name);
        var templateContext = TemplateContext;

        if (model != null)
        {
            var scriptModel = new ScriptObject { ["Model"] = model };
            WellKnownNamespaces.Export(scriptModel);
            templateContext.PushGlobal(scriptModel);
        }

        var text = template.Render(templateContext);

        if (model != null)
        {
            templateContext.PopGlobal();
        }

        return text;
    }

    private protected TemplateEngine TemplateEngine =>
        _templateEngine ?? throw new InvalidOperationException("Template engine not initialized");

    private protected TemplateContext TemplateContext =>
        _templateContext ?? throw new InvalidOperationException("Template context not initialized");

    private protected virtual TemplateContext OnCreateTemplateContext()
    {
        var templateContext = new TemplateContext() { MemberRenamer = member => member.Name };
        return templateContext;
    }

    protected virtual TemplateEngine OnCreateTemplateEngine()
    {
        return new TemplateEngine(
            new EmbeddedTemplateLoaderBuilder()
                .AddSearchPaths(GetType(), typeof(WellKnownSharedTemplates))
                .Build()
        );
    }
}
