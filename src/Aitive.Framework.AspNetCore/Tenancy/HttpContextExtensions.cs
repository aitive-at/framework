using Aitive.Framework.Functional;
using Aitive.Framework.Tenancy;
using Microsoft.AspNetCore.Http;

namespace Aitive.Framework.AspNetCore.Tenancy;

public static class HttpContextExtensions
{
    public const string TenantKey = "_Tenant";

    extension(HttpContext httpContext)
    {
        public Optional<ITenant> Tenant
        {
            get =>
                httpContext.Items.TryGetValue(TenantKey, out var tenant) && tenant != null
                    ? Optional.Some((ITenant)tenant)
                    : Optional.None<ITenant>();
            set
            {
                if (value)
                {
                    httpContext.Items[TenantKey] = value;
                }
                else
                {
                    httpContext.Items.Remove(TenantKey);
                }
            }
        }
    }
}
