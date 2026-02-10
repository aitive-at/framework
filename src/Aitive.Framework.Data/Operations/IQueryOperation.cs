using Aitive.Framework.Functional;

namespace Aitive.Framework.Data.Operations;

public interface IQueryOperation<T, TQuery, TCursor, TError>
{
    Task<QueryResult<T, TCursor, TError>> Query(
        TQuery query,
        Optional<TCursor> start,
        long limit = Limits.Default,
        CancellationToken cancellationToken = default
    );
}

public static class QueryOperationExtensions
{
    extension<T, TCursor, TError>(IQueryOperation<T, Unit, TCursor, TError> queryOperation)
    {
        public Task<QueryResult<T, TCursor, TError>> List(
            Optional<TCursor> start,
            long limit = Limits.Default,
            CancellationToken cancellationToken = default
        )
        {
            return queryOperation.Query(Unit.Default, start, limit, cancellationToken);
        }
    }
}
