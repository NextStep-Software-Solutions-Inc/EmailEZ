using MediatR;
using EmailEZ.Domain.Entities;

namespace EmailEZ.Application.Features.WorkspaceApiKeys.Queries;

public record GetWorkspaceApiKeysQuery(Guid WorkspaceId, string UserId) : IRequest<List<GetWorkspaceApiKeyResponse>>;

public record GetWorkspaceApiKeyResponse
{
    public Guid Id { get; set; }
    public Guid WorkspaceUserId { get; set; } // Foreign key to WorkspaceUser
    public DateTimeOffset? LastUsedAt { get; set; }
    public bool IsActive { get; set; } = true;
    public string? Name { get; set; }
}