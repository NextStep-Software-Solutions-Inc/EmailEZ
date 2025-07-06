using MediatR; // For IRequest<TResponse>

namespace EmailEZ.Application.Features.Workspaces.Commands.CreateWorkspace;

/// <summary>
/// Represents a command to create a new Workspace.
/// </summary>
public class CreateWorkspaceCommand : IRequest<CreateWorkspaceResponse>
{
    public string Name { get; set; } = string.Empty;
    public string Domain { get; set; } = string.Empty;
    
}

/// <summary>
/// Represents the response returned after creating a Workspace.
/// </summary>
public class CreateWorkspaceResponse
{
    public Guid WorkspaceId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Domain { get; set; } = string.Empty;
    public string ApiKey { get; set; } = string.Empty; // The newly generated plaintext API key
    public bool IsSuccess { get; set; } = true;
    public string? Message { get; set; }
}