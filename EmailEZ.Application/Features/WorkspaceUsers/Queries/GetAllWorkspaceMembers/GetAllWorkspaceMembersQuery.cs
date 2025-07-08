using MediatR;
using EmailEZ.Domain.Entities;

namespace EmailEZ.Application.Features.WorkspaceUsers.Queries.GetAllWorkspaceMembers;
public record GetAllWorkspaceMembersQuery(
    Guid WorkspaceId
) : IRequest<List<WorkspaceUser>>;