using EmailEZ.Domain.Common;
using EmailEZ.Domain.Enums;

namespace EmailEZ.Domain.Entities;

public class AuditLog : BaseEntity
{
    public Guid? TenantId { get; set; } // Explicitly defined TenantId (nullable here)

    public AuditEventType EventType { get; set; }
    // We'll use BaseEntity.CreatedAt for the audit timestamp.
    public string PerformedBy { get; set; } = string.Empty;
    public string? Details { get; set; }

    // Navigation property (Optional, because TenantId is nullable)
    public Tenant? Tenant { get; set; }
}