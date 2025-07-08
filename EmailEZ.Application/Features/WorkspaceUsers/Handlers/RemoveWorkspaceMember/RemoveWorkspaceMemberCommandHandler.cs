using MediatR;
using EmailEZ.Application.Interfaces;
using EmailEZ.Application.Features.WorkspaceUsers.Commands.RemoveWorkspaceMemberCommand;

namespace EmailEZ.Application.Features.WorkspaceUsers.Handlers.RemoveWorkspaceMember;
public class RemoveWorkspaceMemberCommandHandler : IRequestHandler<RemoveWorkspaceMemberCommand, RemoveWorkspaceMemberResponse>
{
    private readonly IUnitOfWork _unitOfWork;

    public RemoveWorkspaceMemberCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<RemoveWorkspaceMemberResponse> Handle(RemoveWorkspaceMemberCommand request, CancellationToken cancellationToken)
    {
        var member = await _unitOfWork.WorkspaceUsers
            .FirstOrDefaultAsync(wu => wu.WorkspaceId == request.WorkspaceId && wu.UserId == request.UserId, cancellationToken);

        if (member == null)
        {
            return new RemoveWorkspaceMemberResponse { Success = false, Message = "Member not found." };
        }

        _unitOfWork.WorkspaceUsers.HardDelete(member);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new RemoveWorkspaceMemberResponse { Success = true, Message = "Member removed successfully." };
    }
}