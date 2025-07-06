using MediatR;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore; // For FirstOrDefaultAsync
using EmailEZ.Application.Interfaces;

namespace EmailEZ.Application.Features.Workspaces.Queries.GetWorkspaceById;

public class GetWorkspaceByIdQueryHandler : IRequestHandler<GetWorkspaceByIdQuery, GetWorkspaceByIdResponse?>
{
    private readonly IApplicationDbContext _context;

    public GetWorkspaceByIdQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<GetWorkspaceByIdResponse?> Handle(GetWorkspaceByIdQuery request, CancellationToken cancellationToken)
    {
        var workspace = await _context.Workspaces
            .AsNoTracking() // Recommended for read operations
            .FirstOrDefaultAsync(t => t.Id == request.Id, cancellationToken);

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
