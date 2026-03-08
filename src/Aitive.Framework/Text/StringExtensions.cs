using System.Text;
using System.Text.RegularExpressions;

namespace Aitive.Framework.Text;

public static class StringExtensions
{
    extension(string value)
    {
        public string ToUrlSafeSlug()
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return string.Empty;
            }

            // Normalize unicode characters (e.g., ä → a, é → e)
            var normalized = value.Normalize(NormalizationForm.FormD);
            var sb = new StringBuilder(normalized.Length);

            foreach (var c in normalized)
            {
                var category = char.GetUnicodeCategory(c);
                if (category == System.Globalization.UnicodeCategory.NonSpacingMark)
                {
                    continue; // skip diacritics
                }

                if (char.IsLetterOrDigit(c))
                {
                    sb.Append(c);
                }
                else if (c is ' ' or '-' or '_')
                {
                    sb.Append('-');
                }
                // else: drop the character
            }

            var slug = sb.ToString().ToLowerInvariant();

            // Collapse consecutive hyphens and trim them
            slug = Regex.Replace(slug, "-{2,}", "-").Trim('-');

            return slug;
        }
    }
}
