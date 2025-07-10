
using EmailEZ.Domain.Entities.Common;

namespace EmailEZ.Domain.Entities;

public class Workspace : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Domain { get; set; } = string.Empty;
    public bool IsActive { get; set; }

    // Navigation properties
    public virtual ICollection<Email> Emails { get; set; } = [];
    public virtual ICollection<AuditLog> AuditLogs { get; set; } = [];
    public virtual ICollection<EmailConfiguration> EmailConfigurations { get; set; } = [];
    public virtual ICollection<WorkspaceUser> WorkspaceUsers { get; set; } = [];
    public virtual ICollection<WorkspaceApiKey> WorkspaceApiKeys { get; set; } = [];

}