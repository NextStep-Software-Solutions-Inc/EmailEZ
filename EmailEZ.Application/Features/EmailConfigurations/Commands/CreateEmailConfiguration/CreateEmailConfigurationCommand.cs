using MediatR;

namespace EmailEZ.Application.Features.EmailConfigurations.Commands.CreateEmailConfiguration;

public record CreateEmailConfigurationCommand(
    Guid WorkspaceId,
    string SmtpHost,
    int SmtpPort,
    bool UseSsl,
    string Username,
    string FromEmail,
    string Password, // Remember: this will need encryption before saving!
    string DisplayName
) : IRequest<CreateEmailConfigurationResponse>
{
}

public record CreateEmailConfigurationResponse(
    Guid EmailConfigurationId,
    bool Success,
    string? Message = default
);