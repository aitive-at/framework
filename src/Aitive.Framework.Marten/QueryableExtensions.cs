using Aitive.Framework.Functional;
using Marten;

namespace Aitive.Framework.Marten;

public static class QueryableExtensions
{
    extension<T>(IQueryable<T> queryable)
        where T : notnull
    {
        public async Task<Optional<T>> SingleOrNone(CancellationToken cancellationToken = default)
        {
            var result = await queryable.SingleOrDefaultAsync(cancellationToken);

            return result != null ? result : Optional.None<T>();
        }

        public async Task<Optional<T>> FirstOrNone(CancellationToken cancellationToken = default)
        {
            var result = await queryable.FirstOrDefaultAsync(cancellationToken);
            return result != null ? result : Optional.None<T>();
        }
    }
}
