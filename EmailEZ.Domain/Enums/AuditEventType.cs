namespace EmailEZ.Domain.Enums;

/// <summary>
/// Represents different types of audit events in the system.
/// </summary>
public enum AuditEventType
{
    /// <summary>
    /// A new workspace record was created.
    /// </summary>
    WorkspaceCreated = 1,

    /// <summary>
    /// An existing workspace's details were updated.
    /// </summary>
    WorkspaceUpdated = 2,

    /// <summary>
    /// A workspace's API key was regenerated.
    /// </summary>
    ApiKeyRegenerated = 3,

    /// <summary>
    /// A workspace was activated or deactivated.
    /// </summary>
    WorkspaceStatusChanged = 4,

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
