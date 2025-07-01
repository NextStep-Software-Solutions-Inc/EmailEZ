namespace EmailEZ.Application.Interfaces;

/// <summary>
/// Provides access to the current tenant's context within the application.
/// </summary>
public interface ITenantContext
{
    /// <summary>
    /// Gets the unique identifier of the current tenant.
    /// </summary>
    Guid? TenantId { get; }

    /// <summary>
    /// Gets the domain associated with the current tenant.
    /// </summary>
    string? Domain { get; }

    /// <summary>
    /// Indicates if a tenant has been successfully identified for the current request.
    /// </summary>
    bool IsTenantSet { get; }

    /// <summary>
    /// Sets the tenant context. This method is typically called by middleware.
    /// </summary>
    /// <param name="tenantId">The ID of the identified tenant.</param>
    /// <param name="domain">The domain of the identified tenant.</param>
    void SetTenant(Guid tenantId, string? domain = default);
}