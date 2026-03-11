using Aitive.Framework.Functional;
using YesSql;
using YesSql.Indexes;

namespace Aitive.Framework.YesSql;

public static class YesSqlQueryExtensions
{
    extension<T, TIndex>(IQuery<T, TIndex> query)
        where T : class
        where TIndex : IIndex
    {
        public async Task<Optional<T>> FindOrNone(CancellationToken cancellationToken = default)
        {
            var result = await query.FirstOrDefaultAsync(cancellationToken);

            if (result != null)
            {
                return result;
            }

            return Optional.None<T>();
        }
    }
}
