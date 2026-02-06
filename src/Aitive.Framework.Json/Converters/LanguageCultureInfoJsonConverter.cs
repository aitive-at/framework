using System.Diagnostics.CodeAnalysis;
using System.Globalization;

namespace Aitive.Framework.Json.Converters;

public sealed class LanguageCultureInfoJsonConverter : StringProxyJsonConverter<CultureInfo>
{
    protected override bool TryParse(string value, [NotNullWhen(true)] out CultureInfo? parsedValue)
    {
        try
        {
            parsedValue = CultureInfo.GetCultureInfo(value);
            return true;
        }
        catch (CultureNotFoundException)
        {
            parsedValue = null;
            return false;
        }
    }

    protected override string ToString(CultureInfo value)
    {
        return value.Name;
    }
}
