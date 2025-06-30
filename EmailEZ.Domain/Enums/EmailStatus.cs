namespace EmailEZ.Domain.Enums;

/// <summary>
/// Represents the current status of an email in the system.
/// </summary>
public enum EmailStatus
{
    /// <summary>
    /// The email has been received by the API and queued for sending.
    /// </summary>
    Queued = 1,

    /// <summary>
    /// The email is currently being processed and sent by the background job.
    /// </summary>
    Sending = 2,

    /// <summary>
    /// The email has been successfully handed over to the SMTP server.
    /// (Does not necessarily mean it was delivered to the recipient's inbox).
    /// </summary>
    Sent = 3,

    /// <summary>
    /// The attempt to send the email failed due to a transient or permanent error.
    /// </summary>
    Failed = 4,

    /// <summary>
    /// The email was successfully delivered to the recipient's inbox (typically confirmed via webhooks/bounce processing).
    /// </summary>
    Delivered = 5,

    /// <summary>
    /// The email bounced (e.g., recipient address does not exist). Could be soft or hard bounce.
    /// </summary>
    Bounced = 6,

    /// <summary>
    /// The recipient marked the email as spam/junk.
    /// </summary>
    Complaint = 7
}