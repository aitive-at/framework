namespace Aitive.Framework.Tenancy;

public interface ITenant
{
    TenantId Id { get; }

    IServiceProvider Services { get; }
}
