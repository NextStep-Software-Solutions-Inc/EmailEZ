using MediatR;

namespace EmailEZ.Application.Features.WorkspaceApiKeys.Commands;
public record CreateWorkspaceApiKeyCommand(Guid WorkspaceId, string UserId, string? Name) : IRequest<CreateWorkspaceApiKeyResponse>{}

public record CreateWorkspaceApiKeyResponse(
    Guid ApiKeyId,
    string PlainKey,
    bool Success,
    string? Message = default
);