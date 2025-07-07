using EmailEZ.Application.Interfaces;
using MediatR;

namespace EmailEZ.Application.Features.Workspaces.Commands.DeleteWorkspace;

public class DeleteWorkspaceCommandHandler : IRequestHandler<DeleteWorkspaceCommand, DeleteWorkspaceResponse>
{
    private readonly IWorkspaceManagementService _workspaceManagementService;

    public DeleteWorkspaceCommandHandler(IWorkspaceManagementService workspaceManagementService)
    {
        _workspaceManagementService = workspaceManagementService;
    }

    public async Task<DeleteWorkspaceResponse> Handle(DeleteWorkspaceCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var deletedWorkspace = await _workspaceManagementService.DeleteWorkspaceAsync(
                request.Id,
                cancellationToken);

            return new DeleteWorkspaceResponse(true, "Workspace deleted successfully.");
        }
        catch (InvalidOperationException ex)
        {
            return new DeleteWorkspaceResponse(false, ex.Message);
        }
        catch (Exception)
        {
            return new DeleteWorkspaceResponse(false, "An error occurred while deleting the workspace.");
        }
    }
}