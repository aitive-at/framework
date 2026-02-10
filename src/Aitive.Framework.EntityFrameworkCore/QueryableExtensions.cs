using System.Linq.Expressions;
using Aitive.Framework.Data;
using Aitive.Framework.Data.Operations;
using Aitive.Framework.Functional;
using FastExpressionCompiler;
using Microsoft.EntityFrameworkCore;
using Z.EntityFramework.Plus;

namespace Aitive.Framework.EntityFrameworkCore;

public static class QueryableExtensions
{
    extension<T>(IQueryable<T> queryable)
    {
        public Task<QueryResult<T, TCursor, TError>> AsQueryResult<TCursor, TError>(
            Expression<Func<T, TCursor>> selector,
            TError error,
            Optional<TCursor> start = default,
            long limit = Limits.Default,
            Ordering ordering = Ordering.Ascending,
            CancellationToken cancellationToken = default
        )
        {
            return queryable.AsQueryResult<T, TCursor, TError>(
                selector,
                _ => error,
                start,
                limit,
                ordering,
                cancellationToken
            );
        }

        public async Task<QueryResult<T, TCursor, TError>> AsQueryResult<TCursor, TError>(
            Expression<Func<T, TCursor>> selector,
            Func<Exception, TError> exceptionTranslator,
            Optional<TCursor> start = default,
            long limit = Limits.Default,
            Ordering ordering = Ordering.Ascending,
            CancellationToken cancellationToken = default
        )
        {
            try
            {
                var currentQuery = queryable;

                if (start)
                {
                    currentQuery = currentQuery.Where(
                        CreateFilter(selector, start.Value, ordering)
                    );
                }

                currentQuery =
                    ordering == Ordering.Ascending
                        ? currentQuery.OrderBy(selector)
                        : currentQuery.OrderByDescending(selector);

                var localLimit = (int)limit;

                var futureTotalCount = currentQuery.DeferredLongCount().FutureValue();
                var futureItems = currentQuery.Take(localLimit + 1).Future();

                var items = await futureItems.ToListAsync(cancellationToken);
                var totalCount = await futureTotalCount.ValueAsync(cancellationToken);

                var next = Optional.None<TCursor>();

                // Determine if we have a next page
                if (items.Count > localLimit)
                {
                    var compiledSelector = selector.CompileFast();
                    next = compiledSelector.Invoke(items[localLimit]);
                }

                return new QueryResult<T, TCursor, TError>(items, totalCount, next);
            }
            catch (Exception ex)
            {
                return exceptionTranslator(ex);
            }
        }
    }

    private static Expression<Func<T, bool>> CreateFilter<T, TCursor>(
        Expression<Func<T, TCursor>> selector,
        TCursor cursor,
        Ordering ordering
    )
    {
        var param = selector.Parameters[0];
        var left = selector.Body;

        // value
        var right = Expression.Constant(cursor, typeof(TCursor));

        var comparisonType =
            ordering == Ordering.Ascending
                ? ExpressionType.GreaterThanOrEqual
                : ExpressionType.LessThanOrEqual;

        // x.Property >= value   (or <=, ==, etc. depending on comparisonType)
        var body = Expression.MakeBinary(comparisonType, left, right);

        return Expression.Lambda<Func<T, bool>>(body, param);
    }
}
