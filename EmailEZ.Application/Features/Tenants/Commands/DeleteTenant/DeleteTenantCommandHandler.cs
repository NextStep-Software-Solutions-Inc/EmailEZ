using EmailEZ.Application.Interfaces; // For IApplicationDbContext
using MediatR;
using Microsoft.EntityFrameworkCore; // For FirstOrDefaultAsync and SaveChangesAsync

namespace EmailEZ.Application.Features.Tenants.Commands.DeleteTenant;

public class DeleteTenantCommandHandler : IRequestHandler<DeleteTenantCommand, DeleteTenantResponse>
{
    private readonly IApplicationDbContext _context;

    public DeleteTenantCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<DeleteTenantResponse> Handle(DeleteTenantCommand request, CancellationToken cancellationToken)
    {
        // 1. Find the tenant
        var tenant = await _context.Tenants.FirstOrDefaultAsync(t => t.Id == request.Id, cancellationToken);

        if (tenant == null)
        {
            return new DeleteTenantResponse(false, $"Tenant with ID '{request.Id}' not found.");
        }

        // 2. Remove the tenant
        _context.Tenants.Remove(tenant);

        // 3. Save changes
        await _context.SaveChangesAsync(cancellationToken);

        return new DeleteTenantResponse(true, "Tenant deleted successfully.");
    }
}