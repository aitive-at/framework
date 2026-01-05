using System.Reflection;
using Microsoft.CodeAnalysis;

namespace Aitive.Framework.SourceGenerators.Framework.Dom.Attributes;

/// <summary>
/// Provides reflection-based creation of AttributeSpec from a class definition,
/// and strongly-typed reading of AttributeData into instances of that class.
/// </summary>
public static class AttributeDefinitionBuilder
{
    /// <summary>
    /// Creates an AttributeSpec from a class type decorated with [GeneratedAttribute].
    /// </summary>
    public static AttributeDefinition From<T>()
        where T : class => From(typeof(T));

    /// <summary>
    /// Creates an AttributeSpec from a class type decorated with [GeneratedAttribute].
    /// </summary>
    public static AttributeDefinition From(Type type)
    {
        var genAttr =
            type.GetCustomAttribute<GeneratedAttributeAttribute>()
            ?? throw new InvalidOperationException(
                $"Type {type.Name} must have [GeneratedAttribute]"
            );

        // Determine attribute name (remove "Spec" suffix if present)
        var name = type.Name;
        if (name.EndsWith("Definition"))
        {
            name = name.Substring(0, name.Length - "Definition".Length);
        }

        if (!name.EndsWith("Attribute"))
        {
            name += "Attribute";
        }

        var spec = new AttributeDefinition(name)
            .WithTargets(genAttr.Targets)
            .WithAllowMultiple(genAttr.AllowMultiple)
            .WithInherited(genAttr.Inherited);

        if (genAttr.Namespace != null)
        {
            spec.WithNamespace(genAttr.Namespace);
        }

        // Add type parameters
        foreach (var tpAttr in type.GetCustomAttributes<TypeParameterAttribute>())
        {
            spec.WithTypeParameter(
                tpAttr.Name,
                tp =>
                {
                    if (tpAttr.IsClass)
                    {
                        tp.Class();
                    }

                    if (tpAttr.IsStruct)
                    {
                        tp.Struct();
                    }

                    if (tpAttr.HasNew)
                    {
                        tp.New();
                    }

                    if (tpAttr.IsNotNull)
                    {
                        tp.NotNull();
                    }

                    if (tpAttr.IsUnmanaged)
                    {
                        tp.Unmanaged();
                    }

                    if (tpAttr.BaseTypes != null)
                    {
                        foreach (var bt in tpAttr.BaseTypes)
                        {
                            tp.Is(bt);
                        }
                    }
                }
            );
        }

        // Get constructors (excluding parameterless if others exist)
        var constructors = type.GetConstructors(BindingFlags.Public | BindingFlags.Instance)
            .Where(c => c.GetParameters().Length > 0 || type.GetConstructors().Length == 1)
            .OrderBy(c => c.GetCustomAttribute<ConstructorNameAttribute>()?.Name ?? "")
            .ToList();

        foreach (var ctor in constructors)
        {
            var ctorNameAttr = ctor.GetCustomAttribute<ConstructorNameAttribute>();
            var ctorName = ctorNameAttr?.Name;

            spec.WithConstructor(
                ctorName,
                cs =>
                {
                    foreach (var param in ctor.GetParameters())
                    {
                        var typeRef = GetTypeRef(param.ParameterType, param);
                        var hasDefault = param.HasDefaultValue;
                        var defaultValue = hasDefault ? param.DefaultValue : null;
                        var isParams = param.GetCustomAttribute<ParamArrayAttribute>() != null;

                        cs.WithParameter(param.Name!, typeRef, defaultValue, isParams);
                    }
                }
            );
        }

        // Add properties (only settable ones, not constructor-backed)
        var ctorParamNames = new HashSet<string?>(
            constructors.SelectMany(c => c.GetParameters()).Select(p => p.Name?.ToLowerInvariant())
        );

        foreach (var prop in type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
            // Skip if it's backed by a constructor parameter
            if (ctorParamNames.Contains(prop.Name.ToLowerInvariant()))
            {
                continue;
            }

            if (prop.SetMethod == null || !prop.SetMethod.IsPublic)
            {
                continue;
            }

            var typeRef = GetTypeRef(prop.PropertyType, prop);
            object? defaultValue = null;

            // Try to get default from a default instance
            try
            {
                if (type.GetConstructor(Type.EmptyTypes) != null)
                {
                    var instance = Activator.CreateInstance(type);
                    defaultValue = prop.GetValue(instance);
                }
            }
            catch
            {
                /* ignore */
            }

            spec.WithProperty(prop.Name, typeRef, defaultValue);
        }

        return spec;
    }

    private static AttributeParameterType GetTypeRef(Type type, ICustomAttributeProvider member)
    {
        // Check for special markers
        // For [TypeSymbol] and [EnumSymbol], we don't treat plain 'object' as nullable
        // because these represent required constructor parameters.
        // In netstandard2.0, we can't detect NRT (object?), so we default to non-nullable.
        if (member.GetCustomAttributes(typeof(TypeSymbolAttribute), false).Length > 0)
        {
            var isNullable = Nullable.GetUnderlyingType(type) != null;
            return AttributeParameterType.Type(isNullable);
        }

        var enumAttr = member
            .GetCustomAttributes(typeof(EnumSymbolAttribute), false)
            .OfType<EnumSymbolAttribute>()
            .FirstOrDefault();
        if (enumAttr != null)
        {
            var isNullable = Nullable.GetUnderlyingType(type) != null;
            return AttributeParameterType.Enum(enumAttr.FullTypeName, isNullable);
        }

        // Handle nullable
        var underlying = Nullable.GetUnderlyingType(type);
        if (underlying != null)
        {
            return GetTypeRefCore(underlying).Nullable();
        }

        return GetTypeRefCore(type);
    }

    private static AttributeParameterType GetTypeRefCore(Type type)
    {
        if (type == typeof(string))
        {
            return AttributeParameterType.String;
        }

        if (type == typeof(int))
        {
            return AttributeParameterType.Int;
        }

        if (type == typeof(long))
        {
            return AttributeParameterType.Long;
        }

        if (type == typeof(bool))
        {
            return AttributeParameterType.Bool;
        }

        if (type == typeof(double))
        {
            return AttributeParameterType.Double;
        }

        if (type == typeof(float))
        {
            return AttributeParameterType.Float;
        }

        if (type == typeof(char))
        {
            return AttributeParameterType.Char;
        }

        if (type == typeof(byte))
        {
            return AttributeParameterType.Byte;
        }

        if (type == typeof(object))
        {
            return AttributeParameterType.Object;
        }

        if (type == typeof(Type))
        {
            return AttributeParameterType.Type();
        }

        if (type.IsArray)
        {
            return AttributeParameterType.Array(GetTypeRefCore(type.GetElementType()!));
        }

        if (type.IsEnum)
        {
            return AttributeParameterType.Enum(type.FullName!, false);
        }

        throw new NotSupportedException($"Type {type} is not supported in attribute specs");
    }

    private static bool IsNullable(Type type) =>
        !type.IsValueType || Nullable.GetUnderlyingType(type) != null;
}

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

/// <summary>
/// Extension methods for typed attribute reading.
/// </summary>
public static class AttributeDefinitionExtensions
{
    extension<T>(AttributeDefinition definition)
        where T : class
    {
        /// <summary>
        /// Creates a typed reader for the spec class T.
        /// </summary>
        public TypedAttributeReader<T> CreateTypedReader() => new(definition);

        /// <summary>
        /// Reads AttributeData directly into an instance of T.
        /// </summary>
        public T ReadAs(AttributeData data) => new TypedAttributeReader<T>(definition).Read(data);
    }
}
