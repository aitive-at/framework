using Microsoft.Extensions.DependencyInjection;

namespace Aitive.Framework.DependencyInjection;

public interface IServiceModule
{
    void Register(IServiceCollection services);
}
