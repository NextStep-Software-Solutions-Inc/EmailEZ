using EmailEZ.Application.Interfaces;
using EmailEZ.Domain.Enums;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace EmailEZ.Infrastructure.Services.Security;

public class CurrentUserService : ICurrentUserService
{
    private readonly IWorkspaceContext _workspaceContext; // Inject the workspace context
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IApplicationDbContext _applicationDbContext;

    public CurrentUserService(IWorkspaceContext workspaceContext, IHttpContextAccessor httpContextAccessor, IApplicationDbContext applicationDbContext  )
    {
        _workspaceContext = workspaceContext;
        _httpContextAccessor = httpContextAccessor;
        _applicationDbContext = applicationDbContext;
    }

    public string? GetCurrentUserId()
    {
        var userId = _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!string.IsNullOrEmpty(userId))
        {
            return userId;
        }

        if (_workspaceContext.WorkspaceId.HasValue)
        {
            return _workspaceContext.WorkspaceId.Value.ToString();
        }

        return "System";
    }

    public Guid? GetCurrentWorkspaceId()
    {
        if (_workspaceContext.WorkspaceId.HasValue)
        {
            return _workspaceContext.WorkspaceId;
        }

        return null;
    }
    /// <summary>
    /// Validates if the current user has access to the specified workspace.
    /// </summary>
    /// <param name="workspaceId">The ID of the workspace to check access for.</param>
    /// <param name="requiredRole">Optional. The minimum role required for access. Defaults to any role.</param>
    /// <returns>True if the user has access with the required role, otherwise false.</returns>
    public bool ValidateWorkspaceAccess(Guid workspaceId, WorkspaceUserRole? requiredRole = null)
    {
        var currentUserId = GetCurrentUserId();
        if (string.IsNullOrEmpty(currentUserId))
            return false;

        // Find the user's membership in the workspace
        var membership = _applicationDbContext.WorkspaceUsers
            .FirstOrDefault(wu => wu.WorkspaceId == workspaceId && wu.UserId == currentUserId);

        // No membership means no access
        if (membership == null)
            return false;

        // If no specific role is required, any membership grants access
        if (requiredRole == null)
            return true;

        // Otherwise, check if the user's role meets or exceeds the required role
        return HasSufficientRole(membership.Role, requiredRole.Value);
    }

    /// <summary>
    /// Determines if the user's role is sufficient for the required role.
    /// </summary>
    /// <param name="userRole">The user's current role.</param>
    /// <param name="requiredRole">The minimum role required.</param>
    /// <returns>True if the user's role is sufficient, otherwise false.</returns>
    private bool HasSufficientRole(WorkspaceUserRole userRole, WorkspaceUserRole requiredRole)
    {
        // Owner has all permissions
        if (userRole == WorkspaceUserRole.Owner)
            return true;

        // Admin has all permissions except owner-specific ones
        if (userRole == WorkspaceUserRole.Admin && requiredRole != WorkspaceUserRole.Owner)
            return true;

        // Member has only member-level permissions
        if (userRole == WorkspaceUserRole.Member && requiredRole == WorkspaceUserRole.Member)
            return true;

        // Default: insufficient permissions
        return false;
    }
}