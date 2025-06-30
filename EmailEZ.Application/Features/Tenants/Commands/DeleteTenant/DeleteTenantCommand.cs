using MediatR;
using System;

namespace EmailEZ.Application.Features.Tenants.Commands.DeleteTenant;

public record DeleteTenantCommand(Guid Id) : IRequest<DeleteTenantResponse>;

public record DeleteTenantResponse(bool Success, string? Message = default); // Simple response for delete