using MediatR;

namespace EmailEZ.Application.Features.Emails.Commands.SendEmail;

public record SendEmailCommand(
    Guid TenantId,
    Guid EmailConfigurationId, // The ID of the specific email config to use
    string ToEmail,
    string Subject,
    string Body,
    bool IsHtml,
    string? FromDisplayName // Optional: Overrides the config's DisplayName if provided
) : IRequest<SendEmailResponse>;

public record SendEmailResponse(
    bool Success,
    string? Message = default,
    string? HangfireJobId = null // To track the background job
);