using Aitive.Framework.SourceGenerators.Framework;
using Aitive.Framework.SourceGenerators.Framework.Extensions;
using Aitive.Framework.SourceGenerators.Framework.Logging;
using Microsoft.CodeAnalysis;

namespace Aitive.Framework.SourceGenerators.Generators.ApplicationDescription;

[Generator]
public sealed class ApplicationDescriptionGenerator : TemplatedSourceGenerator
{
    protected override void OnInitialize(
        IncrementalGeneratorInitializationContext context,
        ILogWriter logWriter
    )
    {
        var usesApplicationFramework = IsUsingApplicationFramework(context);
        var usesNerdbank = IsUsingNerdbank(context);
        var trigger = usesApplicationFramework
            .Combine(usesNerdbank)
            .Select((tuple, token) => tuple is { Left: true, Right: true });

        context.RegisterSourceOutput(trigger, ((productionContext, hasPrerequisites) => { }));
    }

    private IncrementalValueProvider<bool> IsUsingApplicationFramework(
        IncrementalGeneratorInitializationContext context
    )
    {
        return context.CompilationProvider.Select(
            (compilation, _) =>
                compilation.IsAssemblyReferenced(WellKnownAssemblies.AitiveFrameworkApplication)
        );
    }

    private IncrementalValueProvider<bool> IsUsingNerdbank(
        IncrementalGeneratorInitializationContext context
    )
    {
        return context.CompilationProvider.Select(
            (compilation, _) =>
            {
                // NBGV generates ThisAssembly with specific nested class
                var thisAssembly = compilation.GetTypeByMetadataName("ThisAssembly");
                if (thisAssembly == null)
                {
                    return false;
                }

                // Check for NBGV-specific members
                var hasGitInfo = thisAssembly
                    .GetMembers()
                    .Any(m =>
                        m.Name is "GitCommitId" or "AssemblyInformationalVersion"
                        || m is INamedTypeSymbol { Name: "Git" }
                    );

                return hasGitInfo;
            }
        );
    }
}
