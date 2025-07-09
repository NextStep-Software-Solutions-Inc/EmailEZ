using EmailEZ.Domain.Entities.Common;

namespace EmailEZ.Domain.Entities;

public class WorkspaceApiKey : BaseEntity
{
    public Guid WorkspaceUserId { get; set; } // Foreign key to WorkspaceUser
    public string ApiKeyHash { get; set; } = string.Empty;
    public DateTimeOffset? LastUsedAt { get; set; }
    public bool IsActive { get; set; } = true;
    public string? Name { get; set; }

    // Navigation property
    public virtual WorkspaceUser WorkspaceUser { get; set; } = null!;
}