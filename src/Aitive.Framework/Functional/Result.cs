namespace Aitive.Framework.Functional;

public interface IResult
{
    public bool WasSuccessful { get; }
    public bool HasFailed { get; }

    public Type ValueType { get; }

    public Type ErrorType { get; }

    public object Value { get; }

    public object Error { get; }
}

public readonly struct Result<T, TError> : IResult
{
    private readonly Optional<T> _value;
    private readonly Optional<TError> _error;

    public static implicit operator Result<T, TError>(T value)
    {
        return new(value);
    }

    public static implicit operator Result<T, TError>(TError error)
    {
        return new(error);
    }

    public static bool operator true(in Result<T, TError> x) => x.WasSuccessful;

    public static bool operator false(in Result<T, TError> x) => x.HasFailed;

    public static bool operator !(in Result<T, TError> x) => x.HasFailed;

    public Result(T value)
    {
        _value = value;
        _error = Optional<TError>.None;
    }

    public Result(TError error)
    {
        _value = Optional<T>.None;
        _error = error;
    }

    public bool WasSuccessful => _value.HasValue;
    public bool HasFailed => _error.HasValue;

    public Type ValueType => typeof(T);
    public Type ErrorType => typeof(TError);

    object IResult.Value => Value ?? throw new InvalidOperationException();
    object IResult.Error => Error ?? throw new InvalidOperationException();

    public T Value => _value.Value;
    public TError Error => _error.Value;

    public Optional<T> ToOptional() => _value;

    public T Or(T defaultValue) => WasSuccessful ? Value : defaultValue;

    public T OrThrow(Func<TError, Exception> exceptionFactory)
    {
        if (HasFailed)
        {
            throw exceptionFactory.Invoke(Error);
        }

        return Value;
    }

    public Result<T, TMappedError> Select<TMappedError>(Func<TError, TMappedError> mappingFunction)
    {
        if (HasFailed)
        {
            return mappingFunction.Invoke(Error);
        }

        return Value;
    }

    public Result<TMapped, TError> Select<TMapped>(Func<T, TMapped> mappingFunction)
    {
        if (WasSuccessful)
        {
            return mappingFunction.Invoke(Value);
        }

        return Error;
    }

    public async ValueTask<Result<TMapped, TError>> Select<TMapped>(
        Func<T, ValueTask<TMapped>> mappingFunction
    )
    {
        if (WasSuccessful)
        {
            return await mappingFunction.Invoke(Value);
        }

        return Error;
    }

    public override string ToString()
    {
        return (WasSuccessful ? Value?.ToString() : Error?.ToString()) ?? string.Empty;
    }
}

public static class Result
{
    public static bool IsResult(this Type resultType) =>
        resultType.IsConstructedGenericType
        && resultType.GetGenericTypeDefinition() == typeof(Result<,>);

    public static (Type ValueType, Type ErrorType) GetUnderlyingTypes(Type resultType)
    {
        if (!resultType.IsResult())
        {
            throw new ArgumentException("Type is not a Result<,> type");
        }

        if (!resultType.IsConstructedGenericType)
        {
            throw new ArgumentException("Type is an open generic type definition");
        }

        var arguments = resultType.GetGenericArguments();

        return (arguments[0], arguments[1]);
    }
}
