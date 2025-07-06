using MediatR;

namespace EmailEZ.Application.Features.EmailConfigurations.Commands.UpdateEmailConfiguration;

public record UpdateEmailConfigurationCommand(
    Guid Id, // The ID of the configuration to update
    Guid WorkspaceId, // To ensure the config belongs to the correct workspace
    string SmtpHost,
    int SmtpPort,
    bool UseSsl,
    string Username,
    string FromEmail,
    string Password, 
    string DisplayName
) : IRequest<UpdateEmailConfigurationResponse>;

public record UpdateEmailConfigurationResponse(bool Success, string? Message = default);