using EmailEZ.Domain.Entities;
using EmailEZ.Domain.Enums;

namespace EmailEZ.Application.Interfaces;

/// <summary>
/// Repository interface for WorkspaceUser entities with custom operations.
/// </summary>
public interface IWorkspaceUserRepository : IGenericRepository<WorkspaceUser>
{
    /// <summary>
    /// Gets all workspace users for a specific workspace.
    /// </summary>
    /// <param name="workspaceId">The workspace identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A collection of workspace users.</returns>
    Task<IEnumerable<WorkspaceUser>> GetByWorkspaceIdAsync(Guid workspaceId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all workspaces for a specific user.
    /// </summary>
    /// <param name="userId">The user identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A collection of workspace users.</returns>
    Task<IEnumerable<WorkspaceUser>> GetByUserIdAsync(string userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a workspace user by workspace and user identifiers.
    /// </summary>
    /// <param name="workspaceId">The workspace identifier.</param>
    /// <param name="userId">The user identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The workspace user if found, otherwise null.</returns>
    Task<WorkspaceUser?> GetByWorkspaceAndUserAsync(Guid workspaceId, string userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if a user has a specific role in a workspace.
    /// </summary>
    /// <param name="workspaceId">The workspace identifier.</param>
    /// <param name="userId">The user identifier.</param>
    /// <param name="role">The role to check for.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>True if the user has the specified role, otherwise false.</returns>
    Task<bool> HasRoleAsync(Guid workspaceId, string userId, WorkspaceUserRole role, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if a user has at least the minimum required role in a workspace.
    /// </summary>
    /// <param name="workspaceId">The workspace identifier.</param>
    /// <param name="userId">The user identifier.</param>
    /// <param name="minimumRole">The minimum role required.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>True if the user has at least the minimum role, otherwise false.</returns>
    Task<bool> HasMinimumRoleAsync(Guid workspaceId, string userId, WorkspaceUserRole minimumRole, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the owner of a workspace.
    /// </summary>
    /// <param name="workspaceId">The workspace identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The workspace owner if found, otherwise null.</returns>
    Task<WorkspaceUser?> GetOwnerAsync(Guid workspaceId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates the role of a user in a workspace.
    /// </summary>
    /// <param name="workspaceId">The workspace identifier.</param>
    /// <param name="userId">The user identifier.</param>
    /// <param name="newRole">The new role.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The updated workspace user if found, otherwise null.</returns>
    Task<WorkspaceUser?> UpdateRoleAsync(Guid workspaceId, string userId, WorkspaceUserRole newRole, CancellationToken cancellationToken = default);
}