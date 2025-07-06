using MediatR;
using System;

namespace EmailEZ.Application.Features.Workspaces.Commands.UpdateWorkspace;

public record UpdateWorkspaceCommand(
    Guid Id, // The ID of the workspace to update
    string Name,
    string Domain,
    bool IsActive
) : IRequest<UpdateWorkspaceResponse>; // Response can be a simple success/failure or the updated entity

public record UpdateWorkspaceResponse(bool Success, string? Message = default); // Allow Message to be nullable  
