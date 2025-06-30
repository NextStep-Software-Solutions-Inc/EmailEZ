using EmailEZ.Domain.Entities; // For EmailConfiguration

namespace EmailEZ.Application.Interfaces;

/// <summary>
/// Defines the contract for sending emails.
/// </summary>
public interface IEmailSender
{
    Task<EmailSendResult> SendEmailAsync(EmailMessage message, EmailConfiguration config);
}

/// <summary>
/// Represents the details of an email message to be sent.
/// </summary>
public class EmailMessage
{
    public required List<string> To { get; set; }
    public List<string>? Cc { get; set; } // Optional: Carbon Copy recipients
    public List<string>? Bcc { get; set; } // Optional: Blind Carbon Copy recipients
    public required string Subject { get; set; }
    public required string Body { get; set; }
    public bool IsHtml { get; set; } = true; // Default to HTML body
    public string? FromDisplayName { get; set; } // Optional: Overrides config's DisplayName if provided
}

/// <summary>
/// Represents the result of an email sending operation.
/// </summary>
public class EmailSendResult
{
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
    public string? SmtpResponse { get; set; } // Full response from the SMTP server
}