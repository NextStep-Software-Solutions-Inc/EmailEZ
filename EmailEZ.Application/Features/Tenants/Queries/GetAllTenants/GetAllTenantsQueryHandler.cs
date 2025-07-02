using EmailEZ.Application.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore; // For ToListAsync, AsNoTracking

namespace EmailEZ.Application.Features.Tenants.Queries.GetAllTenants;

public class GetAllTenantsQueryHandler : IRequestHandler<GetAllTenantsQuery, List<GetAllTenantsResponse>>
{
    private readonly IApplicationDbContext _context;
    //private readonly IEmailSenderClient _emailSenderClient;

    public GetAllTenantsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
        //_emailSenderClient = emailSenderClient;
    }

    public async Task<List<GetAllTenantsResponse>> Handle(GetAllTenantsQuery request, CancellationToken cancellationToken)
    {
        var tenants = await _context.Tenants
            .AsNoTracking() // Recommended for read operations
            .Select(t => new GetAllTenantsResponse(
                t.Id,
                t.Name,
                t.Domain,
                t.IsActive,
                t.CreatedAt
            ))
            .ToListAsync(cancellationToken);

        return tenants;
    }
}