using MediatR; // For IRequest<TResponse>
using System;
using System.Collections.Generic;

namespace EmailEZ.Application.Features.Tenants.Commands.CreateTenant;

/// <summary>
/// Represents a command to create a new Tenant.
/// </summary>
public class CreateTenantCommand : IRequest<CreateTenantResponse>
{
    public string Name { get; set; } = string.Empty;
    public string Domain { get; set; } = string.Empty;
    public string SmtpHost { get; set; } = string.Empty;
    public int SmtpPort { get; set; }
    public string SmtpUsername { get; set; } = string.Empty;
    public string SmtpPassword { get; set; } = string.Empty; // Plaintext, will be encrypted by handler
    public bool SmtpEnableSsl { get; set; } = true; // Default to true
}

/// <summary>
/// Represents the response returned after creating a Tenant.
/// </summary>
public class CreateTenantResponse
{
    public Guid TenantId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Domain { get; set; } = string.Empty;
    public string ApiKey { get; set; } = string.Empty; // The newly generated plaintext API key
    public bool IsSuccess { get; set; } = true;
    public string? Message { get; set; }
}