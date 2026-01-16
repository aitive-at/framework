using Microsoft.CodeAnalysis;

namespace Aitive.Framework.SourceGenerators.Framework.Extensions;

public static class CompilationExtensions
{
    extension(Compilation compilation)
    {
        public bool IsAssemblyReferenced(string assemblyName)
        {
            return compilation.ReferencedAssemblyNames.Any(a =>
                string.Equals(a.Name, assemblyName, StringComparison.OrdinalIgnoreCase)
            );
        }
    }
}
