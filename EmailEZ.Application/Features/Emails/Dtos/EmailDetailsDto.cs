using EmailEZ.Domain.Enums;

namespace EmailEZ.Application.Features.Emails.Queries.GetEmailById;

public class EmailDetailsDto
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public Guid EmailConfigurationId { get; set; }
    public string FromAddress { get; set; } = string.Empty;
    public List<string> ToAddresses { get; set; } = new List<string>();
    public List<string>? CcAddresses { get; set; }
    public List<string>? BccAddresses { get; set; }
    public string Subject { get; set; } = string.Empty;
    public string? BodyHtml { get; set; } // Full (or truncated) HTML body
    public string? BodyPlainText { get; set; } // Full (or truncated) plain text body
    public EmailStatus Status { get; set; }
    public string? ErrorMessage { get; set; }
    public string? SmtpResponse { get; set; } // Full SMTP response for debugging
    public string? HangfireJobId { get; set; }
    public DateTimeOffset QueuedAt { get; set; }
    public DateTimeOffset? SentAt { get; set; }
    public int AttemptCount { get; set; }
}