using MediatR;
using EmailEZ.Application.Interfaces;
using EmailEZ.Application.Features.WorkspaceApiKeys.Commands;

namespace EmailEZ.Application.Features.WorkspaceApiKeys.Handlers;

public class RegenerateWorkspaceApiKeyCommandHandler : IRequestHandler<RegenerateWorkspaceApiKeyCommand, RegenerateWorkspaceApiKeyResponse>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IApiKeyHasher _apiKeyHasher;

    public RegenerateWorkspaceApiKeyCommandHandler(IUnitOfWork unitOfWork, IApiKeyHasher apiKeyHasher)
    {
        _unitOfWork = unitOfWork;
        _apiKeyHasher = apiKeyHasher;
    }

    public async Task<RegenerateWorkspaceApiKeyResponse> Handle(RegenerateWorkspaceApiKeyCommand request, CancellationToken cancellationToken)
    {
        var apiKey = await _unitOfWork.WorkspaceApiKeys.GetByIdAsync(request.ApiKeyId, cancellationToken);
        if (apiKey == null)
        {
            return new RegenerateWorkspaceApiKeyResponse(Guid.Empty, string.Empty, false, "API key not found.");
        } 

        var newPlainKey = Guid.NewGuid().ToString("N") + Guid.NewGuid().ToString("N");
        var hashApiKey = _apiKeyHasher.HashApiKey(newPlainKey);
        var fastHash = _apiKeyHasher.ComputeFastHash(newPlainKey);

        apiKey.ApiKeyHash = hashApiKey;
        apiKey.ApiKeyFastHash = fastHash;
        apiKey.IsActive = true;
        _unitOfWork.WorkspaceApiKeys.Update(apiKey);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        
        return new RegenerateWorkspaceApiKeyResponse(apiKey.Id, newPlainKey, true)
        {
            Message = "API key regenerated successfully."
        };
    }
}