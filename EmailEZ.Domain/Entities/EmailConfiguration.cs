// EmailEZ.Domain/Entities/EmailConfiguration.cs

using EmailEZ.Domain.Common; // Assuming BaseAuditableEntity is here
using System;

namespace EmailEZ.Domain.Entities;

public class EmailConfiguration : BaseEntity // Inherit from your base entity class
{
    // Foreign key to link to the Tenant
    public Guid TenantId { get; set; }
    public Tenant Tenant { get; set; } = null!;

    // SMTP Server Details
    public string SmtpHost { get; set; } = string.Empty;
    public int SmtpPort { get; set; }
    public bool UseSsl { get; set; }

    // Authentication Credentials
    public string Username { get; set; } = string.Empty;

    // IMPORTANT: Store passwords encrypted. We'll handle this later in the service layer.
    // For now, it's just a string, but remind yourself about encryption.
    public string Password { get; set; } = string.Empty;

    // Sender Display Name (e.g., "EmailEZ Support")
    public string DisplayName { get; set; } = string.Empty;
    public string FromEmail { get; set; } = string.Empty;
    public bool IsDefault { get; set; }
}