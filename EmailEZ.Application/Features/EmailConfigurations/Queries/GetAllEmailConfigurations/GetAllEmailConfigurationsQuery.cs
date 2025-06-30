using MediatR;

namespace EmailEZ.Application.Features.EmailConfigurations.Queries.GetAllEmailConfigurations;

public record GetAllEmailConfigurationsQuery(Guid TenantId) : IRequest<List<GetAllEmailConfigurationsResponse>>;

public record GetAllEmailConfigurationsResponse(
    Guid EmailConfigurationId,
    Guid TenantId,
    string SmtpHost,
    int SmtpPort,
    bool UseSsl,
    string Username,
    string FromEmail,
    string DisplayName,
    DateTimeOffset CreatedAtUtc
// Note: Password is NOT returned for security reasons
);