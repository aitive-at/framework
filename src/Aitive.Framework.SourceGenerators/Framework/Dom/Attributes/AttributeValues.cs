using Microsoft.CodeAnalysis;

namespace Aitive.Framework.SourceGenerators.Framework.Dom.Attributes;

/// <summary>
/// Strongly-typed access to attribute values.
/// </summary>
public sealed class AttributeValues
{
    private readonly Dictionary<string, object?> _values;
    private readonly Dictionary<string, ITypeSymbol?> _typeParams;

    /// <summary>
    /// The name of the constructor that was matched (for multi-constructor attributes).
    /// </summary>
    public string? ConstructorName { get; }

    internal AttributeValues(
        Dictionary<string, object?> values,
        Dictionary<string, ITypeSymbol?> typeParams,
        string? ctorName = null
    )
    {
        _values = values;
        _typeParams = typeParams;
        ConstructorName = ctorName;
    }

    // ===== Optional getters (return null/default if missing) =====

    /// <summary>Gets a primitive value. Returns default if not present.</summary>
    public T? Get<T>(string name) => _values.TryGetValue(name, out var v) && v is T t ? t : default;

    /// <summary>Gets a Type parameter as INamedTypeSymbol.</summary>
    public INamedTypeSymbol? GetTypeSymbol(string name) =>
        _values.TryGetValue(name, out var v) ? v as INamedTypeSymbol : null;

    /// <summary>Gets a generic type argument (e.g., "T" from Handler&lt;T&gt;).</summary>
    public ITypeSymbol? GetTypeArgument(string name) =>
        _typeParams.TryGetValue(name, out var v) ? v : null;

    /// <summary>Gets an enum value wrapper.</summary>
    public EnumValue? GetEnumValue(string name) =>
        _values.TryGetValue(name, out var v) ? v as EnumValue : null;

    /// <summary>Gets an enum as CLR type T.</summary>
    public T GetEnum<T>(string name)
        where T : struct, Enum
    {
        var ev = GetEnumValue(name);
        if (ev?.UnderlyingValue == null)
        {
            return default;
        }

        return (T)Enum.ToObject(typeof(T), ev.UnderlyingValue);
    }

    /// <summary>Gets enum member name.</summary>
    public string? GetEnumMemberName(string name) => GetEnumValue(name)?.MemberName;

    /// <summary>Gets an array of primitives.</summary>
    public T[]? GetArray<T>(string name)
    {
        if (!_values.TryGetValue(name, out var v) || v is not object?[] arr)
        {
            return null;
        }

        return arr.OfType<T>().ToArray();
    }

    /// <summary>Gets an array of Type symbols.</summary>
    public INamedTypeSymbol[]? GetTypeSymbolArray(string name)
    {
        if (!_values.TryGetValue(name, out var v) || v is not object?[] arr)
        {
            return null;
        }

        return arr.OfType<INamedTypeSymbol>().ToArray();
    }

    /// <summary>Gets the raw value.</summary>
    public object? GetRaw(string name) => _values.TryGetValue(name, out var v) ? v : null;

    /// <summary>All generic type arguments.</summary>
    public IReadOnlyDictionary<string, ITypeSymbol?> TypeArguments => _typeParams;

    /// <summary>Checks if a value exists and is not null.</summary>
    public bool Has(string name) => _values.TryGetValue(name, out var v) && v != null;

    // ===== Required getters (throw if missing) =====

    /// <summary>Gets a primitive value. Throws if not present or wrong type.</summary>
    public T GetRequired<T>(string name)
    {
        if (!_values.TryGetValue(name, out var v))
        {
            throw new InvalidOperationException(
                $"Required attribute value '{name}' was not found."
            );
        }

        if (v is not T t)
        {
            throw new InvalidOperationException(
                $"Attribute value '{name}' is null or not of type {typeof(T).Name}."
            );
        }

        return t;
    }

    /// <summary>Gets a Type symbol. Throws if not present.</summary>
    public INamedTypeSymbol GetTypeSymbolRequired(string name) =>
        GetTypeSymbol(name)
        ?? throw new InvalidOperationException($"Required type symbol '{name}' was not found.");

    /// <summary>Gets a generic type argument. Throws if not present.</summary>
    public ITypeSymbol GetTypeArgumentRequired(string name) =>
        GetTypeArgument(name)
        ?? throw new InvalidOperationException($"Required type argument '{name}' was not found.");

    /// <summary>Gets an enum value. Throws if not present.</summary>
    public EnumValue GetEnumValueRequired(string name) =>
        GetEnumValue(name)
        ?? throw new InvalidOperationException($"Required enum value '{name}' was not found.");

    /// <summary>Gets an enum as CLR type. Throws if not present.</summary>
    public T GetEnumRequired<T>(string name)
        where T : struct, Enum
    {
        var ev = GetEnumValueRequired(name);
        if (ev.UnderlyingValue == null)
        {
            throw new InvalidOperationException($"Enum value '{name}' has null underlying value.");
        }

        return (T)Enum.ToObject(typeof(T), ev.UnderlyingValue);
    }

    /// <summary>Gets enum member name. Throws if not present.</summary>
    public string GetEnumMemberNameRequired(string name) =>
        GetEnumMemberName(name)
        ?? throw new InvalidOperationException($"Required enum member '{name}' was not found.");

    /// <summary>Gets an array. Throws if not present.</summary>
    public T[] GetArrayRequired<T>(string name) =>
        GetArray<T>(name)
        ?? throw new InvalidOperationException($"Required array '{name}' was not found.");

    /// <summary>Gets a Type symbol array. Throws if not present.</summary>
    public INamedTypeSymbol[] GetTypeSymbolArrayRequired(string name) =>
        GetTypeSymbolArray(name)
        ?? throw new InvalidOperationException($"Required type array '{name}' was not found.");

    /// <summary>Gets raw value. Throws if not present.</summary>
    public object GetRawRequired(string name) =>
        GetRaw(name)
        ?? throw new InvalidOperationException($"Required value '{name}' was not found.");
}
