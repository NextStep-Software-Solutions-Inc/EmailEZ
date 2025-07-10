using EmailEZ.Domain.Entities.Common;

namespace EmailEZ.Domain.Entities;

public class WorkspaceApiKey : BaseEntity
{
    public Guid WorkspaceUserId { get; set; } // Foreign key to WorkspaceUser
    public Guid WorkspaceId { get; set; } // Foreign key to Workspace for filtering
    public string ApiKeyHash { get; set; } = string.Empty;
    public DateTimeOffset? LastUsedAt { get; set; }
    public bool IsActive { get; set; } = true;
    public string? Name { get; set; }
    public required string ApiKeyFastHash { get; set; } // Fast hash for quick lookups

    // Navigation property
    public virtual WorkspaceUser WorkspaceUser { get; set; } = null!;
    public virtual Workspace Workspace { get; set; } = null!;
}