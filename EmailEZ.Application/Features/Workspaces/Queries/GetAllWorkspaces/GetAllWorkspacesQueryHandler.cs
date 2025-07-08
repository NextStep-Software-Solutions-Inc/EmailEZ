using EmailEZ.Application.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore; // For ToListAsync, AsNoTracking

namespace EmailEZ.Application.Features.Workspaces.Queries.GetAllWorkspaces;

public class GetAllWorkspacesQueryHandler(IWorkspaceManagementService workspaceManagementService) : IRequestHandler<GetAllWorkspacesQuery, List<GetAllWorkspacesResponse>>
{
    public async Task<List<GetAllWorkspacesResponse>> Handle(GetAllWorkspacesQuery request, CancellationToken cancellationToken)
    {
        var workspaces = await workspaceManagementService.GetAllWorkspacesForCurrentUserAsync(cancellationToken);
        return workspaces.Select(w => new GetAllWorkspacesResponse(
                w.Id,
                w.Name,
                w.Domain,
                w.IsActive,
                w.CreatedAt
            ))
            .ToList();
    }
}