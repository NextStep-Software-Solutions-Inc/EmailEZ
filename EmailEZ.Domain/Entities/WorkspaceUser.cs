using EmailEZ.Domain.Common;
using EmailEZ.Domain.Enums;

namespace EmailEZ.Domain.Entities;

public class WorkspaceUser : BaseEntity
{
    public string UserId { get; set; } = string.Empty;
    public Guid WorkspaceId { get; set; } 
    public WorkspaceUserRole Role { get; set; }
    public virtual Workspace Workspace { get; set; } = null!;
}
