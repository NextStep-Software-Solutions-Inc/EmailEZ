using MediatR;
using System;

namespace EmailEZ.Application.Features.Tenants.Queries.GetTenantById;

public record GetTenantByIdQuery(Guid Id) : IRequest<GetTenantByIdResponse>;

public record GetTenantByIdResponse(
    Guid TenantId,
    string Name,
    string Domain,
    bool IsActive,
    DateTimeOffset CreatedAtUtc
);