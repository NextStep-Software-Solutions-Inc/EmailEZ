using EmailEZ.Application.Interfaces;

namespace EmailEZ.Infrastructure.Services.Security;

public class CurrentUserService : ICurrentUserService
{
    private readonly IWorkspaceContext _workspaceContext; // Inject the workspace context

    public CurrentUserService(IWorkspaceContext workspaceContext)
    {
        _workspaceContext = workspaceContext;
    }

    public Guid? GetCurrentUserId()
    {
        // If a workspace is set via API Key middleware, return the WorkspaceId
        if (_workspaceContext.IsWorkspaceSet)
        {
            return _workspaceContext.WorkspaceId;
        }

        // For system-level operations (e.g., Hangfire jobs not tied to a specific API call)
        // you might return a predefined system GUID, or null.
        // For now, let's return null if no workspace context is available.
        return null;
    }

    public Guid? GetCurrentWorkspaceId()
    {
        // If a workspace is set via API Key middleware, return the WorkspaceId
        if (_workspaceContext.IsWorkspaceSet)
        {
            return _workspaceContext.WorkspaceId;
        }

        // For system-level operations (e.g., Hangfire jobs not tied to a specific API call)
        // you might return a predefined system GUID, or null.
        // For now, let's return null if no workspace context is available.
        return null;
    }
}