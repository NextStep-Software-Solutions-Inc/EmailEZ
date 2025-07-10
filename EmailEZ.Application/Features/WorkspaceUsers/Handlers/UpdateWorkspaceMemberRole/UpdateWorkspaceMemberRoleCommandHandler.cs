using MediatR;
using EmailEZ.Application.Interfaces;
using EmailEZ.Application.Features.WorkspaceUsers.Commands.UpdateWorkspaceMemberRoleCommand;

namespace EmailEZ.Application.Features.WorkspaceUsers.Handlers.UpdateWorkspaceMemberRole;
public class UpdateWorkspaceMemberRoleCommandHandler : IRequestHandler<UpdateWorkspaceMemberRoleCommand, UpdateWorkspaceMemberRoleResponse>
{
    private readonly IUnitOfWork _unitOfWork;

    public UpdateWorkspaceMemberRoleCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<UpdateWorkspaceMemberRoleResponse> Handle(UpdateWorkspaceMemberRoleCommand request, CancellationToken cancellationToken)
    {
        var member = await _unitOfWork.WorkspaceUsers.FirstOrDefaultAsync(wu => wu.Id == request.MemberId, cancellationToken);

        if (member == null)
        {
            return new UpdateWorkspaceMemberRoleResponse { Success = false, Message = "Member not found." };
        }

        member.Role = request.Role;
        _unitOfWork.WorkspaceUsers.Update(member);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new UpdateWorkspaceMemberRoleResponse { Success = true, Message = "Role updated successfully." };
    }
}