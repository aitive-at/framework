using Aitive.Framework.AspNetCore.Tenancy.Routers;
using Microsoft.Extensions.DependencyInjection;

namespace Aitive.Framework.AspNetCore.Tenancy.Builders;

internal sealed class DefaultTenancyPathPrefixRoutingServiceBuilder(IServiceCollection services)
    : ITenancyPathPrefixRoutingServiceBuilder
{
    public ITenancyPathPrefixRoutingServiceBuilder WithPathPrefixProvider<T>()
        where T : class, ITenantHttpPathPrefixProvider
    {
        services.AddSingleton<ITenantHttpPathPrefixProvider, T>();
        return this;
    }

    public ITenancyPathPrefixRoutingServiceBuilder WithPathPrefixProvider<T>(
        Func<IServiceProvider, T> pathPrefixProviderFactory
    )
        where T : ITenantHttpPathPrefixProvider
    {
        services.AddSingleton<ITenantHttpPathPrefixProvider>(sp =>
            pathPrefixProviderFactory.Invoke(sp)
        );
        return this;
    }
}
