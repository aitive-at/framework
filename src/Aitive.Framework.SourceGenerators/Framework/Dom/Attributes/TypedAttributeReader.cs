using System.Reflection;
using Microsoft.CodeAnalysis;

namespace Aitive.Framework.SourceGenerators.Framework.Dom.Attributes;

/// <summary>
/// Reads AttributeData into a strongly-typed instance of the spec class.
/// </summary>wd
public sealed class TypedAttributeReader<T>
    where T : class
{
    private readonly AttributeDefinition _definition;
    private readonly Type _type;
    private readonly ConstructorInfo[] _constructors;

    public TypedAttributeReader(AttributeDefinition definition)
    {
        _definition = definition;
        _type = typeof(T);
        _constructors = _type.GetConstructors(BindingFlags.Public | BindingFlags.Instance);
    }

    /// <summary>
    /// Reads AttributeData into an instance of T.
    /// Properties marked with [TypeSymbol] will contain INamedTypeSymbol.
    /// Properties marked with [EnumSymbol] will contain EnumValue.
    /// </summary>
    public T Read(AttributeData data)
    {
        var values = _definition.CreateReader().Read(data);
        return PopulateInstance(values);
    }

    /// <summary>
    /// Reads AttributeData, also providing the raw AttributeValues for additional access.
    /// </summary>
    public (T Instance, AttributeValues Values) ReadWithValues(AttributeData data)
    {
        var values = _definition.CreateReader().Read(data);
        return (PopulateInstance(values), values);
    }

    private T PopulateInstance(AttributeValues values)
    {
        // Find matching constructor
        var ctor = FindMatchingConstructor(values);
        var ctorParams = ctor.GetParameters();

        // Build constructor arguments
        var args = new object?[ctorParams.Length];
        for (int i = 0; i < ctorParams.Length; i++)
        {
            var param = ctorParams[i];
            var propName = ToPascalCase(param.Name!);
            args[i] = GetValue(values, propName, param.ParameterType, param);
        }

        // Create instance
        var instance = (T)ctor.Invoke(args);

        // Set properties
        foreach (var prop in _type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
            if (prop.SetMethod == null || !prop.SetMethod.IsPublic)
            {
                continue;
            }

            // Skip constructor parameters
            if (
                ctorParams.Any(p =>
                    string.Equals(p.Name, prop.Name, StringComparison.OrdinalIgnoreCase)
                )
            )
                continue;

            var value = GetValue(values, prop.Name, prop.PropertyType, prop);
            if (value != null || IsNullable(prop.PropertyType))
            {
                prop.SetValue(instance, value);
            }
        }

        return instance;
    }

    private ConstructorInfo FindMatchingConstructor(AttributeValues values)
    {
        if (_constructors.Length == 1)
        {
            return _constructors[0];
        }

        // Match by constructor name if available
        if (values.ConstructorName != null)
        {
            var named = _constructors.FirstOrDefault(c =>
                c.GetCustomAttribute<ConstructorNameAttribute>()?.Name == values.ConstructorName
            );
            if (named != null)
            {
                return named;
            }
        }

        // Fall back to first
        return _constructors[0];
    }

    private object? GetValue(
        AttributeValues values,
        string name,
        Type targetType,
        ICustomAttributeProvider member
    )
    {
        // TypeSymbol -> INamedTypeSymbol
        if (member.GetCustomAttributes(typeof(TypeSymbolAttribute), false).Length > 0)
        {
            return values.GetTypeSymbol(name);
        }

        // EnumSymbol -> EnumValue
        if (member.GetCustomAttributes(typeof(EnumSymbolAttribute), false).Length > 0)
        {
            return values.GetEnumValue(name);
        }

        // Regular types
        var underlying = Nullable.GetUnderlyingType(targetType) ?? targetType;

        if (underlying == typeof(string))
        {
            return values.Get<string>(name);
        }

        if (underlying == typeof(int))
        {
            return values.Get<int>(name);
        }

        if (underlying == typeof(long))
        {
            return values.Get<long>(name);
        }

        if (underlying == typeof(bool))
        {
            return values.Get<bool>(name);
        }

        if (underlying == typeof(double))
        {
            return values.Get<double>(name);
        }

        if (underlying == typeof(float))
        {
            return values.Get<float>(name);
        }

        if (underlying == typeof(char))
        {
            return values.Get<char>(name);
        }

        if (underlying == typeof(byte))
        {
            return values.Get<byte>(name);
        }

        if (underlying.IsEnum)
        {
            var ev = values.GetEnumValue(name);
            if (ev?.UnderlyingValue != null)
            {
                return Enum.ToObject(underlying, ev.UnderlyingValue);
            }

            return null;
        }

        if (targetType.IsArray)
        {
            var elemType = targetType.GetElementType()!;
            if (elemType == typeof(string))
            {
                return values.GetArray<string>(name);
            }

            if (elemType == typeof(int))
            {
                return values.GetArray<int>(name);
            }
            // Add more as needed
        }

        return values.GetRaw(name);
    }

    private static string ToPascalCase(string name) =>
        string.IsNullOrEmpty(name) ? name : char.ToUpperInvariant(name[0]) + name.Substring(1);

    private static bool IsNullable(Type type) =>
        !type.IsValueType || Nullable.GetUnderlyingType(type) != null;
}
