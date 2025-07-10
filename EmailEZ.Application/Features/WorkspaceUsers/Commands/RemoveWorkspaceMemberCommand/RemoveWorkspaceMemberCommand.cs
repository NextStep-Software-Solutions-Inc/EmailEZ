using MediatR;

namespace EmailEZ.Application.Features.WorkspaceUsers.Commands.RemoveWorkspaceMemberCommand;
public record RemoveWorkspaceMemberCommand(
    Guid WorkspaceId,
    Guid MemberId
) : IRequest<RemoveWorkspaceMemberResponse>;

public class RemoveWorkspaceMemberResponse
{
    public bool Success { get; set; }
    public string? Message { get; set; }
}