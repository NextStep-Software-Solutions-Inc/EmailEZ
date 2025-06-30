using EmailEZ.Domain.Common;
using EmailEZ.Domain.Enums;

namespace EmailEZ.Domain.Entities;

public class Email : BaseEntity
{
    public Guid TenantId { get; set; } // Explicitly defined TenantId
    public Guid? ExternalId { get; set; }

    // New: Reference to the EmailConfiguration used for sending
    public Guid EmailConfigurationId { get; set; } // <--- ADD THIS LINE

    public string FromAddress { get; set; } = string.Empty;
    public List<string> ToAddresses { get; set; } = new List<string>();
    public List<string>? CcAddresses { get; set; }
    public List<string>? BccAddresses { get; set; }

    public string Subject { get; set; } = string.Empty;

    public bool IsHtml { get; set; }
    public string? BodyHtml { get; set; }
    public string? BodyPlainText { get; set; }

    public EmailStatus Status { get; set; }
    public string? ErrorMessage { get; set; }

    public string? SmtpResponse { get; set; }

    public string? HangfireJobId { get; set; }

    public DateTimeOffset QueuedAt { get; set; } = DateTimeOffset.UtcNow; // Explicitly initialized here for email specific timestamp
    public DateTimeOffset? SentAt { get; set; }
    public int AttemptCount { get; set; } = 0;

    // Navigation properties
    public Tenant Tenant { get; set; } = null!;

    public EmailConfiguration EmailConfiguration { get; set; } = null!; 
    public ICollection<EmailAttachment> Attachments { get; set; } = new List<EmailAttachment>();
    public ICollection<EmailEvent> Events { get; set; } = new List<EmailEvent>();
}