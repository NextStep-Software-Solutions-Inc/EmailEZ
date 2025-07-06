using MediatR;
using System.Collections.Generic; // For List<T>

namespace EmailEZ.Application.Features.Workspaces.Queries.GetAllWorkspaces;

// This query doesn't need any parameters, so it's an empty record.
public record GetAllWorkspacesQuery() : IRequest<List<GetAllWorkspacesResponse>>;

public record GetAllWorkspacesResponse(
    Guid WorkspaceId,
    string Name,
    string Domain,
    bool IsActive,
    DateTimeOffset CreatedAtUtc
);