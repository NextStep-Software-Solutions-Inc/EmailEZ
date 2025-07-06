using EmailEZ.Domain.Common;

namespace EmailEZ.Domain.Entities;

public class Workspace : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string ApiKeyHash { get; set; } = string.Empty;
    public string Domain { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTimeOffset ApiKeyLastUsedAt { get; set; }

    // Navigation properties
    public ICollection<Email> Emails { get; set; } = new List<Email>();
    public ICollection<AuditLog> AuditLogs { get; set; } = new List<AuditLog>();
}