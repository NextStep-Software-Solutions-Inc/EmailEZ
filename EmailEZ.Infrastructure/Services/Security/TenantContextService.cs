using EmailEZ.Application.Interfaces; // For ITenantContext

namespace EmailEZ.Infrastructure.Services.Security;

// Implements ITenantContext, stores tenant info for the current request
public class TenantContextService : ITenantContext
{
    public Guid TenantId { get; private set; }
    public string Domain { get; private set; } = string.Empty;
    public bool IsTenantSet { get; private set; }

    public void SetTenant(Guid tenantId, string domain)
    {
        TenantId = tenantId;
        Domain = domain;
        IsTenantSet = true;
    }
}