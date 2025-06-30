using EmailEZ.Domain.Enums;

namespace EmailEZ.Application.Features.Emails.Queries.GetEmails;

public class EmailDto
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public string FromAddress { get; set; } = string.Empty;
    public List<string> ToAddresses { get; set; } = new List<string>();
    public string Subject { get; set; } = string.Empty;
    public EmailStatus Status { get; set; }
    public string? ErrorMessage { get; set; }
    public DateTimeOffset QueuedAt { get; set; }
    public DateTimeOffset? SentAt { get; set; }
    public int AttemptCount { get; set; }

    // Optional: A snippet of the body for list view
    public string? BodySnippet { get; set; }
    public bool IsHtml { get; set; } // To know if original body was HTML
}