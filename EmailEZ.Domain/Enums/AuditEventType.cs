namespace EmailEZ.Domain.Enums;

/// <summary>
/// Represents different types of audit events in the system.
/// </summary>
public enum AuditEventType
{
    /// <summary>
    /// A new tenant record was created.
    /// </summary>
    TenantCreated = 1,

    /// <summary>
    /// An existing tenant's details were updated.
    /// </summary>
    TenantUpdated = 2,

    /// <summary>
    /// A tenant's API key was regenerated.
    /// </summary>
    ApiKeyRegenerated = 3,

    /// <summary>
    /// A tenant was activated or deactivated.
    /// </summary>
    TenantStatusChanged = 4,

    /// <summary>
    /// An email send attempt event (could be logged by the system).
    /// </summary>
    EmailSendAttempt = 5,

    /// <summary>
    /// System configuration was updated.
    /// </summary>
    SystemConfigUpdated = 6,

    /// <summary>
    /// An administrative user logged in.
    /// </summary>
    AdminLogin = 7,

    /// <summary>
    /// A system-level error occurred.
    /// </summary>
    SystemError = 8
}
