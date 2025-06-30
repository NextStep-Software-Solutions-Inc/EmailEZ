using MediatR;
using System.Collections.Generic; // For List<T>

namespace EmailEZ.Application.Features.Tenants.Queries.GetAllTenants;

// This query doesn't need any parameters, so it's an empty record.
public record GetAllTenantsQuery() : IRequest<List<GetAllTenantsResponse>>;

public record GetAllTenantsResponse(
    Guid TenantId,
    string Name,
    string Domain,
    bool IsActive,
    DateTimeOffset CreatedAtUtc
);