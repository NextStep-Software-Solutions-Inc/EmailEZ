using MediatR;

namespace EmailEZ.Application.Features.WorkspaceApiKeys.Commands;
public record RegenerateWorkspaceApiKeyCommand(Guid WorkspaceId, string UserId, Guid ApiKeyId) : IRequest<RegenerateWorkspaceApiKeyResponse>;

public record RegenerateWorkspaceApiKeyResponse(
    Guid ApiKeyId,
    string NewPlainKey,
    bool Success,
    string? Message = null
);