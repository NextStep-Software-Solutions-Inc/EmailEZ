using EmailEZ.Application.Services;
using EmailEZ.Domain.Entities;
using EmailEZ.Domain.Enums;

namespace EmailEZ.Application.Interfaces;

/// <summary>
/// Interface for workspace management service operations.
/// </summary>
public interface IWorkspaceManagementService
{

    /// <summary>
    /// Retrieves all workspaces for the current user.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A list of workspaces.</returns>
    Task<List<Workspace>> GetAllWorkspacesForCurrentUserAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a new workspace with an owner.
    /// </summary>
    /// <param name="workspaceName">The name of the workspace.</param>
    /// <param name="domain">The domain of the workspace.</param>
    /// <param name="ownerId">The ID of the owner.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The created workspace with API key.</returns>
    Task<WorkspaceCreationResult> CreateWorkspaceWithOwnerAsync(
        string workspaceName, 
        string domain, 
        string ownerId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates a workspace's basic information.
    /// </summary>
    /// <param name="workspaceId">The workspace ID.</param>
    /// <param name="name">The new name.</param>
    /// <param name="domain">The new domain.</param>
    /// <param name="isActive">The new active status.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The updated workspace.</returns>
    Task<Workspace> UpdateWorkspaceAsync(
        Guid workspaceId,
        string name,
        string domain,
        bool isActive,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a workspace.
    /// </summary>
    /// <param name="workspaceId">The workspace ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The deleted workspace.</returns>
    Task<Workspace> DeleteWorkspaceAsync(
        Guid workspaceId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a user to a workspace with a specific role.
    /// </summary>
    /// <param name="workspaceId">The workspace ID.</param>
    /// <param name="userId">The user ID.</param>
    /// <param name="role">The role to assign.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The created workspace user.</returns>
    Task<WorkspaceUser> AddUserToWorkspaceAsync(
        Guid workspaceId, 
        string userId, 
        WorkspaceUserRole role,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates a user's role in a workspace.
    /// </summary>
    /// <param name="workspaceId">The workspace ID.</param>
    /// <param name="userId">The user ID.</param>
    /// <param name="newRole">The new role.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The updated workspace user.</returns>
    Task<WorkspaceUser> UpdateUserRoleAsync(
        Guid workspaceId,
        string userId,
        WorkspaceUserRole newRole,
        CancellationToken cancellationToken = default);
}
