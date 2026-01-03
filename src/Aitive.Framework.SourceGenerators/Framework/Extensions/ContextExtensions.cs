using Aitive.Framework.SourceGenerators.Framework.Output;
using Microsoft.CodeAnalysis;

namespace Aitive.Framework.SourceGenerators.Framework.Extensions;

internal static class ContextExtensions
{
    extension(IncrementalGeneratorInitializationContext context)
    {
        public void AddSourceFiles(IncrementalValuesProvider<SourceFile?> sourceFiles)
        {
            context.RegisterSourceOutput(
                sourceFiles.Where(o => o != null),
                (productionContext, sourceFile) => sourceFile?.AddToOutput(productionContext)
            );
        }
    }
}
