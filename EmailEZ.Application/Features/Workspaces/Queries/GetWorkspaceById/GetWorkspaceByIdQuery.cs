using MediatR;
using System;

namespace EmailEZ.Application.Features.Workspaces.Queries.GetWorkspaceById;

public record GetWorkspaceByIdQuery(Guid Id) : IRequest<GetWorkspaceByIdResponse>;

public record GetWorkspaceByIdResponse(
    Guid WorkspaceId,
    string Name,
    string Domain,
    bool IsActive,
    DateTimeOffset CreatedAtUtc
);