using EmailEZ.Application.Interfaces; // For IApplicationDbContext
using MediatR;
using Microsoft.EntityFrameworkCore; // For FirstOrDefaultAsync and SaveChangesAsync

namespace EmailEZ.Application.Features.Workspaces.Commands.UpdateWorkspace;

public class UpdateWorkspaceCommandHandler : IRequestHandler<UpdateWorkspaceCommand, UpdateWorkspaceResponse>
{
    private readonly IApplicationDbContext _context;

    public UpdateWorkspaceCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<UpdateWorkspaceResponse> Handle(UpdateWorkspaceCommand request, CancellationToken cancellationToken)
    {
        // 1. Find the workspace
        var workspace = await _context.Workspaces.FirstOrDefaultAsync(t => t.Id == request.Id, cancellationToken);

        if (workspace == null)
        {
            return new UpdateWorkspaceResponse(false, $"Workspace with ID '{request.Id}' not found.");
        }

        // Optional: Check for duplicate Name or Domain if they are unique constraints
        // For example, if you want to prevent changing domain to an existing one:
        var existingWorkspaceWithSameDomain = await _context.Workspaces
            .AnyAsync(t => t.Domain == request.Domain && t.Id != request.Id, cancellationToken);
        if (existingWorkspaceWithSameDomain)
        {
            return new UpdateWorkspaceResponse(false, $"Another workspace with domain '{request.Domain}' already exists.");
        }

        var existingWorkspaceWithSameName = await _context.Workspaces
            .AnyAsync(t => t.Name == request.Name && t.Id != request.Id, cancellationToken);
        if (existingWorkspaceWithSameName)
        {
            return new UpdateWorkspaceResponse(false, $"Another workspace with name '{request.Name}' already exists.");
        }


        // 2. Update properties
        workspace.Name = request.Name;
        workspace.Domain = request.Domain;
        workspace.IsActive = request.IsActive;
        // workspace.LastModifiedAtUtc = DateTime.UtcNow; // Consider adding LastModified field

        // 3. Save changes
        await _context.SaveChangesAsync(cancellationToken);

        return new UpdateWorkspaceResponse(true, "Workspace updated successfully.");
    }
}