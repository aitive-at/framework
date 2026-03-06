using Aitive.Framework.Application;
using Aitive.Framework.Application.Hosting;
using Microsoft.AspNetCore.Builder;

namespace Aitive.Framework.AspNetCore.Hosting;

public abstract class WebApplication
    : Application<WebApplicationBuilder, Microsoft.AspNetCore.Builder.WebApplication>
{
    protected WebApplication(ApplicationOptions options, IApplicationDescription description)
        : base(options, description) { }
}
