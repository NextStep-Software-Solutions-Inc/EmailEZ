
using System.Text.Json.Serialization;
using EmailEZ.Domain.Entities.Common;

namespace EmailEZ.Domain.Entities;

public class Workspace : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string ApiKeyHash { get; set; } = string.Empty;
    public string Domain { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTimeOffset ApiKeyLastUsedAt { get; set; }

    // Navigation properties
    [JsonIgnore]
    public virtual ICollection<Email> Emails { get; set; } = new List<Email>();
    
    [JsonIgnore]
    public virtual ICollection<AuditLog> AuditLogs { get; set; } = new List<AuditLog>();

    [JsonIgnore]
    public virtual ICollection<EmailConfiguration> EmailConfigurations { get; set; } = new List<EmailConfiguration>();

    [JsonIgnore]
    public virtual ICollection<WorkspaceUser> WorkspaceUsers { get; set; } = new List<WorkspaceUser>();
}