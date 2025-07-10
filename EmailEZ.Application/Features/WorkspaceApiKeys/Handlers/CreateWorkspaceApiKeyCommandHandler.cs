using MediatR;
using EmailEZ.Application.Interfaces;
using EmailEZ.Domain.Entities;
using EmailEZ.Application.Features.WorkspaceApiKeys.Commands;


namespace EmailEZ.Application.Features.WorkspaceApiKeys.Handlers;

public class CreateWorkspaceApiKeyCommandHandler : IRequestHandler<CreateWorkspaceApiKeyCommand, CreateWorkspaceApiKeyResponse>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IApiKeyHasher _apiKeyHasher;

    public CreateWorkspaceApiKeyCommandHandler(IUnitOfWork unitOfWork, IApiKeyHasher apiKeyHasher)
    {
        _unitOfWork = unitOfWork;
        _apiKeyHasher = apiKeyHasher;
    }

    public async Task<CreateWorkspaceApiKeyResponse> Handle(CreateWorkspaceApiKeyCommand request, CancellationToken cancellationToken)
    {
        var workspaceUser = await _unitOfWork.WorkspaceUsers.FirstOrDefaultAsync(x => x.UserId == request.UserId, cancellationToken) ?? throw new InvalidOperationException("User is not a member of the workspace.");

        // check if name already exists for the user in the workspace
        var existingApiKeyName = await _unitOfWork.WorkspaceApiKeys
            .FirstOrDefaultAsync(x => x.WorkspaceUserId == workspaceUser.Id && x.Name == request.Name && x.IsActive, cancellationToken);
        if (existingApiKeyName != null)
        {
            return new CreateWorkspaceApiKeyResponse(
                ApiKeyId: Guid.Empty,
                PlainKey: "",
                Success: false,
                Message: "An API key with this name already exists for the user in the workspace."
            );
        }

        var plainKey = Guid.NewGuid().ToString("N") + Guid.NewGuid().ToString("N");
        var hash = _apiKeyHasher.HashApiKey(plainKey);
        var fastHash = _apiKeyHasher.ComputeFastHash(plainKey); // Optional, if you want to store a fast hash for quick lookups

        var apiKey = new WorkspaceApiKey
        {
            WorkspaceUserId = workspaceUser.Id,
            WorkspaceId = request.WorkspaceId,
            ApiKeyHash = hash,
            ApiKeyFastHash = fastHash,
            IsActive = true,
            Name = request.Name
        };

        await _unitOfWork.WorkspaceApiKeys.AddAsync(apiKey, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new CreateWorkspaceApiKeyResponse(
            ApiKeyId: apiKey.Id,
            PlainKey: plainKey,
            Success: true
        );
    }
}