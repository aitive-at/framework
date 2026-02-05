using System.Diagnostics.CodeAnalysis;

namespace Aitive.Framework.Json.Converters;

public sealed class DelegateStringProxyJsonConverter<T> : StringProxyJsonConverter<T>
    where T : notnull
{
    private readonly Func<string, T> _parser;
    private readonly Func<T, string> _toString;

    public DelegateStringProxyJsonConverter(
        Func<string, T> parser,
        Func<T, string>? toString = null
    )
    {
        _parser = parser;
        _toString = toString ?? (value => value.ToString()!);
    }

    protected override bool TryParse(string value, [NotNullWhen(true)] out T? parsedValue)
    {
        try
        {
            parsedValue = _parser(value);
            return true;
        }
        catch (Exception)
        {
            parsedValue = default;
            return false;
        }
    }

    protected override string ToString(T value)
    {
        return _toString(value);
    }
}
