using EmailEZ.Domain.Enums;

namespace EmailEZ.Application.Features.Emails.Dtos;

public class EmailDto
{
    public Guid Id { get; set; }
    public Guid WorkspaceId { get; set; }
    public required string FromAddress { get; set; }
    public List<string> ToAddresses { get; set; } = new List<string>();
    public string Subject { get; set; } = string.Empty;
    public required string Status { get; set; } 
    public string? ErrorMessage { get; set; }
    public DateTimeOffset QueuedAt { get; set; }
    public DateTimeOffset? SentAt { get; set; }
    public int AttemptCount { get; set; }

    public string? BodySnippet { get; set; }
    public bool IsHtml { get; set; } 
}