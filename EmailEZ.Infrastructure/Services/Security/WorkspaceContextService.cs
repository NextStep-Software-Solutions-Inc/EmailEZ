using EmailEZ.Application.Interfaces; // For IWorkspaceContext

namespace EmailEZ.Infrastructure.Services.Security;

// Implements IWorkspaceContext, stores workspace info for the current request
public class WorkspaceContextService : IWorkspaceContext
{
    public Guid? WorkspaceId { get; private set; }

    public string? Domain { get; private set; } = string.Empty;
    public bool IsWorkspaceSet { get; private set; }

    public void SetWorkspace(Guid workspaceId, string? domain = default)
    {
        WorkspaceId = workspaceId;
        Domain = domain;
        IsWorkspaceSet = true;
    }
}