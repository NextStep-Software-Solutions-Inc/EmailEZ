using EmailEZ.Application.Interfaces; // For IApplicationDbContext
using MediatR;
using Microsoft.EntityFrameworkCore; // For FirstOrDefaultAsync and SaveChangesAsync

namespace EmailEZ.Application.Features.Workspaces.Commands.DeleteWorkspace;

public class DeleteWorkspaceCommandHandler : IRequestHandler<DeleteWorkspaceCommand, DeleteWorkspaceResponse>
{
    private readonly IApplicationDbContext _context;

    public DeleteWorkspaceCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<DeleteWorkspaceResponse> Handle(DeleteWorkspaceCommand request, CancellationToken cancellationToken)
    {
        // 1. Find the workspace
        var workspace = await _context.Workspaces.FirstOrDefaultAsync(t => t.Id == request.Id, cancellationToken);

        if (workspace == null)
        {
            return new DeleteWorkspaceResponse(false, $"Workspace with ID '{request.Id}' not found.");
        }

        // 2. Remove the workspace
        _context.Workspaces.Remove(workspace);

        // 3. Save changes
        await _context.SaveChangesAsync(cancellationToken);

        return new DeleteWorkspaceResponse(true, "Workspace deleted successfully.");
    }
}