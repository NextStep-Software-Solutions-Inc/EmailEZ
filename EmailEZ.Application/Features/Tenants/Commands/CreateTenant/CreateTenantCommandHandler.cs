using EmailEZ.Application.Interfaces; // For IApplicationDbContext, IApiKeyHasher, ISmtpPasswordEncryptor
using EmailEZ.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore; // For .AnyAsync(), .FirstOrDefaultAsync() for domain uniqueness checks

namespace EmailEZ.Application.Features.Tenants.Commands.CreateTenant;

/// <summary>
/// Handles the creation of a new Tenant.
/// </summary>
public class CreateTenantCommandHandler : IRequestHandler<CreateTenantCommand, CreateTenantResponse>
{
    private readonly IApplicationDbContext _context;
    private readonly IApiKeyHasher _apiKeyHasher;

    public CreateTenantCommandHandler(
        IApplicationDbContext context,
        IApiKeyHasher apiKeyHasher,
        IEncryptionService smtpPasswordEncryptor,
        ICurrentUserService currentUserService)
    {
        _context = context;
        _apiKeyHasher = apiKeyHasher;
    }

    public async Task<CreateTenantResponse> Handle(CreateTenantCommand request, CancellationToken cancellationToken)
    {
        // 1. Business Rule: Ensure tenant name is unique
        var existingTenantWithName = await _context.Tenants
            .IgnoreQueryFilters() // Ignore IsDeleted filter to check against all existing tenants
            .FirstOrDefaultAsync(t => t.Name == request.Name, cancellationToken);
        if (existingTenantWithName != null)
        {
            return new CreateTenantResponse { IsSuccess = false, Message = $"Tenant with name '{request.Name}' already exists." };
        }

        // 2. Business Rule: Ensure tenant domain is unique
        var existingTenantWithDomain = await _context.Tenants
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(t => t.Domain == request.Domain, cancellationToken);
        if (existingTenantWithDomain != null)
        {
            return new CreateTenantResponse { IsSuccess = false, Message = $"Tenant with domain '{request.Domain}' already exists." };
        }

        // 3. Generate a new API Key for the tenant
        var newPlaintextApiKey = Guid.NewGuid().ToString("N"); // N format for no hyphens
        var hashedApiKey = _apiKeyHasher.HashApiKey(newPlaintextApiKey);

        // 5. Create the new Tenant entity
        var tenant = new Tenant
        {
            Name = request.Name,
            ApiKeyHash = hashedApiKey,
            Domain = request.Domain,
            IsActive = true,
        };

        // 6. Add to DbContext and save
        _context.Tenants.Add(tenant);
        await _context.SaveChangesAsync(cancellationToken);

        // 7. Return the response, including the plaintext API Key (which is only shown upon creation)
        return new CreateTenantResponse
        {
            TenantId = tenant.Id,
            Name = tenant.Name,
            Domain = tenant.Domain,
            ApiKey = newPlaintextApiKey, // Crucial: return the plaintext key once, client must store it.
            IsSuccess = true,
            Message = "Tenant created successfully."
        };
    }
}