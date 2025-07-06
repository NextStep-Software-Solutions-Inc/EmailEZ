using MediatR;

namespace EmailEZ.Application.Features.EmailConfigurations.Queries.GetAllEmailConfigurations;

public record GetAllEmailConfigurationsQuery(Guid WorkspaceId) : IRequest<List<GetAllEmailConfigurationsResponse>>;

public record GetAllEmailConfigurationsResponse(
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