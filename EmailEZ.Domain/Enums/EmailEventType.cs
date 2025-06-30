namespace EmailEZ.Domain.Enums;

/// <summary>
/// Represents different events in the lifecycle of an email for historical tracking.
/// </summary>
public enum EmailEventType
{
    /// <summary>
    /// The email was initially queued by the application.
    /// </summary>
    Queued = 1,

    /// <summary>
    /// The email sending process has started.
    /// </summary>
    Sending = 2,

    /// <summary>
    /// The email was successfully handed over to the SMTP server.
    /// </summary>
    Sent = 3,

    /// <summary>
    /// An attempt to send the email failed.
    /// </summary>
    Failed = 4,

    /// <summary>
    /// The email was successfully delivered to the recipient's inbox.
    /// </summary>
    Delivered = 5,

    /// <summary>
    /// The email bounced back (e.g., invalid address, mailbox full).
    /// </summary>
    Bounced = 6,

    /// <summary>
    /// The recipient marked the email as spam/junk.
    /// </summary>
    Complaint = 7,

    /// <summary>
    /// The email was opened by the recipient (requires tracking pixel).
    /// </summary>
    Opened = 8,

    /// <summary>
    /// A link within the email was clicked by the recipient (requires tracking links).
    /// </summary>
    Clicked = 9,

    /// <summary>
    /// The email was suppressed and not sent (e.g., due to previous hard bounce or complaint).
    /// </summary>
    Suppressed = 10,

    /// <summary>
    /// The background job retried sending the email after a temporary failure.
    /// </summary>
    Retried = 11,

    /// <summary>
    /// A temporary delivery failure (e.g., mailbox full), might be retried later.
    /// </summary>
    SoftBounce = 12,

    /// <summary>
    /// A permanent delivery failure (e.g., invalid address), usually won't be retried.
    /// </summary>
    HardBounce = 13
}
