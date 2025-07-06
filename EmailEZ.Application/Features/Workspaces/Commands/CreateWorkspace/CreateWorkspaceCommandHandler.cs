using EmailEZ.Application.Interfaces; // For IApplicationDbContext, IApiKeyHasher, ISmtpPasswordEncryptor
using EmailEZ.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore; // For .AnyAsync(), .FirstOrDefaultAsync() for domain uniqueness checks

namespace EmailEZ.Application.Features.Workspaces.Commands.CreateWorkspace;

/// <summary>
/// Handles the creation of a new Workspace.
/// </summary>
public class CreateWorkspaceCommandHandler : IRequestHandler<CreateWorkspaceCommand, CreateWorkspaceResponse>
{
    private readonly IApplicationDbContext _context;
    private readonly IApiKeyHasher _apiKeyHasher;

    public CreateWorkspaceCommandHandler(
        IApplicationDbContext context,
        IApiKeyHasher apiKeyHasher,
        IEncryptionService smtpPasswordEncryptor,
        ICurrentUserService currentUserService)
    {
        _context = context;
        _apiKeyHasher = apiKeyHasher;
    }

    public async Task<CreateWorkspaceResponse> Handle(CreateWorkspaceCommand request, CancellationToken cancellationToken)
    {
        // 1. Business Rule: Ensure tenant name is unique
        var existingWorkspaceWithName = await _context.Workspaces
            .IgnoreQueryFilters() // Ignore IsDeleted filter to check against all existing tenants
            .FirstOrDefaultAsync(t => t.Name == request.Name, cancellationToken);
        if (existingWorkspaceWithName != null)
        {
            return new CreateWorkspaceResponse { IsSuccess = false, Message = $"Workspace with name '{request.Name}' already exists." };
        }

        // 2. Business Rule: Ensure tenant domain is unique
        var existingWorkspaceWithDomain = await _context.Workspaces
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(t => t.Domain == request.Domain, cancellationToken);
        if (existingWorkspaceWithDomain != null)
        {
            return new CreateWorkspaceResponse { IsSuccess = false, Message = $"Workspace with domain '{request.Domain}' already exists." };
        }

        // 3. Generate a new API Key for the tenant
        var newPlaintextApiKey = Guid.NewGuid().ToString("N"); // N format for no hyphens
        var hashedApiKey = _apiKeyHasher.HashApiKey(newPlaintextApiKey);

        // 5. Create the new Workspace entity
        var tenant = new Workspace
        {
            Name = request.Name,
            ApiKeyHash = hashedApiKey,
            Domain = request.Domain,
            IsActive = true,
        };

        // 6. Add to DbContext and save
        _context.Workspaces.Add(tenant);
        await _context.SaveChangesAsync(cancellationToken);

        // 7. Return the response, including the plaintext API Key (which is only shown upon creation)
        return new CreateWorkspaceResponse
        {
            WorkspaceId = tenant.Id,
            Name = tenant.Name,
            Domain = tenant.Domain,
            ApiKey = newPlaintextApiKey, // Crucial: return the plaintext key once, client must store it.
            IsSuccess = true,
            Message = "Workspace created successfully."
        };
    }
}