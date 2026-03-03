using Aitive.Framework.Collections;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Hosting;

namespace Aitive.Framework.AspNetCore;

public interface IHttpModule : IOrdered
{
    void Register(IServiceProvider services, IApplicationBuilder app, IEndpointRouteBuilder routes);
}
