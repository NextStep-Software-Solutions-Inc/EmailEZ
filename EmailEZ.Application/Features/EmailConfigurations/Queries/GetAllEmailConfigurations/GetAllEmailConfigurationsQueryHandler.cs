using EmailEZ.Application.Interfaces; // For IApplicationDbContext
using MediatR;
using Microsoft.EntityFrameworkCore; // For ToListAsync, AsNoTracking

namespace EmailEZ.Application.Features.EmailConfigurations.Queries.GetAllEmailConfigurations;

public class GetAllEmailConfigurationsQueryHandler : IRequestHandler<GetAllEmailConfigurationsQuery, List<GetAllEmailConfigurationsResponse>>
{
    private readonly IApplicationDbContext _context;

    public GetAllEmailConfigurationsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<GetAllEmailConfigurationsResponse>> Handle(GetAllEmailConfigurationsQuery request, CancellationToken cancellationToken)
    {
        var configs = await _context.EmailConfigurations
            .AsNoTracking()
            .Where(ec => ec.WorkspaceId == request.WorkspaceId) // Crucial: only get configs for the specified workspace
            .Select(ec => new GetAllEmailConfigurationsResponse(
                ec.Id,
                ec.WorkspaceId,
                ec.SmtpHost,
                ec.SmtpPort,
                ec.UseSsl,
                ec.Username,
                ec.FromEmail,
                ec.DisplayName,
                ec.CreatedAt
            ))
            .ToListAsync(cancellationToken);

        return configs; // Returns an empty list if no configs found, which is standard for "get all"
    }
}