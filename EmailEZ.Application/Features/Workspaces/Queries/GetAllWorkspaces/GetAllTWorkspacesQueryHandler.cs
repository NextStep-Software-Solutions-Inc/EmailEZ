using EmailEZ.Application.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore; // For ToListAsync, AsNoTracking

namespace EmailEZ.Application.Features.Workspaces.Queries.GetAllWorkspaces;

public class GetAllWorkspacesQueryHandler : IRequestHandler<GetAllWorkspacesQuery, List<GetAllWorkspacesResponse>>
{
    private readonly IApplicationDbContext _context;
    //private readonly IEmailSenderClient _emailSenderClient;

    public GetAllWorkspacesQueryHandler(IApplicationDbContext context)
    {
        _context = context;
        //_emailSenderClient = emailSenderClient;
    }

    public async Task<List<GetAllWorkspacesResponse>> Handle(GetAllWorkspacesQuery request, CancellationToken cancellationToken)
    {
        var workspaces = await _context.Workspaces
            .AsNoTracking() // Recommended for read operations
            .Select(t => new GetAllWorkspacesResponse(
                t.Id,
                t.Name,
                t.Domain,
                t.IsActive,
                t.CreatedAt
            ))
            .ToListAsync(cancellationToken);

        return workspaces;
    }
}