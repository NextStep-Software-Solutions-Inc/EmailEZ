using MediatR;

namespace EmailEZ.Application.Features.EmailConfigurations.Commands.UpdateEmailConfiguration;

public record UpdateEmailConfigurationCommand(
    Guid Id, // The ID of the configuration to update
    Guid TenantId, // To ensure the config belongs to the correct tenant
    string SmtpHost,
    int SmtpPort,
    bool UseSsl,
    string Username,
    string Password, // REMINDER: This will be encrypted before saving
    string DisplayName
) : IRequest<UpdateEmailConfigurationResponse>;

public record UpdateEmailConfigurationResponse(bool Success, string? Message = default);