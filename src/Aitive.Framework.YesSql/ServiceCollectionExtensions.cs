using Microsoft.Extensions.DependencyInjection;
using YesSql;
using YesSql.Indexes;

namespace Aitive.Framework.YesSql;

public static class ServiceCollectionExtensions
{
    extension(IServiceCollection services)
    {
        public void AddYesSql()
        {
            services.AddSingleton<IStore>(sp =>
            {
                var configuration = sp.GetRequiredService<IConfiguration>();
                var store = StoreFactory.Create(configuration);
                var indices = sp.GetServices<IIndexProvider>();

                store.RegisterIndexes(indices);

                return store;
            });
        }
    }
}
