using System.Collections.Frozen;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.Runtime.CompilerServices;
using Aitive.Framework.Functional;

namespace Aitive.Framework.Data.Operations;

public interface IQueryResult : IResult
{
    long TotalCount { get; }
}

public interface IQueryResult<T, TCursor, TError> : IQueryResult, IResult<IReadOnlyList<T>, TError>
{
    Optional<TCursor> Next { get; }
}

public interface IQueryResult<T, TCursor, TError, TSelf>
    : IQueryResult<T, TCursor, TError>,
        IResult<IReadOnlyList<T>, TError, TSelf>
    where TSelf : IQueryResult<T, TCursor, TError, TSelf> { }

public readonly struct QueryResult<T, TCursor, TError>
    : IQueryResult<T, TCursor, TError, QueryResult<T, TCursor, TError>>
{
    private readonly Optional<IReadOnlyList<T>> _value;
    private readonly Optional<TError> _error;
    private readonly long _totalCount;
    private readonly Optional<TCursor> _next;

    public static implicit operator QueryResult<T, TCursor, TError>(TError error)
    {
        return new(error);
    }

    public static bool operator true(in QueryResult<T, TCursor, TError> x) => x.WasSuccessful;

    public static bool operator false(in QueryResult<T, TCursor, TError> x) => x.HasFailed;

    public static bool operator !(in QueryResult<T, TCursor, TError> x) => x.HasFailed;

    public QueryResult(IReadOnlyList<T> value, long totalCount, Optional<TCursor> next)
    {
        _value = Optional.Some(value);
        _error = Optional<TError>.None;
        _totalCount = totalCount;
        _next = next;
    }

    public QueryResult(TError error)
    {
        _value = Optional<IReadOnlyList<T>>.None;
        _error = error;
        _next = Optional.None<TCursor>();
        _totalCount = 0L;
    }

    public bool WasSuccessful => _value.HasValue;
    public bool HasFailed => _error.HasValue;

    public Type ValueType => typeof(T);
    public Type ErrorType => typeof(TError);
    public IReadOnlyList<T> Value => _value.Value;
    public TError Error => _error.Value;

    public long TotalCount => _totalCount;

    public Optional<TCursor> Next => _next;

    object IResult.Value => Value;

    object IResult.Error => Error!;

    public Optional<IReadOnlyList<T>> AsOptional()
    {
        return _value;
    }

    public IReadOnlyList<T> Or(T value)
    {
        return Or([value]);
    }

    public IReadOnlyList<T> Or(IReadOnlyList<T> defaultValue)
    {
        if (HasFailed)
        {
            return defaultValue;
        }

        return _value.Value;
    }

    public IReadOnlyList<T> OrThrow(Func<TError, Exception> exceptionFactory)
    {
        if (HasFailed)
        {
            throw exceptionFactory.Invoke(_error.Value);
        }

        return _value.Value;
    }

    public QueryResult<T, TCursor, TMappedError> Select<TMappedError>(
        Func<TError, TMappedError> mappingFunction
    )
    {
        if (HasFailed)
        {
            return new QueryResult<T, TCursor, TMappedError>(mappingFunction.Invoke(Error));
        }

        return new QueryResult<T, TCursor, TMappedError>(_value.Value, _totalCount, _next);
    }

    public QueryResult<TMapped, TCursor, TError> Select<TMapped>(Func<T, TMapped> mappingFunction)
    {
        if (HasFailed)
        {
            return new QueryResult<TMapped, TCursor, TError>(Error);
        }

        return new QueryResult<TMapped, TCursor, TError>(
            Value.Select(mappingFunction).ToList(),
            _totalCount,
            _next
        );
    }

    public async ValueTask<QueryResult<TMapped, TCursor, TError>> Select<TMapped>(
        Func<T, ValueTask<TMapped>> mappingFunction
    )
    {
        if (WasSuccessful)
        {
            var result = new List<TMapped>(Value.Count);

            foreach (var item in Value)
            {
                result.Add(await mappingFunction.Invoke(item));
            }

            return new QueryResult<TMapped, TCursor, TError>(result, _totalCount, _next);
        }
        else
        {
            return new QueryResult<TMapped, TCursor, TError>(Error);
        }
    }
}
