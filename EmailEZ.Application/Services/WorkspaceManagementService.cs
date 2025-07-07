using EmailEZ.Application.Interfaces;
using EmailEZ.Domain.Entities;
using EmailEZ.Domain.Enums;

namespace EmailEZ.Application.Services;

/// <summary>
/// Service for managing workspace-related business operations.
/// </summary>
public class WorkspaceManagementService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IApiKeyHasher _apiKeyHasher;

    public WorkspaceManagementService(IUnitOfWork unitOfWork, IApiKeyHasher apiKeyHasher)
    {
        _unitOfWork = unitOfWork;
        _apiKeyHasher = apiKeyHasher;
    }

    /// <summary>
    /// Creates a new workspace with an owner.
    /// </summary>
    /// <param name="workspaceName">The name of the workspace.</param>
    /// <param name="domain">The domain of the workspace.</param>
    /// <param name="ownerId">The ID of the owner.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The created workspace with API key.</returns>
    public async Task<WorkspaceCreationResult> CreateWorkspaceWithOwnerAsync(
        string workspaceName, 
        string domain, 
        string ownerId,
        CancellationToken cancellationToken = default)
    {
        await _unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            // ? Business Rule: Ensure workspace name is unique
            var existingWorkspaceWithName = await _unitOfWork.Workspaces
                .Query()
                .Where(w => w.Name == workspaceName)
                .FirstOrDefaultAsync(cancellationToken);

            if (existingWorkspaceWithName != null)
            {
                throw new InvalidOperationException($"Workspace with name '{workspaceName}' already exists.");
            }

            // ? Business Rule: Ensure workspace domain is unique
            var existingWorkspaceWithDomain = await _unitOfWork.Workspaces
                .Query()
                .Where(w => w.Domain == domain)
                .FirstOrDefaultAsync(cancellationToken);

            if (existingWorkspaceWithDomain != null)
            {
                throw new InvalidOperationException($"Workspace with domain '{domain}' already exists.");
            }

            // ? Generate API Key (business logic)
            var plaintextApiKey = Guid.NewGuid().ToString("N");
            var hashedApiKey = _apiKeyHasher.HashApiKey(plaintextApiKey);

            // ? Create workspace entity (business logic)
            var workspace = new Workspace
            {
                Name = workspaceName,
                Domain = domain,
                ApiKeyHash = hashedApiKey,
                IsActive = true,
                ApiKeyLastUsedAt = DateTimeOffset.UtcNow
            };

            var createdWorkspace = await _unitOfWork.Workspaces.AddAsync(workspace, cancellationToken);

            // ? Create workspace user with Owner role (business logic)
            var workspaceUser = new WorkspaceUser
            {
                WorkspaceId = createdWorkspace.Id,
                UserId = ownerId,
                Role = WorkspaceUserRole.Owner
            };

            await _unitOfWork.WorkspaceUsers.AddAsync(workspaceUser, cancellationToken);

            // ? Save all changes atomically
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await _unitOfWork.CommitTransactionAsync(cancellationToken);

            return new WorkspaceCreationResult
            {
                Workspace = createdWorkspace,
                PlaintextApiKey = plaintextApiKey,
                Owner = workspaceUser
            };
        }
        catch
        {
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }

    /// <summary>
    /// Adds a user to a workspace with a specific role.
    /// </summary>
    /// <param name="workspaceId">The workspace ID.</param>
    /// <param name="userId">The user ID.</param>
    /// <param name="role">The role to assign.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The created workspace user.</returns>
    public async Task<WorkspaceUser> AddUserToWorkspaceAsync(
        Guid workspaceId, 
        string userId, 
        WorkspaceUserRole role,
        CancellationToken cancellationToken = default)
    {
        // ? Business Rule: Check if user is already in the workspace
        var existingUser = await _unitOfWork.WorkspaceUserRepository
            .GetByWorkspaceAndUserAsync(workspaceId, userId, cancellationToken);

        if (existingUser != null)
        {
            throw new InvalidOperationException("User is already a member of this workspace.");
        }

        // ? Business Rule: Verify workspace exists
        var workspace = await _unitOfWork.Workspaces.GetByIdAsync(workspaceId, cancellationToken);
        if (workspace == null)
        {
            throw new InvalidOperationException($"Workspace with ID {workspaceId} not found.");
        }

        // ? Create new workspace user
        var workspaceUser = new WorkspaceUser
        {
            WorkspaceId = workspaceId,
            UserId = userId,
            Role = role
        };

        var addedUser = await _unitOfWork.WorkspaceUsers.AddAsync(workspaceUser, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        
        return addedUser;
    }

    /// <summary>
    /// Updates a user's role in a workspace.
    /// </summary>
    /// <param name="workspaceId">The workspace ID.</param>
    /// <param name="userId">The user ID.</param>
    /// <param name="newRole">The new role.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The updated workspace user.</returns>
    public async Task<WorkspaceUser> UpdateUserRoleAsync(
        Guid workspaceId,
        string userId,
        WorkspaceUserRole newRole,
        CancellationToken cancellationToken = default)
    {
        // ? Business Logic: Get existing user
        var workspaceUser = await _unitOfWork.WorkspaceUserRepository
            .GetByWorkspaceAndUserAsync(workspaceId, userId, cancellationToken);

        if (workspaceUser == null)
        {
            throw new InvalidOperationException($"User {userId} is not a member of workspace {workspaceId}.");
        }

        // ? Business Rule: Can't change the last owner
        if (workspaceUser.Role == WorkspaceUserRole.Owner && newRole != WorkspaceUserRole.Owner)
        {
            var ownerCount = await _unitOfWork.WorkspaceUsers.CountAsync(
                wu => wu.WorkspaceId == workspaceId && wu.Role == WorkspaceUserRole.Owner,
                cancellationToken);

            if (ownerCount <= 1)
            {
                throw new InvalidOperationException("Cannot remove the last owner from the workspace.");
            }
        }

        // ? Update role
        workspaceUser.Role = newRole;
        var updatedUser = await _unitOfWork.WorkspaceUsers.UpdateAsync(workspaceUser, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return updatedUser;
    }
}

/// <summary>
/// Result of workspace creation operation.
/// </summary>
public class WorkspaceCreationResult
{
    public Workspace Workspace { get; set; } = null!;
    public string PlaintextApiKey { get; set; } = string.Empty;
    public WorkspaceUser Owner { get; set; } = null!;
}