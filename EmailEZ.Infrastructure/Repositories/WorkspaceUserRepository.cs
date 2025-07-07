using EmailEZ.Application.Interfaces;
using EmailEZ.Domain.Entities;
using EmailEZ.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace EmailEZ.Infrastructure.Repositories;

/// <summary>
/// Repository implementation for WorkspaceUser entities with custom operations.
/// </summary>
public class WorkspaceUserRepository : GenericRepository<WorkspaceUser>, IWorkspaceUserRepository
{
    public WorkspaceUserRepository(IApplicationDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<WorkspaceUser>> GetByWorkspaceIdAsync(Guid workspaceId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(wu => wu.WorkspaceId == workspaceId)
            .Include(wu => wu.Workspace)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<WorkspaceUser>> GetByUserIdAsync(string userId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(wu => wu.UserId == userId)
            .Include(wu => wu.Workspace)
            .ToListAsync(cancellationToken);
    }

    public async Task<WorkspaceUser?> GetByWorkspaceAndUserAsync(Guid workspaceId, string userId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(wu => wu.WorkspaceId == workspaceId && wu.UserId == userId)
            .Include(wu => wu.Workspace)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<bool> HasRoleAsync(Guid workspaceId, string userId, WorkspaceUserRole role, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .AnyAsync(wu => wu.WorkspaceId == workspaceId && wu.UserId == userId && wu.Role == role, cancellationToken);
    }

    public async Task<bool> HasMinimumRoleAsync(Guid workspaceId, string userId, WorkspaceUserRole minimumRole, CancellationToken cancellationToken = default)
    {
        var workspaceUser = await _dbSet
            .FirstOrDefaultAsync(wu => wu.WorkspaceId == workspaceId && wu.UserId == userId, cancellationToken);

        if (workspaceUser == null)
            return false;

        // Lower enum values represent higher privileges (Owner = 1, Admin = 2, Member = 3)
        return workspaceUser.Role <= minimumRole;
    }

    public async Task<WorkspaceUser?> GetOwnerAsync(Guid workspaceId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(wu => wu.WorkspaceId == workspaceId && wu.Role == WorkspaceUserRole.Owner)
            .Include(wu => wu.Workspace)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<WorkspaceUser?> UpdateRoleAsync(Guid workspaceId, string userId, WorkspaceUserRole newRole, CancellationToken cancellationToken = default)
    {
        var workspaceUser = await _dbSet
            .FirstOrDefaultAsync(wu => wu.WorkspaceId == workspaceId && wu.UserId == userId, cancellationToken);

        if (workspaceUser == null)
            return null;

        workspaceUser.Role = newRole;
        // ? FIXED: Do NOT call SaveChangesAsync here
        // Let the Unit of Work control when to save

        return workspaceUser;
    }
}