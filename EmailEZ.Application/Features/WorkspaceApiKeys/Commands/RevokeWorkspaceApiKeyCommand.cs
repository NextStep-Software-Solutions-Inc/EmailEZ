using MediatR;

namespace EmailEZ.Application.Features.WorkspaceApiKeys.Commands;
public record RevokeWorkspaceApiKeyCommand(Guid WorkspaceId, string UserId, Guid ApiKeyId) : IRequest<RevokeWorkspaceApiKeyResponse>;

public record RevokeWorkspaceApiKeyResponse(bool Success, string? Message = default);