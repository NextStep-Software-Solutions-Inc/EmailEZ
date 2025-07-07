using EmailEZ.Application.Interfaces;
using MediatR;

namespace EmailEZ.Application.Features.Workspaces.Commands.CreateWorkspace;

/// <summary>
/// Handles the creation of a new Workspace.
/// </summary>
public class CreateWorkspaceCommandHandler : IRequestHandler<CreateWorkspaceCommand, CreateWorkspaceResponse>
{
    private readonly IWorkspaceManagementService _workspaceManagementService;
    private readonly ICurrentUserService _currentUserService;

    public CreateWorkspaceCommandHandler(
        IWorkspaceManagementService workspaceManagementService,
        ICurrentUserService currentUserService)
    {
        _workspaceManagementService = workspaceManagementService;
        _currentUserService = currentUserService;
    }

    public async Task<CreateWorkspaceResponse> Handle(CreateWorkspaceCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var userId = _currentUserService.GetCurrentUserId();
            if (string.IsNullOrEmpty(userId))
            {
                return new CreateWorkspaceResponse { IsSuccess = false, Message = "User not authenticated." };
            }

            var result = await _workspaceManagementService.CreateWorkspaceWithOwnerAsync(
                request.Name, 
                request.Domain, 
                userId, 
                cancellationToken);

            return new CreateWorkspaceResponse
            {
                WorkspaceId = result.Workspace.Id,
                Name = result.Workspace.Name,
                Domain = result.Workspace.Domain,
                ApiKey = result.PlaintextApiKey,
                IsSuccess = true,
                Message = "Workspace created successfully."
            };
        }
        catch (InvalidOperationException ex)
        {
            return new CreateWorkspaceResponse { IsSuccess = false, Message = ex.Message };
        }
        catch (Exception)
        {
            return new CreateWorkspaceResponse { IsSuccess = false, Message = "An error occurred while creating the workspace." };
        }
    }
}