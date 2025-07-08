using MediatR;
using EmailEZ.Application.Interfaces;
using EmailEZ.Domain.Entities;

namespace EmailEZ.Application.Features.Workspaces.Queries.GetWorkspaceById;

public class GetWorkspaceByIdQueryHandler(IGenericRepository<Workspace> workspaceRepository) : IRequestHandler<GetWorkspaceByIdQuery, GetWorkspaceByIdResponse?>
{
    private readonly IGenericRepository<Workspace> _workspaceRepository = workspaceRepository;

    public async Task<GetWorkspaceByIdResponse?> Handle(GetWorkspaceByIdQuery request, CancellationToken cancellationToken)
    {
        var workspace = await _workspaceRepository.GetByIdAsync(request.Id, cancellationToken);
        if (workspace == null)
        {
            return null;
        }

        return new GetWorkspaceByIdResponse(
            workspace.Id,
            workspace.Name,
            workspace.Domain,
            workspace.IsActive,
            workspace.CreatedAt
        );
    }
}
