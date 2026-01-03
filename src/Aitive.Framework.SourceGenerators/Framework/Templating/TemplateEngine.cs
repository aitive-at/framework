using System.Collections.Concurrent;
using Scriban;
using Scriban.Parsing;
using Scriban.Runtime;

namespace Aitive.Framework.SourceGenerators.Framework.Templating;

public sealed class TemplateEngine
{
    private readonly TemplateContext _dummyContext;
    private readonly ITemplateLoader _templateLoader;
    private readonly ConcurrentDictionary<string, Template> _templates;

    internal TemplateEngine(ITemplateLoader templateLoader)
    {
        _dummyContext = new();
        _templateLoader = templateLoader;
        _templates = new();
    }

    internal Template GetTemplate(string name)
    {
        return _templates.GetOrAdd(name, ParseTemplate);
    }

    private Template ParseTemplate(string name)
    {
        var sourceSpan = new SourceSpan(name, new TextPosition(0, 0, 0), TextPosition.Eof);

        var path = _templateLoader.GetPath(_dummyContext, sourceSpan, name);

        var sourceCode = _templateLoader.Load(_dummyContext, sourceSpan, path);

        if (sourceCode == null)
        {
            throw new FileNotFoundException($"Could not find template: {name}");
        }

        return ParseTemplate(path, sourceCode);
    }

    private Template ParseTemplate(string filename, string sourceCode)
    {
        var template = Template.Parse(sourceCode, filename);

        if (template.HasErrors)
        {
            var allErrorMessages = template.Messages.Select(m =>
                $"{m.Message}, line: {m.Span.Start.Line}, column: {m.Span.Start.Column}"
            );

            var errorText = $"Template error in: {filename}\n{string.Join("\n", allErrorMessages)}";

            throw new FormatException(errorText);
        }

        return template;
    }
}
