using MediatR;
using System;

namespace EmailEZ.Application.Features.Workspaces.Commands.DeleteWorkspace;

public record DeleteWorkspaceCommand(Guid Id) : IRequest<DeleteWorkspaceResponse>;

public record DeleteWorkspaceResponse(bool Success, string? Message = default); // Simple response for delete