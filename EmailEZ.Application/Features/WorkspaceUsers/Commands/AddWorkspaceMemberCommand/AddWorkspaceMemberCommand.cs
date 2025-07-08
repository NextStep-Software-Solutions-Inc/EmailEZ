using MediatR;
using EmailEZ.Domain.Enums;

namespace EmailEZ.Application.Features.WorkspaceUsers.Commands.AddWorkspaceMemberCommand;
public record AddWorkspaceMemberCommand(
    Guid WorkspaceId,
    string UserId,
    WorkspaceUserRole Role
) : IRequest<AddWorkspaceMemberResponse>;

public record AddWorkspaceMemberResponse
{
    public bool Success { get; set; }
    public string? Message { get; set; }
}