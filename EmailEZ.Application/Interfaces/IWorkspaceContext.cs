namespace EmailEZ.Application.Interfaces;

/// <summary>
/// Provides access to the current workspace's context within the application.
/// </summary>
public interface IWorkspaceContext
{
    /// <summary>
    /// Gets the unique identifier of the current workspace.
    /// </summary>
    Guid? WorkspaceId { get; }

    /// <summary>
    /// Gets the domain associated with the current workspace.
    /// </summary>
    string? Domain { get; }

    /// <summary>
    /// Indicates if a workspace has been successfully identified for the current request.
    /// </summary>
    bool IsWorkspaceSet { get; }

    /// <summary>
    /// Sets the workspace context. This method is typically called by middleware.
    /// </summary>
    /// <param name="workspaceId">The ID of the identified workspace.</param>
    /// <param name="domain">The domain of the identified workspace.</param>
    void SetWorkspace(Guid workspaceId, string? domain = default);
}