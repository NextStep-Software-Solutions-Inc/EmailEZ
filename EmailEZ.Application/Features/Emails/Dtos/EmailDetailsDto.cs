using EmailEZ.Domain.Enums;

namespace EmailEZ.Application.Features.Emails.Dtos;

public class EmailDetailsDto
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public Guid EmailConfigurationId { get; set; }
    public required string FromAddress { get; set; }
    public List<string> ToAddresses { get; set; } = new List<string>();
    public List<string>? CcAddresses { get; set; }
    public List<string>? BccAddresses { get; set; }
    public string Subject { get; set; } = string.Empty;
    public string? BodyHtml { get; set; } 
    public string? BodyPlainText { get; set; } 
    public required string Status { get; set; }
    public string? ErrorMessage { get; set; }
    public string? SmtpResponse { get; set; } 
    public string? HangfireJobId { get; set; }
    public DateTimeOffset QueuedAt { get; set; }
    public DateTimeOffset? SentAt { get; set; }
    public int AttemptCount { get; set; }
}