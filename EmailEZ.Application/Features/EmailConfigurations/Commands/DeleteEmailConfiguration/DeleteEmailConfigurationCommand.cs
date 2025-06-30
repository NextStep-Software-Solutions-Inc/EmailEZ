using MediatR;

namespace EmailEZ.Application.Features.EmailConfigurations.Commands.DeleteEmailConfiguration;

public record DeleteEmailConfigurationCommand(Guid TenantId, Guid Id) : IRequest<DeleteEmailConfigurationResponse>;

public record DeleteEmailConfigurationResponse(bool Success, string? Message = default);