using Scriban.Runtime;

namespace Aitive.Framework.SourceGenerators;

public static class WellKnownNamespaces
{
    public const string GeneratedCodeNamespace = "Aitive.Framework.GeneratedCode";

    internal static void Export(ScriptObject scriptObject)
    {
        var namespaces = new ScriptObject { ["GeneratedCode"] = GeneratedCodeNamespace };

        scriptObject["namespaces"] = namespaces;
    }
}
