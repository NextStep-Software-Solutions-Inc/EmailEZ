using MediatR;
using EmailEZ.Domain.Entities;
using EmailEZ.Application.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace EmailEZ.Application.Features.WorkspaceUsers.Queries.GetAllWorkspaceMembers;
public class GetAllWorkspaceMembersQueryHandler : IRequestHandler<GetAllWorkspaceMembersQuery, List<WorkspaceUser>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetAllWorkspaceMembersQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<List<WorkspaceUser>> Handle(GetAllWorkspaceMembersQuery request, CancellationToken cancellationToken)
    {
        var membersQuery = _unitOfWork.WorkspaceUsers.Query().AsNoTracking()
            .Where(wu => wu.WorkspaceId == request.WorkspaceId);

        var members = await _unitOfWork.WorkspaceUsers.ToListAsync(membersQuery, cancellationToken);

        return members.ToList();
    }
}