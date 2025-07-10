using EmailEZ.Application.Interfaces;
using EmailEZ.Domain.Entities;

namespace EmailEZ.Application.Services;

public class WorkspaceApiKeyService : IWorkspaceApiKeyService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly IApiKeyHasher _apiKeyHasher;

    public WorkspaceApiKeyService(IUnitOfWork unitOfWork, ICurrentUserService currentUserService, IApiKeyHasher apiKeyHasher)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
        _apiKeyHasher = apiKeyHasher;
    }
    private Guid WorkspaceId => _currentUserService.GetCurrentWorkspaceId() ?? throw new InvalidOperationException("Current user is not associated with any workspace.");
    public async Task<string> CreateApiKeyAsync(Guid workspaceUserId, string? name, CancellationToken cancellationToken)
    {
        var plainKey = Guid.NewGuid().ToString("N") + Guid.NewGuid().ToString("N"); // 64-char random
        var hash = _apiKeyHasher.HashApiKey(plainKey);
        var fastHash = _apiKeyHasher.ComputeFastHash(plainKey);

        var apiKey = new WorkspaceApiKey
        {
            WorkspaceUserId = workspaceUserId,
            WorkspaceId = WorkspaceId,
            ApiKeyHash = hash,
            ApiKeyFastHash = fastHash,
            Name = name,
            IsActive = true
        };

        await _unitOfWork.WorkspaceApiKeys.AddAsync(apiKey, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return plainKey; // Show only once!
    }

    public async Task<bool> RevokeApiKeyAsync(Guid apiKeyId, CancellationToken cancellationToken)
    {
        var apiKey = await _unitOfWork.WorkspaceApiKeys.GetByIdAsync(apiKeyId, cancellationToken);
        if (apiKey == null) return false;
        apiKey.IsActive = false;
        _unitOfWork.WorkspaceApiKeys.Update(apiKey);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<List<WorkspaceApiKey>> ListApiKeysAsync(Guid workspaceUserId, CancellationToken cancellationToken)
    {
        var waksQuery = _unitOfWork.WorkspaceApiKeys.Query()
            .Where(x => x.WorkspaceUserId == workspaceUserId);

        var waks = await _unitOfWork.WorkspaceApiKeys.ToListAsync(waksQuery, cancellationToken);

        return waks.ToList();
    }

    public async Task<string> RegenerateApiKeyAsync(Guid apiKeyId, CancellationToken cancellationToken)
    {
        var apiKey = await _unitOfWork.WorkspaceApiKeys.GetByIdAsync(apiKeyId, cancellationToken);
        if (apiKey == null) throw new InvalidOperationException("API key not found.");

        var newPlainKey = Guid.NewGuid().ToString("N") + Guid.NewGuid().ToString("N");
        apiKey.ApiKeyHash = _apiKeyHasher.HashApiKey(newPlainKey);
        apiKey.IsActive = true;
        _unitOfWork.WorkspaceApiKeys.Update(apiKey);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return newPlainKey;
    }

    public async Task<bool> ValidateApiKeyAsync(string apiKey, CancellationToken cancellationToken)
    {
        var apiKeysQuery = _unitOfWork.WorkspaceApiKeys
                .Query()
                .Where(x => x.IsActive);

        var apiKeys = await _unitOfWork.WorkspaceApiKeys.ToListAsync(apiKeysQuery, cancellationToken);

        // Check if the provided API key matches any of the stored hashes
        return apiKeys.Any(x => _apiKeyHasher.VerifyApiKey(apiKey, x.ApiKeyHash));
    }

   

}