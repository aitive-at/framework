using System.Reflection;
using Microsoft.CodeAnalysis;

namespace Aitive.Framework.SourceGenerators.Framework.Extensions;

public static class TypeExtensions
{
    private static readonly Dictionary<Type, string> TypeAliases = new()
    {
        { typeof(bool), "bool" },
        { typeof(byte), "byte" },
        { typeof(sbyte), "sbyte" },
        { typeof(char), "char" },
        { typeof(short), "short" },
        { typeof(ushort), "ushort" },
        { typeof(int), "int" },
        { typeof(uint), "uint" },
        { typeof(long), "long" },
        { typeof(ulong), "ulong" },
        { typeof(float), "float" },
        { typeof(double), "double" },
        { typeof(decimal), "decimal" },
        { typeof(string), "string" },
        { typeof(object), "object" },
        { typeof(void), "void" },
    };

    extension(Type type)
    {
        public string ConstructorName
        {
            get
            {
                var name = type.Name;
                var backtickIndex = name.IndexOf('`');
                return backtickIndex > 0 ? name[..backtickIndex] : name;
            }
        }

        public string FullDeclarationName => $"{type.Namespace}.{type.DeclarationName}";

        public string GlobalFullDeclarationName => $"global::{type.FullDeclarationName}";

        public string DeclarationName
        {
            get
            {
                // Handle nullable value types (Nullable<T> -> T?)
                if (Nullable.GetUnderlyingType(type) is { } underlying)
                {
                    return underlying.DeclarationName + "?";
                }

                // Handle arrays
                if (type.IsArray)
                {
                    var rank = type.GetArrayRank();
                    var commas = rank > 1 ? new string(',', rank - 1) : "";
                    return type.GetElementType()!.DeclarationName + $"[{commas}]";
                }

                // Handle built-in type aliases
                if (TypeAliases.TryGetValue(type, out var alias))
                {
                    return alias;
                }

                // Handle generics
                if (type.IsGenericType)
                {
                    var name = type.GetGenericTypeDefinition().Name;
                    var backtickIndex = name.IndexOf('`');

                    if (backtickIndex > 0)
                    {
                        name = name[..backtickIndex];
                    }

                    var args = type.GetGenericArguments().Select(t => t.DeclarationName);

                    return $"{name}<{string.Join(", ", args)}>";
                }

                return type.Name;
            }
        }

        public string ConstraintsString
        {
            get
            {
                if (!type.IsGenericType)
                {
                    return string.Empty;
                }

                var constraints = new List<string>();

                foreach (var param in type.GetGenericTypeDefinition().GetGenericArguments())
                {
                    var paramConstraints = type.ParameterConstraints;
                    if (paramConstraints.Count > 0)
                    {
                        constraints.Add(
                            $"where {param.Name} : {string.Join(", ", paramConstraints)}"
                        );
                    }
                }

                return string.Join(" ", constraints);
            }
        }

        private List<string> ParameterConstraints
        {
            get
            {
                var constraints = new List<string>();
                var attrs = type.GenericParameterAttributes;

                bool isStruct =
                    (attrs & GenericParameterAttributes.NotNullableValueTypeConstraint) != 0;
                bool isClass = (attrs & GenericParameterAttributes.ReferenceTypeConstraint) != 0;
                bool hasNew =
                    (attrs & GenericParameterAttributes.DefaultConstructorConstraint) != 0;

                // Check for 'unmanaged' (struct + IsUnmanagedAttribute)
                bool isUnmanaged =
                    isStruct
                    && type.HasAttribute("System.Runtime.CompilerServices.IsUnmanagedAttribute");

                // Check for 'notnull' (NullableAttribute with value 1, without class/struct)
                bool isNotNull = !isStruct && !isClass && type.HasNotNullConstraint;

                // Order matters in C#: class/struct/unmanaged/notnull first, then types, then new()

                if (isUnmanaged)
                {
                    constraints.Add("unmanaged");
                }
                else if (isStruct)
                {
                    constraints.Add("struct");
                }
                else if (isClass)
                {
                    // Could check for 'class?' (nullable reference type constraint) via NullableAttribute
                    bool isNullableClass = type.HasNullableReferenceConstraint;
                    constraints.Add(isNullableClass ? "class?" : "class");
                }
                else if (isNotNull)
                {
                    constraints.Add("notnull");
                }

                // Base class and interface constraints
                foreach (var constraint in type.GetGenericParameterConstraints())
                {
                    // 'struct'/'unmanaged' constraint adds System.ValueType - skip it
                    if (constraint == typeof(ValueType))
                        continue;

                    constraints.Add(constraint.DeclarationName);
                }

                // 'new()' constraint - must be last
                // 'struct' and 'unmanaged' imply new(), so don't duplicate
                if (hasNew && !isStruct && !isUnmanaged)
                {
                    constraints.Add("new()");
                }

                return constraints;
            }
        }

        private bool HasNotNullConstraint
        {
            get
            {
                // 'notnull' is encoded via NullableAttribute with value 1
                var nullableAttr = type.GetCustomAttributesData()
                    .FirstOrDefault(a =>
                        a.AttributeType.FullName
                        == "System.Runtime.CompilerServices.NullableAttribute"
                    );

                if (nullableAttr == null)
                {
                    return false;
                }

                var args = nullableAttr.ConstructorArguments;

                if (args.Count == 0)
                {
                    return false;
                }

                // Can be byte or byte[]
                if (args[0].Value is byte b)
                {
                    return b == 1;
                }

                if (args[0].Value is IReadOnlyCollection<CustomAttributeTypedArgument> arr)
                {
                    return arr.FirstOrDefault().Value is byte first && first == 1;
                }

                return false;
            }
        }

        private bool HasNullableReferenceConstraint
        {
            get
            {
                // 'class?' has NullableAttribute with value 2
                var nullableAttr = type.GetCustomAttributesData()
                    .FirstOrDefault(a =>
                        a.AttributeType.FullName
                        == "System.Runtime.CompilerServices.NullableAttribute"
                    );

                if (nullableAttr == null)
                {
                    return false;
                }

                var args = nullableAttr.ConstructorArguments;

                if (args.Count == 0)
                {
                    return false;
                }

                if (args[0].Value is byte b)
                {
                    return b == 2;
                }

                if (args[0].Value is IReadOnlyCollection<CustomAttributeTypedArgument> arr)
                {
                    return arr.FirstOrDefault().Value is byte and 2;
                }

                return false;
            }
        }

        private bool HasAttribute(string attributeFullName)
        {
            return type.GetCustomAttributesData()
                .Any(a => a.AttributeType.FullName == attributeFullName);
        }
    }
}
