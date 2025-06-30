using EmailEZ.Application.Interfaces; // For IApplicationDbContext
using MediatR;
using Microsoft.EntityFrameworkCore; // For FirstOrDefaultAsync and SaveChangesAsync

namespace EmailEZ.Application.Features.Tenants.Commands.UpdateTenant;

public class UpdateTenantCommandHandler : IRequestHandler<UpdateTenantCommand, UpdateTenantResponse>
{
    private readonly IApplicationDbContext _context;

    public UpdateTenantCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<UpdateTenantResponse> Handle(UpdateTenantCommand request, CancellationToken cancellationToken)
    {
        // 1. Find the tenant
        var tenant = await _context.Tenants.FirstOrDefaultAsync(t => t.Id == request.Id, cancellationToken);

        if (tenant == null)
        {
            return new UpdateTenantResponse(false, $"Tenant with ID '{request.Id}' not found.");
        }

        // Optional: Check for duplicate Name or Domain if they are unique constraints
        // For example, if you want to prevent changing domain to an existing one:
        var existingTenantWithSameDomain = await _context.Tenants
            .AnyAsync(t => t.Domain == request.Domain && t.Id != request.Id, cancellationToken);
        if (existingTenantWithSameDomain)
        {
            return new UpdateTenantResponse(false, $"Another tenant with domain '{request.Domain}' already exists.");
        }

        var existingTenantWithSameName = await _context.Tenants
            .AnyAsync(t => t.Name == request.Name && t.Id != request.Id, cancellationToken);
        if (existingTenantWithSameName)
        {
            return new UpdateTenantResponse(false, $"Another tenant with name '{request.Name}' already exists.");
        }


        // 2. Update properties
        tenant.Name = request.Name;
        tenant.Domain = request.Domain;
        tenant.IsActive = request.IsActive;
        // tenant.LastModifiedAtUtc = DateTime.UtcNow; // Consider adding LastModified field

        // 3. Save changes
        await _context.SaveChangesAsync(cancellationToken);

        return new UpdateTenantResponse(true, "Tenant updated successfully.");
    }
}