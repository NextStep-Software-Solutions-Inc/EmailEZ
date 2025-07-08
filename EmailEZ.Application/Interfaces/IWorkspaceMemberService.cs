using EmailEZ.Domain.Entities;
using EmailEZ.Domain.Enums;

namespace EmailEZ.Application.Interfaces;

public interface IWorkspaceMemberService
{
    Task AddMemberAsync(Guid workspaceId, string userId, WorkspaceUserRole role, CancellationToken cancellationToken);
    Task RemoveMemberAsync(Guid workspaceId, string userId, CancellationToken cancellationToken);
    Task UpdateMemberRoleAsync(Guid workspaceId, string userId, WorkspaceUserRole role, CancellationToken cancellationToken);
    Task<List<WorkspaceUser>> ListMembersAsync(Guid workspaceId, CancellationToken cancellationToken);
}