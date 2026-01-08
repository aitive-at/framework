using Aitive.Framework.SourceGenerators.Framework.Output;
using Aitive.Framework.SourceGenerators.Framework.Patterns;
using Aitive.Framework.SourceGenerators.Framework.Templating;
using Aitive.Framework.SourceGenerators.Framework.Templating.Shared;
using Microsoft.CodeAnalysis;
using Scriban;
using Scriban.Runtime;

namespace Aitive.Framework.SourceGenerators.Framework;

public abstract class TemplatedSourceGenerator : IncrementalSourceGenerator
{
    private readonly ThreadLocal<TemplateEngine> _templateEngine;
    private readonly ThreadLocal<TemplateContext> _templateContext;

    protected TemplatedSourceGenerator()
    {
        _templateEngine = new ThreadLocal<TemplateEngine>(OnCreateTemplateEngine);
        _templateContext = new ThreadLocal<TemplateContext>(OnCreateTemplateContext);
    }

    protected TemplateEngine TemplateEngine => _templateEngine.Value;

    private protected TemplateContext TemplateContext => _templateContext.Value;

    protected string RenderTemplate(string name, object? model = null)
    {
        var template = TemplateEngine.GetTemplate(name);
        using var m = PushModel(model, "Model");
        return template.Render(TemplateContext);
    }

    protected IDisposable PushModel(object? model, string? name = null)
    {
        if (model != null)
        {
            var scriptObject = new ScriptObject();

            if (name != null)
            {
                scriptObject[name] = model;
            }
            else
            {
                scriptObject.Import(model);
            }

            TemplateContext.PushGlobal(scriptObject);
            return new ActionDisposable(() => TemplateContext.PopGlobal());
        }

        return new ActionDisposable(() => { });
    }

    private protected virtual TemplateContext OnCreateTemplateContext()
    {
        var templateContext = new TemplateContext() { MemberRenamer = member => member.Name };
        var rootScriptObject = new ScriptObject();

        rootScriptObject.Import(typeof(TemplateFunctions));
        WellKnownNamespaces.Export(rootScriptObject);

        templateContext.PushGlobal(rootScriptObject);

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
