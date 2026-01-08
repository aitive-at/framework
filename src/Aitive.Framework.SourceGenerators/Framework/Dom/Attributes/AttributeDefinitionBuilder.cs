using System.Reflection;

namespace Aitive.Framework.SourceGenerators.Framework.Dom.Attributes;

/// <summary>
/// Provides reflection-based creation of AttributeSpec from a class definition,
/// and strongly-typed reading of AttributeData into instances of that class.
/// </summary>
public static class AttributeDefinitionBuilder
{
    extension(AttributeDefinition definition)
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
                constructors
                    .SelectMany(c => c.GetParameters())
                    .Select(p => p.Name?.ToLowerInvariant())
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
