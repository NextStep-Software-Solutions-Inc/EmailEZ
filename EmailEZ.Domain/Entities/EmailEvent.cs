using EmailEZ.Domain.Common;
using EmailEZ.Domain.Enums;

namespace EmailEZ.Domain.Entities;

public class EmailEvent : BaseEntity
{
    public Guid EmailId { get; set; }
    public Guid TenantId { get; set; } // Explicitly defined TenantId

    public EmailEventType EventType { get; set; }
    // We'll use BaseEntity.CreatedAt for the event timestamp.
    public string? Details { get; set; }

    // Navigation property
    public Email Email { get; set; } = null!;
}