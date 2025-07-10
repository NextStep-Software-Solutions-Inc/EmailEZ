using MediatR;
using EmailEZ.Domain.Entities;
using EmailEZ.Application.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace EmailEZ.Application.Features.WorkspaceUsers.Queries.GetAllWorkspaceMembers;
public class GetAllWorkspaceMembersQueryHandler : IRequestHandler<GetAllWorkspaceMembersQuery, List<GetAllWorkspaceMemberResponse>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetAllWorkspaceMembersQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<List<GetAllWorkspaceMemberResponse>> Handle(GetAllWorkspaceMembersQuery request, CancellationToken cancellationToken)
    {
        var membersQuery = _unitOfWork.WorkspaceUsers.Query().AsNoTracking()
            .Where(wu => wu.WorkspaceId == request.WorkspaceId)
            .Select(wu => new GetAllWorkspaceMemberResponse(
                wu.Id,
                wu.UserId,
                wu.WorkspaceId,
                wu.Role.ToString(),
                wu.CreatedAt,
                wu.UpdatedAt
            ));

        var members = await _unitOfWork.WorkspaceUsers.ToListAsync(membersQuery, cancellationToken);

        return members.ToList();
    }
}