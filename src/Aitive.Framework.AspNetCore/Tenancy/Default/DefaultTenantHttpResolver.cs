using Aitive.Framework.Functional;
using Aitive.Framework.Patterns.Disposal;
using Aitive.Framework.Tenancy;

namespace Aitive.Framework.AspNetCore.Tenancy.Default;

internal sealed class DefaultTenantHttpResolver : ITenantResolver
{
    private readonly AsyncLocal<Optional<ITenantHttpPipeline>> _tenants;

    internal DefaultTenantHttpResolver()
    {
        _tenants = new AsyncLocal<Optional<ITenantHttpPipeline>>();
    }

    internal IDisposable SetTenantPipeline(ITenantHttpPipeline pipeline)
    {
        _tenants.Value = Optional.Some(pipeline);
        return new ActionDisposable(() => _tenants.Value = Optional.None<ITenantHttpPipeline>());
    }

    public ValueTask<Optional<ITenant>> Resolve(CancellationToken cancellationToken = default)
    {
        return _tenants.Value.HasValue
            ? ValueTask.FromResult(_tenants.Value.Select(c => (ITenant)c))
            : ValueTask.FromResult(Optional.None<ITenant>());
    }
}
