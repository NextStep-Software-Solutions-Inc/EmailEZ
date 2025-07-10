using EmailEZ.Application.Specifications;
using EmailEZ.Domain.Entities;
using EmailEZ.Domain.Enums;

namespace EmailEZ.Application.Specifications.WorkspaceSpecifications;

/// <summary>
/// Specification for finding active workspaces by domain pattern.
/// </summary>
public class ActiveWorkspacesByDomainSpecification : BaseSpecification<Workspace>
{
    public ActiveWorkspacesByDomainSpecification(string domainPattern)
    {
        AddCriteria(w => w.IsActive && w.Domain.Contains(domainPattern));
        AddOrderBy(w => w.Name);
    }
}

/// <summary>
/// Specification for workspaces with recent API key usage.
/// </summary>
public class RecentlyUsedWorkspacesSpecification : BaseSpecification<Workspace>
{
    public RecentlyUsedWorkspacesSpecification(TimeSpan withinPeriod)
    {
        var cutoffDate = DateTimeOffset.UtcNow.Subtract(withinPeriod);
        AddCriteria(w => w.IsActive);
    }
}

/// <summary>
/// Specification for workspaces with user details.
/// </summary>
public class WorkspacesWithUsersSpecification : BaseSpecification<Workspace>
{
    public WorkspacesWithUsersSpecification(bool includeUserDetails = true)
    {
        AddCriteria(w => w.IsActive);
        
        if (includeUserDetails)
        {
            AddInclude(w => w.WorkspaceUsers);
        }
        
        AddOrderBy(w => w.Name);
    }
}

/// <summary>
/// Specification for workspace users by role.
/// </summary>
public class WorkspaceUsersByRoleSpecification : BaseSpecification<WorkspaceUser>
{
    public WorkspaceUsersByRoleSpecification(Guid workspaceId, WorkspaceUserRole role)
    {
        AddCriteria(wu => wu.WorkspaceId == workspaceId && wu.Role == role);
        AddInclude(wu => wu.Workspace);
        AddOrderBy(wu => wu.CreatedAt);
    }
}

/// <summary>
/// Specification for finding workspace owners across multiple workspaces.
/// </summary>
public class WorkspaceOwnersSpecification : BaseSpecification<WorkspaceUser>
{
    public WorkspaceOwnersSpecification(string? userId = null)
    {
        if (!string.IsNullOrEmpty(userId))
        {
            AddCriteria(wu => wu.Role == WorkspaceUserRole.Owner && wu.UserId == userId);
        }
        else
        {
            AddCriteria(wu => wu.Role == WorkspaceUserRole.Owner);
        }
        
        AddInclude(wu => wu.Workspace);
        AddOrderBy(wu => wu.Workspace.Name);
    }
}

/// <summary>
/// Specification for users with multiple workspace memberships.
/// </summary>
public class MultiWorkspaceUsersSpecification : BaseSpecification<WorkspaceUser>
{
    public MultiWorkspaceUsersSpecification(int minWorkspaces = 2)
    {
        // This would typically be implemented with a group by query
        // For now, we'll get all workspace users and filter in the service layer
        AddInclude(wu => wu.Workspace);
        AddOrderBy(wu => wu.UserId);
        AddOrderBy(wu => wu.WorkspaceId);
    }
}