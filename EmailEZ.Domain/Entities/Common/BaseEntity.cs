using System.ComponentModel.DataAnnotations;

namespace EmailEZ.Domain.Common;

/// <summary>
/// Base class for all domain entities with common properties like Id and auditing fields.
/// </summary>
public abstract class BaseEntity
{
    /// <summary>
    /// Gets or sets the unique identifier for the entity.
    /// </summary>
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid(); // Auto-generate GUID on creation

    /// <summary>
    /// Gets or sets the date and time when the entity was created (UTC).
    /// </summary>
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    /// <summary>
    /// Gets or sets the identifier of the entity or user who created this record.
    /// Could be a TenantId, AdminUserId, or a system identifier.
    /// </summary>
    public Guid? CreatedBy { get; set; }

    /// <summary>
    /// Gets or sets the date and time when the entity was last updated (UTC).
    /// </summary>
    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;

    /// <summary>
    /// Gets or sets the identifier of the entity or user who last updated this record.
    /// Could be a TenantId, AdminUserId, or a system identifier.
    /// </summary>
    public Guid? UpdatedBy { get; set; }

    /// <summary>
    /// Gets or sets a flag indicating if the entity is soft-deleted.
    /// </summary>
    public bool IsDeleted { get; set; } = false;

    /// <summary>
    /// Gets or sets the date and time when the entity was soft-deleted (UTC). Null if not deleted.
    /// </summary>
    public DateTimeOffset? DeletedAt { get; set; }

    /// <summary>
    /// Gets or sets the identifier of the entity or user who soft-deleted this record.
    /// </summary>
    public Guid? DeletedBy { get; set; }
}