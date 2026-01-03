using Microsoft.CodeAnalysis;

namespace Aitive.Framework.SourceGenerators.Framework.Extensions;

internal static class AccessibilityExtensions
{
    extension(Accessibility accessibility)
    {
        internal string ToCsharpString()
        {
            return accessibility switch
            {
                Accessibility.NotApplicable => string.Empty,
                Accessibility.Private => "private",
                Accessibility.ProtectedAndInternal => "protected internal",
                Accessibility.Protected => "protected",
                Accessibility.Internal => "internal",
                Accessibility.ProtectedOrInternal => "protected internal",
                Accessibility.Public => "public",
                _ => throw new ArgumentOutOfRangeException(
                    nameof(accessibility),
                    accessibility,
                    null
                ),
            };
        }
    }
}
