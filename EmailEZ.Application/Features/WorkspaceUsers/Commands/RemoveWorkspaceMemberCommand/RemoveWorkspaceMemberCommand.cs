using MediatR;

namespace EmailEZ.Application.Features.WorkspaceUsers.Commands.RemoveWorkspaceMemberCommand;
public record RemoveWorkspaceMemberCommand(
    Guid WorkspaceId,
    string UserId
) : IRequest<RemoveWorkspaceMemberResponse>;

public class RemoveWorkspaceMemberResponse
{
    public bool Success { get; set; }
    public string? Message { get; set; }
}