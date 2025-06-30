using EmailEZ.Application.Interfaces;

namespace EmailEZ.Infrastructure.Services.Security;

public class CurrentUserService : ICurrentUserService
{
    private readonly ITenantContext _tenantContext; // Inject the tenant context

    public CurrentUserService(ITenantContext tenantContext)
    {
        _tenantContext = tenantContext;
    }

    public Guid? GetCurrentUserId()
    {
        // If a tenant is set via API Key middleware, return the TenantId
        if (_tenantContext.IsTenantSet)
        {
            return _tenantContext.TenantId;
        }

        // For system-level operations (e.g., Hangfire jobs not tied to a specific API call)
        // you might return a predefined system GUID, or null.
        // For now, let's return null if no tenant context is available.
        return null;
    }

    public Guid? GetCurrentTenantId()
    {
        // If a tenant is set via API Key middleware, return the TenantId
        if (_tenantContext.IsTenantSet)
        {
            return _tenantContext.TenantId;
        }

        // For system-level operations (e.g., Hangfire jobs not tied to a specific API call)
        // you might return a predefined system GUID, or null.
        // For now, let's return null if no tenant context is available.
        return null;
    }
}