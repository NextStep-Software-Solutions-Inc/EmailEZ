using MediatR;
using EmailEZ.Domain.Enums;

namespace EmailEZ.Application.Features.WorkspaceUsers.Commands.UpdateWorkspaceMemberRoleCommand;
public record UpdateWorkspaceMemberRoleCommand(
    Guid WorkspaceId,
    string UserId,
    WorkspaceUserRole Role
) : IRequest<UpdateWorkspaceMemberRoleResponse>;

public class UpdateWorkspaceMemberRoleResponse
{
    public bool Success { get; set; }
    public string? Message { get; set; }
}