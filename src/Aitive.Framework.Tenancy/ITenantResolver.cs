using Aitive.Framework.Functional;

namespace Aitive.Framework.Tenancy;

public interface ITenantResolver
{
    ValueTask<Optional<ITenant>> Resolve(CancellationToken cancellationToken = default);
}
