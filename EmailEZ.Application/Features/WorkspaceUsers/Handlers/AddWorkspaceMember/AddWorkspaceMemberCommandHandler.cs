using MediatR;
using EmailEZ.Application.Interfaces;
using EmailEZ.Domain.Entities;
using EmailEZ.Application.Features.WorkspaceUsers.Commands.AddWorkspaceMemberCommand;

namespace EmailEZ.Application.Features.WorkspaceUsers.Handlers.AddWorkspaceMember;
public class AddWorkspaceMemberCommandHandler : IRequestHandler<AddWorkspaceMemberCommand, AddWorkspaceMemberResponse>
{
    private readonly IUnitOfWork _unitOfWork;

    public AddWorkspaceMemberCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<AddWorkspaceMemberResponse> Handle(AddWorkspaceMemberCommand request, CancellationToken cancellationToken)
    {
        var exists = await _unitOfWork.WorkspaceUsers
            .AnyAsync(wu => wu.WorkspaceId == request.WorkspaceId && wu.UserId == request.UserId, cancellationToken);

        if (exists)
        {
            return new AddWorkspaceMemberResponse { Success = false, Message = "User is already a member." };
        }

        var member = new WorkspaceUser
        {
            WorkspaceId = request.WorkspaceId,
            UserId = request.UserId,
            Role = request.Role
        };

        await _unitOfWork.WorkspaceUsers.AddAsync(member, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new AddWorkspaceMemberResponse { Success = true, Message = "Member added successfully." };
    }
}