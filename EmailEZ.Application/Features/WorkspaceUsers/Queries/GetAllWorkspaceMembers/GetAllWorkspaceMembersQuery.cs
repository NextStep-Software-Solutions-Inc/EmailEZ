using MediatR;

namespace EmailEZ.Application.Features.WorkspaceUsers.Queries.GetAllWorkspaceMembers;
public record GetAllWorkspaceMembersQuery(
    Guid WorkspaceId
) : IRequest<List<GetAllWorkspaceMemberResponse>>;

public record GetAllWorkspaceMemberResponse(
    Guid Id,
    string UserId,
    Guid WorkspaceId,
    string Role,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt
);