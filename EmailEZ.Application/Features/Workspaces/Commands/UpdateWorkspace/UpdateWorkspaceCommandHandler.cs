using EmailEZ.Application.Interfaces;
using MediatR;

namespace EmailEZ.Application.Features.Workspaces.Commands.UpdateWorkspace;

public class UpdateWorkspaceCommandHandler : IRequestHandler<UpdateWorkspaceCommand, UpdateWorkspaceResponse>
{
    private readonly IWorkspaceManagementService _workspaceManagementService;

    public UpdateWorkspaceCommandHandler(IWorkspaceManagementService workspaceManagementService)
    {
        _workspaceManagementService = workspaceManagementService;
    }

    public async Task<UpdateWorkspaceResponse> Handle(UpdateWorkspaceCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var updatedWorkspace = await _workspaceManagementService.UpdateWorkspaceAsync(
                request.Id,
                request.Name,
                request.Domain,
                request.IsActive,
                cancellationToken);

            return new UpdateWorkspaceResponse(true, "Workspace updated successfully.");
        }
        catch (InvalidOperationException ex)
        {
            return new UpdateWorkspaceResponse(false, ex.Message);
        }
        catch (Exception)
        {
            return new UpdateWorkspaceResponse(false, "An error occurred while updating the workspace.");
        }
    }
}