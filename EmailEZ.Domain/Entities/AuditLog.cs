using EmailEZ.Domain.Entities.Common;
using EmailEZ.Domain.Enums;

namespace EmailEZ.Domain.Entities;

public class AuditLog : BaseEntity
{
    public Guid? WorkspaceId { get; set; } // Explicitly defined WorkspaceId (nullable here)

    public AuditEventType EventType { get; set; }
    // We'll use BaseEntity.CreatedAt for the audit timestamp.
    public string PerformedBy { get; set; } = string.Empty;
    public string? Details { get; set; }

    // Navigation property (Optional, because WorkspaceId is nullable)
    public virtual Workspace? Workspace { get; set; }
}