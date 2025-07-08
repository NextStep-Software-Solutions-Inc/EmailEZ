using EmailEZ.Application.Interfaces;
using EmailEZ.Domain.Entities;
using EmailEZ.Domain.Enums;

public class WorkspaceMemberService : IWorkspaceMemberService
{
    private readonly IUnitOfWork _unitOfWork;

    public WorkspaceMemberService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task AddMemberAsync(Guid workspaceId, string userId, WorkspaceUserRole role, CancellationToken cancellationToken)
    {
        var member = new WorkspaceUser { WorkspaceId = workspaceId, UserId = userId, Role = role };
        await _unitOfWork.WorkspaceUsers.AddAsync(member, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task RemoveMemberAsync(Guid workspaceId, string userId, CancellationToken cancellationToken)
    {
        var member = await _unitOfWork.WorkspaceUsers.FirstOrDefaultAsync(
            wu => wu.WorkspaceId == workspaceId && wu.UserId == userId, cancellationToken);
        if (member != null)
        {
            _unitOfWork.WorkspaceUsers.HardDelete(member);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task UpdateMemberRoleAsync(Guid workspaceId, string userId, WorkspaceUserRole role, CancellationToken cancellationToken)
    {
        var member = await _unitOfWork.WorkspaceUsers.FirstOrDefaultAsync(
            wu => wu.WorkspaceId == workspaceId && wu.UserId == userId, cancellationToken);
        if (member != null)
        {
            member.Role = role;
            _unitOfWork.WorkspaceUsers.Update(member);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task<List<WorkspaceUser>> ListMembersAsync(Guid workspaceId, CancellationToken cancellationToken)
    {
        var workspaceUsersQuery = _unitOfWork.WorkspaceUsers.Query()
            .Where(wu => wu.WorkspaceId == workspaceId);

        var workspaceUsers = await _unitOfWork.WorkspaceUsers.ToListAsync(workspaceUsersQuery, cancellationToken);
        return workspaceUsers.ToList();
    }
}