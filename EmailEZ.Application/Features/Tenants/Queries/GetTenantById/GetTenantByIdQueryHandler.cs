using MediatR;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore; // For FirstOrDefaultAsync
using EmailEZ.Application.Interfaces; // For IApplicationDbContext

namespace EmailEZ.Application.Features.Tenants.Queries.GetTenantById;

public class GetTenantByIdQueryHandler : IRequestHandler<GetTenantByIdQuery, GetTenantByIdResponse?>
{
    private readonly IApplicationDbContext _context;

    public GetTenantByIdQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<GetTenantByIdResponse?> Handle(GetTenantByIdQuery request, CancellationToken cancellationToken)
    {
        var tenant = await _context.Tenants
            .AsNoTracking() // Recommended for read operations
            .FirstOrDefaultAsync(t => t.Id == request.Id, cancellationToken);

        if (tenant == null)
        {
            return null;
        }

        return new GetTenantByIdResponse(
            tenant.Id,
            tenant.Name,
            tenant.Domain,
            tenant.IsActive,
            tenant.CreatedAt
        );
    }
}
