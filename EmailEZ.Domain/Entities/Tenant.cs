using EmailEZ.Domain.Common;

namespace EmailEZ.Domain.Entities;

public class Tenant : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string ApiKeyHash { get; set; } = string.Empty;
    public string Domain { get; set; } = string.Empty;

    public string SmtpHost { get; set; } = string.Empty;
    public int SmtpPort { get; set; }
    public string SmtpUsername { get; set; } = string.Empty;
    public string SmtpPasswordEncrypted { get; set; } = string.Empty;
    public bool SmtpEnableSsl { get; set; }
    public bool IsActive { get; set; }

    // Navigation properties
    public ICollection<Email> Emails { get; set; } = new List<Email>();
    public ICollection<AuditLog> AuditLogs { get; set; } = new List<AuditLog>();
}