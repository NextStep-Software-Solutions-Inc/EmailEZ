using EmailEZ.Application.Interfaces; // For IApplicationDbContext
using EmailEZ.Domain.Entities; // For EmailConfiguration entity
using MediatR;
using Microsoft.EntityFrameworkCore; // For AnyAsync

namespace EmailEZ.Application.Features.EmailConfigurations.Commands.CreateEmailConfiguration;

public class CreateEmailConfigurationCommandHandler : IRequestHandler<CreateEmailConfigurationCommand, CreateEmailConfigurationResponse>
{
    private readonly IApplicationDbContext _context;
    private readonly IEncryptionService _smtpPasswordEncryptor;


    public CreateEmailConfigurationCommandHandler(IApplicationDbContext context, IEncryptionService smtpPasswordEncryptor)
    {
        _context = context;
        _smtpPasswordEncryptor = smtpPasswordEncryptor;
    }

    public async Task<CreateEmailConfigurationResponse> Handle(CreateEmailConfigurationCommand request, CancellationToken cancellationToken)
    {
        // Optional: Basic validation if a tenant can only have one active configuration, etc.
        // Or if SmtpHost+Username must be unique per tenant.
        var tenantExists = await _context.Tenants.AnyAsync(t => t.Id == request.TenantId, cancellationToken);
        if (!tenantExists)
        {
            return new CreateEmailConfigurationResponse(Guid.Empty, false, $"Tenant with ID '{request.TenantId}' not found.");
        }

        // Check for duplicate configuration for the same tenant (e.g., same host/username combo)
        var existingConfig = await _context.EmailConfigurations
            .AnyAsync(ec => ec.TenantId == request.TenantId &&
                            ec.SmtpHost == request.SmtpHost &&
                            ec.Username == request.Username, // Assuming username is unique per host per tenant
                            cancellationToken);

        if (existingConfig)
        {
            return new CreateEmailConfigurationResponse(Guid.Empty, false, "A configuration with this SMTP Host and Username already exists for this tenant.");
        }


        // IMPORTANT: In a real application, you would encrypt the request.Password here
        // before assigning it to the entity. For now, we're storing it in plain text.
        // You'll integrate an encryption service (e.g., from Infrastructure) here.

        // encrypt the SMTP password
        var encryptedSmtpPassword = _smtpPasswordEncryptor.Encrypt(request.Password);

        var emailConfig = new EmailConfiguration
        {
            TenantId = request.TenantId,
            SmtpHost = request.SmtpHost,
            SmtpPort = request.SmtpPort,
            UseSsl = request.UseSsl,
            Username = request.Username,
            Password = encryptedSmtpPassword, 
            DisplayName = request.DisplayName
        };

        _context.EmailConfigurations.Add(emailConfig);
        await _context.SaveChangesAsync(cancellationToken);

        return new CreateEmailConfigurationResponse(emailConfig.Id, true, "Email configuration created successfully.");
    }
}