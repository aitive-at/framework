using System.Collections;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Aitive.Framework.Functional;

internal static class Sentinel
{
    internal static readonly object Instance = new();
}

internal interface IOptional
{
    bool HasValue { get; }

    Type ValueType { get; }
}

public static class Optional
{
    extension<T>(IEnumerable<Optional<T>> items)
    {
        public Optional<T> FirstOrNone()
        {
            foreach (var item in items)
            {
                if (item)
                {
                    return item;
                }
            }

            return None<T>();
        }
    }

    /// <param name="task">The task returning optional value.</param>
    /// <typeparam name="T">The type of the value.</typeparam>
    extension<T>(Task<Optional<T>> task)
        where T : struct
    {
        /// <summary>
        ///     If a value is present, returns the value, otherwise <see langword="null" />.
        /// </summary>
        /// <returns>Nullable value.</returns>
        public async Task<T?> OrNull()
        {
            return (await task.ConfigureAwait(false)).OrNull();
        }
    }

    /// <param name="task">The task returning optional value.</param>
    /// <typeparam name="T">The type of the value.</typeparam>
    extension<T>(Task<Optional<T>> task)
    {
        /// <summary>
        ///     Returns the value if present; otherwise return default value.
        /// </summary>
        /// <param name="defaultValue">The value to be returned if there is no value present.</param>
        /// <returns>The value, if present, otherwise default.</returns>
        public async Task<T?> Or(T? defaultValue)
        {
            return (await task.ConfigureAwait(false)).Or(defaultValue);
        }

        /// <summary>
        ///     If a value is present, returns the value, otherwise return default value.
        /// </summary>
        /// <returns>The value, if present, otherwise default.</returns>
        public async Task<T?> OrDefault()
        {
            return (await task.ConfigureAwait(false)).ValueOrDefault;
        }
    }

    /// <param name="optionalType">The type to check.</param>
    extension(Type optionalType)
    {
        /// <summary>
        ///     Indicates that specified type is optional type.
        /// </summary>
        /// <returns><see langword="true" />, if specified type is optional type; otherwise, <see langword="false" />.</returns>
        public bool IsOptional()
        {
            return optionalType.IsConstructedGenericType
                && optionalType.GetGenericTypeDefinition() == typeof(Optional<>);
        }
    }

    /// <summary>
    ///     Returns the underlying type argument of the specified optional type.
    /// </summary>
    /// <param name="optionalType">Optional type.</param>
    /// <returns>Underlying type argument of optional type; otherwise, <see langword="null" />.</returns>
    public static Type? GetUnderlyingType(Type optionalType)
    {
        return IsOptional(optionalType) ? optionalType.GetGenericArguments()[0] : null;
    }

    /// <param name="value">The value to convert.</param>
    /// <typeparam name="T">Type of value.</typeparam>
    extension<T>(T? value)
        where T : struct
    {
        /// <summary>
        ///     Constructs optional value from nullable reference type.
        /// </summary>
        /// <returns>The value wrapped into Optional container.</returns>
        public Optional<T> ToOptional()
        {
            return value.HasValue ? Some(value.GetValueOrDefault()) : None<T>();
        }
    }

    /// <param name="value">Optional value.</param>
    /// <typeparam name="T">Value type.</typeparam>
    extension<T>(in Optional<T> value)
        where T : struct
    {
        /// <summary>
        ///     If a value is present, returns the value, otherwise <see langword="null" />.
        /// </summary>
        /// <returns>Nullable value.</returns>
        public T? OrNull()
        {
            return value.HasValue ? value.ValueOrDefault : null;
        }
    }

    /// <param name="first">The first optional value.</param>
    /// <typeparam name="T">Type of value.</typeparam>
    extension<T>(in Optional<T> first)
    {
        /// <summary>
        ///     Returns the second value if the first is empty.
        /// </summary>
        /// <param name="second">The second optional value.</param>
        /// <returns>The second value if the first is empty; otherwise, the first value.</returns>
        public ref readonly Optional<T> Coalesce(in Optional<T> second)
        {
            return ref first.HasValue ? ref first : ref second;
        }

        public Optional<TResult> OfType<TResult>()
        {
            if (first)
            {
                if (first.Value is TResult result)
                {
                    return Optional.Some(result);
                }
            }

            return None<TResult>();
        }

        public Optional<TResult> Cast<TResult>()
        {
            if (first)
            {
                if (first.Value is TResult result)
                {
                    return Optional.Some(result);
                }
                else
                {
                    throw new InvalidCastException(
                        $"Cannot cast {typeof(T).Name} to {typeof(TResult).Name}"
                    );
                }
            }

            return None<TResult>();
        }
    }

    public static object NoneUntyped(Type type)
    {
        var noneProperty = typeof(Optional<>)
            .MakeGenericType(type)
            .GetProperty("None", BindingFlags.Static | BindingFlags.Public);

        if (noneProperty == null)
        {
            throw new NotImplementedException("Cannot get None property of Optional");
        }

        return noneProperty.GetValue(null)!;
    }

    /// <summary>
    ///     Returns empty value.
    /// </summary>
    /// <typeparam name="T">The type of empty result.</typeparam>
    /// <returns>The empty value.</returns>
    public static Optional<T> None<T>()
    {
        return Optional<T>.None;
    }

    public static object SomeUntyped(object value)
    {
        return SomeUntyped(value.GetType(), value);
    }

    public static object SomeUntyped(Type type, object value)
    {
        var someMethod = typeof(Optional)
            .GetMethod("Some", BindingFlags.Static | BindingFlags.Public)
            ?.MakeGenericMethod(type);

        if (someMethod == null)
        {
            throw new NotImplementedException("Cannot get Some method of Optional");
        }

        return someMethod.Invoke(null, [value])!;
    }

    /// <summary>
    ///     Wraps the value to <see cref="Optional{T}" /> container.
    /// </summary>
    /// <param name="value">The value to be wrapped.</param>
    /// <typeparam name="T">The type of the value.</typeparam>
    /// <returns>The optional container.</returns>
    public static Optional<T> Some<T>([DisallowNull] T value)
    {
        return new Optional<T>(value);
    }

    /// <summary>
    ///     Wraps <see langword="null" /> value to <see cref="Optional{T}" /> container.
    /// </summary>
    /// <typeparam name="T">The reference type.</typeparam>
    /// <returns>The <see cref="Optional{T}" /> instance representing <see langword="null" /> value.</returns>
    public static Optional<T> Null<T>()
        where T : class?
    {
        return new Optional<T>(null);
    }

    /// <summary>
    ///     Obtains immutable reference to the value in the container.
    /// </summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    /// <param name="optional">The optional container.</param>
    /// <returns>The immutable reference to the value in the container.</returns>
    /// <exception cref="InvalidOperationException">No value is present.</exception>
    public static ref readonly T GetReference<T>(in Optional<T> optional)
        where T : struct
    {
        optional.Validate();
        return ref Optional<T>.GetReference(in optional);
    }

    /// <param name="optional">The nested optional value.</param>
    /// <typeparam name="T">The type of the underlying value.</typeparam>
    extension<T>(in Optional<Optional<T>> optional)
    {
        /// <summary>
        ///     Flattens the nested optional value.
        /// </summary>
        /// <returns>Flattened value.</returns>
        public Optional<T> Flatten()
        {
            return new Optional<T>(in optional);
        }
    }

    extension(string? value)
    {
        public Optional<T> TryParseOptional<T>(IFormatProvider? formatProvider = null)
            where T : IParsable<T>
        {
            if (T.TryParse(value, formatProvider, out var result))
            {
                return result;
            }

            return Optional<T>.None;
        }
    }

    extension<T>(Optional<T> value)
        where T : class
    {
        public T? ToNullable()
        {
            return value.HasValue ? value.Value : null;
        }
    }

    extension<T>(T? value)
        where T : class
    {
        public Optional<T> NullableAsOptional()
        {
            if (value == null)
            {
                return Optional<T>.None;
            }

            return Optional.Some<T>(value);
        }
    }
}

/// <summary>
///     A container object which may or may not contain a value.
/// </summary>
/// <typeparam name="T">Type of value.</typeparam>
[StructLayout(LayoutKind.Auto)]
public readonly struct Optional<T>
    : IEquatable<Optional<T>>,
        IEquatable<T>,
        IStructuralEquatable,
        IEnumerable<T>,
        IOptional
{
    private const byte UndefinedValue = 0;
    private const byte NullValue = 1;
    private const byte NotEmptyValue = 3;

    private static readonly bool IsOptional;

    static Optional()
    {
        var type = typeof(T);
        IsOptional =
            type.IsConstructedGenericType && type.GetGenericTypeDefinition() == typeof(Optional<>);
    }

    private readonly T? _value;
    private readonly byte _kind;

    /// <summary>
    ///     Constructs non-empty container.
    /// </summary>
    /// <param name="value">A value to be placed into container.</param>
    /// <remarks>
    ///     The property <see langword="IsNull" /> of the constructed object may be <see langword="true" />
    ///     if <paramref name="value" /> is <see langword="null" />.
    ///     The property <see langword="IsUndefined" /> of the constructed object is always <see langword="false" />.
    /// </remarks>
    public Optional(T? value)
    {
        _value = value;
        _kind =
            value is null ? NullValue
            : IsOptional ? GetKindUnsafe(ref value)
            : NotEmptyValue;
    }

    public Type ValueType => typeof(T);

    internal Optional(in Optional<Optional<T>> value)
    {
        _value = value._value._value;
        _kind = value._kind;
    }

    private static byte GetKindUnsafe([DisallowNull] ref T optionalValue)
    {
        Debug.Assert(IsOptional);

        return optionalValue.Equals(null) ? NullValue
            : optionalValue.Equals(Sentinel.Instance) ? UndefinedValue
            : NotEmptyValue;
    }

    /// <summary>
    ///     Determines whether the object represents meaningful value.
    /// </summary>
    /// <param name="value">The value to check.</param>
    /// <returns>
    ///     <see langword="true" /> if <paramref name="value" /> is not null,
    ///     or <see cref="Nullable{T}.HasValue" /> property is <see langword="true" />,
    ///     or <see cref="Optional{T}.HasValue" /> property is <see langword="true" />;
    ///     otherwise, <see langword="false" />.
    /// </returns>
    [EditorBrowsable(EditorBrowsableState.Advanced)]
    public static bool IsValueDefined([NotNullWhen(true)] T? value)
    {
        return value is not null && (!IsOptional || GetKindUnsafe(ref value) is NotEmptyValue);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static ref readonly T? GetReference(in Optional<T> optional)
    {
        return ref optional._value;
    }

    /// <summary>
    ///     Represents optional container without value.
    /// </summary>
    /// <remarks>
    ///     The property <see cref="IsUndefined" /> of returned object is always <see langword="true" />.
    /// </remarks>
    public static Optional<T> None => default;

    /// <summary>
    ///     Indicates whether the value is present.
    /// </summary>
    /// <remarks>
    ///     If this property is <see langword="true" /> then <see cref="IsUndefined" /> and <see cref="IsNull" />
    ///     equal to <see langword="false" />.
    /// </remarks>
    [MemberNotNullWhen(true, nameof(_value))]
    [MemberNotNullWhen(true, nameof(ValueOrDefault))]
    public bool HasValue => _kind is NotEmptyValue;

    /// <summary>
    ///     Indicates that the value is undefined.
    /// </summary>
    /// <seealso cref="None" />
    public bool IsUndefined => _kind is UndefinedValue;

    /// <summary>
    ///     Indicates that the value is <see langword="null" />.
    /// </summary>
    /// <remarks>
    ///     This property returns <see langword="true" /> only if this instance
    ///     was constructed using <see cref="Optional{T}(T)" /> with <see langword="null" /> argument.
    /// </remarks>
    public bool IsNull => _kind is NullValue;

    public Optional<TResult> Select<TResult>(Func<T, TResult> converter)
    {
        return HasValue ? converter.Invoke(Value) : Optional<TResult>.None;
    }

    public async Task<Optional<TResult>> Select<TResult>(Func<T, Task<TResult>> selector)
    {
        if (HasValue)
        {
            return await selector.Invoke(Value);
        }

        return Optional<TResult>.None;
    }

    public Optional<TNew> OfType<TNew>()
    {
        if (HasValue)
        {
            if (Value is TNew newValue)
            {
                return Optional.Some(newValue);
            }
        }

        return Optional<TNew>.None;
    }

    /// <summary>
    ///     Boxes value encapsulated by this object.
    /// </summary>
    /// <returns>The boxed value.</returns>
    public Optional<object> Box()
    {
        return IsUndefined ? default : new Optional<object>(_value);
    }

    /// <summary>
    ///     Attempts to extract value from container if it is present.
    /// </summary>
    /// <param name="value">Extracted value.</param>
    /// <returns><see langword="true" /> if value is present; otherwise, <see langword="false" />.</returns>
    public bool TryGet([MaybeNullWhen(false)] out T value)
    {
        value = _value;
        return HasValue;
    }

    /// <summary>
    ///     Attempts to extract value from container if it is present.
    /// </summary>
    /// <param name="value">Extracted value.</param>
    /// <param name="isNull">
    ///     <see langword="true" /> if underlying value is <see langword="null" />; otherwise,
    ///     <see langword="false" />.
    /// </param>
    /// <returns><see langword="true" /> if value is present; otherwise, <see langword="false" />.</returns>
    public bool TryGet([MaybeNullWhen(false)] out T value, out bool isNull)
    {
        value = _value!;
        switch (_kind)
        {
            default:
                isNull = false;
                return false;
            case NullValue:
                isNull = true;
                return false;
            case NotEmptyValue:
                Debug.Assert(value is not null);
                isNull = false;
                return true;
        }
    }

    public T OrThrow(Func<Exception> exceptionGenerator)
    {
        return HasValue ? _value : throw exceptionGenerator.Invoke();
    }

    public Result<T, TError> ToResult<TError>(TError error)
    {
        if (HasValue)
        {
            return new Result<T, TError>(Value);
        }

        return new Result<T, TError>(error);
    }

    /// <summary>
    ///     Returns the value if present; otherwise return default value.
    /// </summary>
    /// <param name="defaultValue">The value to be returned if there is no value present.</param>
    /// <returns>The value, if present, otherwise <paramref name="defaultValue" />.</returns>
    [return: NotNullIfNotNull(nameof(defaultValue))]
    public T? Or(T? defaultValue)
    {
        return HasValue ? _value : defaultValue;
    }

    public T? Or(Func<T?> defaultValueFactory)
    {
        return HasValue ? _value : defaultValueFactory();
    }

    /// <summary>
    ///     If a value is present, returns the value, otherwise default value.
    /// </summary>
    /// <value>The value, if present, otherwise default.</value>
    public T? ValueOrDefault => _value;

    /// <summary>
    ///     If a value is present, returns the value, otherwise throw exception.
    /// </summary>
    /// <exception cref="InvalidOperationException">No value is present.</exception>
    [DisallowNull]
    public T Value
    {
        get
        {
            Validate();
            return _value;
        }
    }

    [MemberNotNull(nameof(_value))]
    internal void Validate()
    {
        var kind = _kind;

        if (kind is NotEmptyValue)
        {
            Debug.Assert(_value is not null);
        }
        else
        {
            Throw(kind is UndefinedValue);
        }

        [StackTraceHidden]
        [DoesNotReturn]
        static void Throw(bool isUndefined)
        {
            throw new InvalidOperationException(isUndefined ? "Undefined value" : "Null value");
        }
    }

    /// <summary>
    ///     Returns textual representation of this object.
    /// </summary>
    /// <returns>The textual representation of this object.</returns>
    public override string? ToString()
    {
        return _kind switch
        {
            UndefinedValue => "<Undefined>",
            NullValue => "<Null>",
            _ => _value!.ToString(),
        };
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    /// <summary>
    ///     Computes hash code of the stored value.
    /// </summary>
    /// <returns>The hash code of the stored value.</returns>
    /// <remarks>
    ///     This method uses <see cref="EqualityComparer{T}" /> type
    ///     to get hash code of <see cref="Value" />.
    /// </remarks>
    public override int GetHashCode()
    {
        return _kind switch
        {
            UndefinedValue => 0,
            NullValue => NullValue,
            _ => EqualityComparer<T?>.Default.GetHashCode(_value!),
        };
    }

    /// <summary>
    ///     Determines whether this container stored the same
    ///     value as the specified value.
    /// </summary>
    /// <param name="other">Other value to compare.</param>
    /// <returns>
    ///     <see langword="true" /> if <see cref="Value" /> is equal to <paramref name="other" />; otherwise,
    ///     <see langword="false" />.
    /// </returns>
    public bool Equals(T? other)
    {
        return !IsUndefined && EqualityComparer<T?>.Default.Equals(_value, other);
    }

    private bool Equals(in Optional<T> other)
    {
        return _kind == other._kind
            && (
                _kind is UndefinedValue or NullValue
                || EqualityComparer<T?>.Default.Equals(_value, other._value)
            );
    }

    /// <summary>
    ///     Determines whether this container stores
    ///     the same value as other.
    /// </summary>
    /// <param name="other">Other container to compare.</param>
    /// <returns>
    ///     <see langword="true" /> if this container stores the same value as <paramref name="other" />; otherwise,
    ///     <see langword="false" />.
    /// </returns>
    public bool Equals(Optional<T> other)
    {
        return Equals(in other);
    }

    public IEnumerator<T> GetEnumerator()
    {
        if (this)
        {
            yield return Value;
        }
    }

    /// <summary>
    ///     Determines whether this container stores
    ///     the same value as other.
    /// </summary>
    /// <param name="other">Other container to compare.</param>
    /// <returns>
    ///     <see langword="true" /> if this container stores the same value as <paramref name="other" />; otherwise,
    ///     <see langword="false" />.
    /// </returns>
    public override bool Equals(object? other)
    {
        return other switch
        {
            null => IsNull,
            Optional<T> optional => Equals(in optional),
            T value => Equals(value),
            _ => ReferenceEquals(other, Sentinel.Instance) && IsUndefined,
        };
    }

    /// <summary>
    ///     Performs equality check between stored value
    ///     and the specified value using method <see cref="IEqualityComparer.Equals(object, object)" />.
    /// </summary>
    /// <param name="other">Other object to compare with <see cref="Value" />.</param>
    /// <param name="comparer">The comparer implementing custom equality check.</param>
    /// <returns>
    ///     <see langword="true" /> if <paramref name="other" /> is equal to <see cref="Value" /> using custom check;
    ///     otherwise, <see langword="false" />.
    /// </returns>
    public bool Equals(object? other, IEqualityComparer comparer)
    {
        return !IsUndefined && comparer.Equals(_value, other);
    }

    /// <summary>
    ///     Computes hash code for the stored value
    ///     using method <see cref="IEqualityComparer.GetHashCode(object)" />.
    /// </summary>
    /// <param name="comparer">The comparer implementing hash code function.</param>
    /// <returns>The hash code of <see cref="Value" />.</returns>
    public int GetHashCode(IEqualityComparer comparer)
    {
        return _kind switch
        {
            UndefinedValue => 0,
            NullValue => NullValue,
            _ => comparer.GetHashCode(_value!),
        };
    }

    /// <summary>
    ///     Wraps value into Optional container.
    /// </summary>
    /// <param name="value">The value to convert.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator Optional<T>(T? value)
    {
        return new Optional<T>(value);
    }

    /// <summary>
    ///     Extracts value stored in the Optional container.
    /// </summary>
    /// <param name="optional">The container.</param>
    /// <exception cref="InvalidOperationException">No value is present.</exception>
    public static explicit operator T(in Optional<T> optional)
    {
        return optional.Value;
    }

    /// <summary>
    ///     Determines whether two containers store the same value.
    /// </summary>
    /// <param name="first">The first container to compare.</param>
    /// <param name="second">The second container to compare.</param>
    /// <returns><see langword="true" />, if both containers store the same value; otherwise, <see langword="false" />.</returns>
    public static bool operator ==(in Optional<T> first, in Optional<T> second)
    {
        return first.Equals(in second);
    }

    /// <summary>
    ///     Determines whether two containers store the different values.
    /// </summary>
    /// <param name="first">The first container to compare.</param>
    /// <param name="second">The second container to compare.</param>
    /// <returns><see langword="true" />, if both containers store the different values; otherwise, <see langword="false" />.</returns>
    public static bool operator !=(in Optional<T> first, in Optional<T> second)
    {
        return !first.Equals(in second);
    }

    /// <summary>
    ///     Returns non-empty container.
    /// </summary>
    /// <param name="first">The first container.</param>
    /// <param name="second">The second container.</param>
    /// <returns>The first non-empty container.</returns>
    /// <seealso cref="Optional.Coalesce{T}" />
    public static Optional<T> operator |(in Optional<T> first, in Optional<T> second)
    {
        return first.HasValue ? first : second;
    }

    /// <summary>
    ///     Determines whether two containers are empty or have values.
    /// </summary>
    /// <param name="first">The first container.</param>
    /// <param name="second">The second container.</param>
    /// <returns><see cref="None" />, if both containers are empty or have values; otherwise, non-empty container.</returns>
    public static Optional<T> operator ^(in Optional<T> first, in Optional<T> second)
    {
        return (first._kind - second._kind) switch
        {
            UndefinedValue - NullValue
            or NullValue - NotEmptyValue
            or UndefinedValue - NotEmptyValue => second,
            NotEmptyValue - UndefinedValue
            or NotEmptyValue - NullValue
            or NullValue - UndefinedValue => first,
            _ => None,
        };
    }

    /// <summary>
    ///     Checks whether the container has value.
    /// </summary>
    /// <param name="optional">The container to check.</param>
    /// <returns><see langword="true" /> if this container has value; otherwise, <see langword="false" />.</returns>
    /// <see cref="HasValue" />
    [MemberNotNullWhen(true, nameof(ValueOrDefault))]
    public static bool operator true(in Optional<T> optional)
    {
        return optional.HasValue;
    }

    /// <summary>
    ///     Checks whether the container has no value.
    /// </summary>
    /// <param name="optional">The container to check.</param>
    /// <returns><see langword="true" /> if this container has no value; otherwise, <see langword="false" />.</returns>
    /// <see cref="HasValue" />
    [MemberNotNullWhen(false, nameof(ValueOrDefault))]
    public static bool operator false(in Optional<T> optional)
    {
        return optional._kind < NotEmptyValue;
    }

    public static bool operator !(in Optional<T> optional)
    {
        return !optional.HasValue;
    }
}
