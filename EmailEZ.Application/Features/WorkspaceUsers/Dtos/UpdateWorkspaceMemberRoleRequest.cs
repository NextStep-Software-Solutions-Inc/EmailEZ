using EmailEZ.Domain.Enums;

namespace EmailEZ.Application.Features.WorkspaceUsers.Dtos;
public class UpdateWorkspaceMemberRoleRequest
{
    public WorkspaceUserRole Role { get; set; }
}