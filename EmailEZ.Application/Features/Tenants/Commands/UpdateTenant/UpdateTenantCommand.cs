using MediatR;
using System;

namespace EmailEZ.Application.Features.Tenants.Commands.UpdateTenant;

public record UpdateTenantCommand(
    Guid Id, // The ID of the tenant to update
    string Name,
    string Domain,
    bool IsActive
) : IRequest<UpdateTenantResponse>; // Response can be a simple success/failure or the updated entity

public record UpdateTenantResponse(bool Success, string? Message = default); // Allow Message to be nullable  
