using Aitive.Framework.AspNetCore.Tenancy.Builders;
using Aitive.Framework.AspNetCore.Tenancy.Routers;
using Microsoft.Extensions.DependencyInjection;

namespace Aitive.Framework.AspNetCore.Tenancy;

public interface ITenancyServiceBuilder
{
    IServiceCollection Services { get; }

    ITenancyServiceBuilder WithRouting<T>()
        where T : class, ITenantHttpRouter;

    ITenancyServiceBuilder WithRouting<T>(Func<IServiceProvider, T> routingFactory)
        where T : ITenantHttpRouter;
}

public interface ITenancyPathPrefixRoutingServiceBuilder
{
    ITenancyPathPrefixRoutingServiceBuilder WithPathPrefixProvider<T>()
        where T : class, ITenantHttpPathPrefixProvider;

    ITenancyPathPrefixRoutingServiceBuilder WithPathPrefixProvider<T>(
        Func<IServiceProvider, T> pathPrefixProviderFactory
    )
        where T : ITenantHttpPathPrefixProvider;
}

public static class TenancyServiceBuilderExtensions
{
    extension(ITenancyServiceBuilder tenancyServiceBuilder)
    {
        public ITenancyPathPrefixRoutingServiceBuilder WithPathPrefixRouting()
        {
            tenancyServiceBuilder.WithRouting(sp => new PathPrefixTenantHttpRouter(
                sp.GetRequiredService<ITenantHttpPathPrefixProvider>()
            ));

            return new DefaultTenancyPathPrefixRoutingServiceBuilder(
                tenancyServiceBuilder.Services
            );
        }
    }
}
