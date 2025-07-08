using MediatR;

namespace EmailEZ.Application.Features.EmailConfigurations.Queries.GetEmailConfigurationById;

public record GetEmailConfigurationByIdQuery(Guid WorkspaceId, Guid Id) : IRequest<GetEmailConfigurationByIdResponse?>;

public record GetEmailConfigurationByIdResponse(
    Guid EmailConfigurationId,
    Guid WorkspaceId,
    string SmtpHost,
    int SmtpPort,
    bool UseSsl,
    string Username,
    string FromEmail,
    string DisplayName,
    DateTimeOffset CreatedAtUtc
// Note: Password is NOT returned for security reasons
);