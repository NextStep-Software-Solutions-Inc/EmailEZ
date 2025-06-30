using EmailEZ.Application.Interfaces; // For IApplicationDbContext
using MediatR;
using Microsoft.EntityFrameworkCore; // For FirstOrDefaultAsync

namespace EmailEZ.Application.Features.EmailConfigurations.Queries.GetEmailConfigurationById;

public class GetEmailConfigurationByIdQueryHandler : IRequestHandler<GetEmailConfigurationByIdQuery, GetEmailConfigurationByIdResponse>
{
    private readonly IApplicationDbContext _context;

    public GetEmailConfigurationByIdQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<GetEmailConfigurationByIdResponse> Handle(GetEmailConfigurationByIdQuery request, CancellationToken cancellationToken)
    {
        var config = await _context.EmailConfigurations
            .AsNoTracking()
            .Where(ec => ec.Id == request.Id && ec.TenantId == request.TenantId) // Crucial: ensure it belongs to the specified tenant
            .FirstOrDefaultAsync(cancellationToken);

        if (config == null)
        {
            return null; // Endpoint will handle 404
        }

        return new GetEmailConfigurationByIdResponse(
            config.Id,
            config.TenantId,
            config.SmtpHost,
            config.SmtpPort,
            config.UseSsl,
            config.Username,
            config.FromEmail,
            config.DisplayName,
            config.CreatedAt
        );
    }
}