using EmailEZ.Domain.Entities;

namespace EmailEZ.Application.Interfaces;

public interface IWorkspaceApiKeyService
{
    Task<string> CreateApiKeyAsync(Guid workspaceUserId, string? name, CancellationToken cancellationToken);
    Task<bool> RevokeApiKeyAsync(Guid apiKeyId, CancellationToken cancellationToken);
    Task<List<WorkspaceApiKey>> ListApiKeysAsync(Guid workspaceUserId, CancellationToken cancellationToken);
    Task<string> RegenerateApiKeyAsync(Guid apiKeyId, CancellationToken cancellationToken);
    Task<bool> ValidateApiKeyAsync(string apiKey, CancellationToken cancellationToken);
}