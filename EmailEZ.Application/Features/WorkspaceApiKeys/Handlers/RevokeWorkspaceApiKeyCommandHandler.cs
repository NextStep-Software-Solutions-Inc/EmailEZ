using MediatR;
using EmailEZ.Application.Interfaces;
using EmailEZ.Application.Features.WorkspaceApiKeys.Commands;

namespace EmailEZ.Application.Features.WorkspaceApiKeys.Handlers;

public class RevokeWorkspaceApiKeyCommandHandler : IRequestHandler<RevokeWorkspaceApiKeyCommand, RevokeWorkspaceApiKeyResponse>
{
    private readonly IUnitOfWork _unitOfWork;

    public RevokeWorkspaceApiKeyCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<RevokeWorkspaceApiKeyResponse> Handle(RevokeWorkspaceApiKeyCommand request, CancellationToken cancellationToken)
    {
        var apiKey = await _unitOfWork.WorkspaceApiKeys.FirstOrDefaultAsync(wak => wak.Id == request.ApiKeyId && wak.IsActive, cancellationToken);
        if (apiKey == null) return new RevokeWorkspaceApiKeyResponse(false, "API key not found.");

        apiKey.IsActive = false;
        _unitOfWork.WorkspaceApiKeys.Update(apiKey);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return new RevokeWorkspaceApiKeyResponse(true, "API key revoked successfully.");
    }
}