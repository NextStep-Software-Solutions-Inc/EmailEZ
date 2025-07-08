using EmailEZ.Domain.Enums;

namespace EmailEZ.Application.Features.WorkspaceUsers.Dtos;

public class AddWorkspaceMemberRequest
{
    public required string UserId { get; set; }
    public WorkspaceUserRole Role { get; set; }
}