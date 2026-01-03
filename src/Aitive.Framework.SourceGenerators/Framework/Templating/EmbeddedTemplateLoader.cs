using System.Reflection;
using System.Text;
using Scriban;
using Scriban.Parsing;
using Scriban.Runtime;

namespace Aitive.Framework.SourceGenerators.Framework.Templating;

internal sealed class EmbeddedTemplateLoaderBuilder
{
    private readonly List<string> _searchPaths;

    internal EmbeddedTemplateLoaderBuilder()
    {
        _searchPaths = new();
    }

    internal EmbeddedTemplateLoaderBuilder AddSearchPaths(
        string searchPath,
        params string[] additionalPaths
    )
    {
        _searchPaths.Add(searchPath);

        foreach (var path in additionalPaths)
        {
            _searchPaths.Add(path);
        }

        return this;
    }

    internal EmbeddedTemplateLoaderBuilder AddSearchPaths(
        Type namespaceType,
        params Type[] additionalTypes
    )
    {
        if (namespaceType.Namespace != null)
        {
            _searchPaths.Add(namespaceType.Namespace);
        }

        foreach (var type in additionalTypes)
        {
            if (type.Namespace != null)
            {
                _searchPaths.Add(type.Namespace);
            }
        }

        return this;
    }

    internal EmbeddedTemplateLoader Build()
    {
        return new(_searchPaths.ToArray());
    }
}

internal sealed class EmbeddedTemplateLoader : ITemplateLoader
{
    internal const string Extension = ".scriban";

    private readonly string[] _searchPaths;

    internal EmbeddedTemplateLoader(string[] searchPaths)
    {
        _searchPaths = searchPaths;
    }

    public string GetPath(TemplateContext context, SourceSpan callerSpan, string templateName)
    {
        foreach (var candidatePath in GetCandidatePaths(templateName))
        {
            var stream = GetType().Assembly.GetManifestResourceStream(candidatePath);

            if (stream != null)
            {
                stream.Dispose();
                return candidatePath;
            }
        }

        throw new FileNotFoundException(
            $"Could not find template: {templateName}, tried {string.Join("\\n", GetCandidatePaths(templateName))}"
        );
    }

    public string Load(TemplateContext context, SourceSpan callerSpan, string templatePath)
    {
        using var stream = GetType().Assembly.GetManifestResourceStream(templatePath);

        if (stream != null)
        {
            using var reader = new StreamReader(stream, Encoding.UTF8, false, 1024, true);

            return reader.ReadToEnd();
        }

        throw new FileNotFoundException($"Could not find template: {templatePath}");
    }

    private IEnumerable<string> GetCandidatePaths(string name)
    {
        var hasExtension = name.EndsWith(Extension, StringComparison.InvariantCultureIgnoreCase);
        var extensionToUse = hasExtension ? null : Extension;

        foreach (var searchPath in _searchPaths)
        {
            yield return GetPath(name, extensionToUse, searchPath);
        }

        yield return GetPath(name, extensionToUse, null);
    }

    private string GetPath(string name, string? extension, string? searchPath)
    {
        var filename = extension != null ? $"{name}{extension}" : name;

        return searchPath != null ? $"{searchPath}.{filename}" : filename;
    }
}
