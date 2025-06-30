using MediatR;
using System.Collections.Generic;
using System.Linq; // For ToListAsync
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore; // For ToListAsync, AsNoTracking
using EmailEZ.Application.Interfaces; // For IApplicationDbContext

namespace EmailEZ.Application.Features.Tenants.Queries.GetAllTenants;

public class GetAllTenantsQueryHandler : IRequestHandler<GetAllTenantsQuery, List<GetAllTenantsResponse>>
{
    private readonly IApplicationDbContext _context;

    public GetAllTenantsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
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