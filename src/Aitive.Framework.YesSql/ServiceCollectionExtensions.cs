using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using YesSql;
using YesSql.Indexes;

namespace Aitive.Framework.YesSql;

public static class ServiceCollectionExtensions
{
    extension(IServiceCollection services)
    {
        public void AddYesSql(
            Action<IServiceProvider, global::YesSql.Configuration>? configure = null
        )
        {
            services.AddSingleton<IStore>(sp =>
            {
                var options = sp.GetRequiredService<IOptions<YesSqlOptions>>().Value;
                var configuration = new global::YesSql.Configuration();
                options.Apply(configuration);

                configure?.Invoke(sp, configuration);

                var store = StoreFactory.Create(configuration);
                var indices = sp.GetServices<IIndexProvider>();

                store.RegisterIndexes(indices);

                return store;
            });
        }
    }
}
