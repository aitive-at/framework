using Aitive.Framework.Collections;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Hosting;

namespace Aitive.Framework.AspNetCore;

public interface IHttpModule
{
    void Register(
        IServiceProvider services,
        IWebHostEnvironment host,
        IApplicationBuilder app,
        IEndpointRouteBuilder routes
    );
}
