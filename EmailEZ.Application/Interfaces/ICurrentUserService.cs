namespace EmailEZ.Application.Interfaces;

/// <summary>
/// Provides information about the current user or actor performing an operation.
/// </summary>
public interface ICurrentUserService
{
    /// <summary>
    /// Gets the unique identifier of the current user or actor.
    /// This could be a TenantId, AdminUserId, or a system account ID.
    /// </summary>
    Guid? GetCurrentUserId();

    
}