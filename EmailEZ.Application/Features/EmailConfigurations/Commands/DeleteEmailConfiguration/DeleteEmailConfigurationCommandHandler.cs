using EmailEZ.Application.Interfaces; // For IApplicationDbContext
using MediatR;
using Microsoft.EntityFrameworkCore; // For FirstOrDefaultAsync and SaveChangesAsync

namespace EmailEZ.Application.Features.EmailConfigurations.Commands.DeleteEmailConfiguration;

public class DeleteEmailConfigurationCommandHandler : IRequestHandler<DeleteEmailConfigurationCommand, DeleteEmailConfigurationResponse>
{
    private readonly IApplicationDbContext _context;

    public DeleteEmailConfigurationCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<DeleteEmailConfigurationResponse> Handle(DeleteEmailConfigurationCommand request, CancellationToken cancellationToken)
    {
        // 1. Find the email configuration within the specified tenant
        var config = await _context.EmailConfigurations
            .FirstOrDefaultAsync(ec => ec.Id == request.Id && ec.TenantId == request.TenantId, cancellationToken);

        if (config == null)
        {
            return new DeleteEmailConfigurationResponse(false, $"Email configuration with ID '{request.Id}' not found for tenant '{request.TenantId}'.");
        }

        // 2. Remove the configuration
        _context.EmailConfigurations.Remove(config);

        // 3. Save changes
        await _context.SaveChangesAsync(cancellationToken);

        return new DeleteEmailConfigurationResponse(true, "Email configuration deleted successfully.");
    }
}