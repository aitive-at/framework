using Microsoft.Extensions.DependencyInjection;

namespace Aitive.Framework.AspNetCore.Tenancy.Builders;

internal sealed class DefaultTenancyServiceBuilder(IServiceCollection services)
    : ITenancyServiceBuilder
{
    public IServiceCollection Services { get; } = services;

    public ITenancyServiceBuilder WithRouting<T>()
        where T : class, ITenantHttpRouter
    {
        Services.AddSingleton<ITenantHttpRouter, T>();
        return this;
    }

    public ITenancyServiceBuilder WithRouting<T>(Func<IServiceProvider, T> routingFactory)
        where T : ITenantHttpRouter
    {
        Services.AddSingleton<ITenantHttpRouter>(sp => routingFactory.Invoke(sp));
        return this;
    }
}
