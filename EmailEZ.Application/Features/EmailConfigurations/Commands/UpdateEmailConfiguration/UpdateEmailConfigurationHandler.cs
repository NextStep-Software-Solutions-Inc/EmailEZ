using EmailEZ.Application.Interfaces; // For IApplicationDbContext
using MediatR;
using Microsoft.EntityFrameworkCore; // For FirstOrDefaultAsync and SaveChangesAsync

namespace EmailEZ.Application.Features.EmailConfigurations.Commands.UpdateEmailConfiguration;

public class UpdateEmailConfigurationCommandHandler : IRequestHandler<UpdateEmailConfigurationCommand, UpdateEmailConfigurationResponse>
{
    private readonly IApplicationDbContext _context;
    private readonly IEncryptionService _smtpPasswordEncryptor;

    // Potentially inject an encryption service here if you build one
    // private readonly IEncryptionService _encryptionService;

    public UpdateEmailConfigurationCommandHandler(IApplicationDbContext context, IEncryptionService smtpPasswordEncryptor)
    {
        _context = context;
        _smtpPasswordEncryptor = smtpPasswordEncryptor;
    }

    public async Task<UpdateEmailConfigurationResponse> Handle(UpdateEmailConfigurationCommand request, CancellationToken cancellationToken)
    {
        // 1. Find the email configuration within the specified workspace
        var config = await _context.EmailConfigurations
            .FirstOrDefaultAsync(ec => ec.Id == request.Id && ec.WorkspaceId == request.WorkspaceId, cancellationToken);

        if (config == null)
        {
            return new UpdateEmailConfigurationResponse(false, $"Email configuration with ID '{request.Id}' not found for workspace '{request.WorkspaceId}'.");
        }

        // 2. Optional: Check for duplicate host/username for *other* configs of this workspace
        var existingConfigWithSameHostAndUsername = await _context.EmailConfigurations
            .AnyAsync(ec => ec.WorkspaceId == request.WorkspaceId &&
                            ec.SmtpHost == request.SmtpHost &&
                            ec.Username == request.Username &&
                            ec.FromEmail == request.FromEmail && 
                            ec.Id != request.Id,
                            cancellationToken);

        if (existingConfigWithSameHostAndUsername)
        {
            return new UpdateEmailConfigurationResponse(false, "Another configuration with this SMTP Host and Username already exists for this workspace.");
        }

        // 3. Update properties
        config.SmtpHost = request.SmtpHost;
        config.SmtpPort = request.SmtpPort;
        config.UseSsl = request.UseSsl;
        config.Username = request.Username;
        config.FromEmail = request.FromEmail;

        // IMPORTANT: If you implemented password encryption, you'd encrypt the new password here
        if (!string.IsNullOrEmpty(request.Password))
        {
            var encryptedSmtpPassword = _smtpPasswordEncryptor.Encrypt(request.Password);
            config.Password = encryptedSmtpPassword;
        }

        config.DisplayName = request.DisplayName;

        // 4. Save changes
        await _context.SaveChangesAsync(cancellationToken);

        return new UpdateEmailConfigurationResponse(true, "Email configuration updated successfully.");
    }
}