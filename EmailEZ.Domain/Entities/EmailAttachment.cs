using EmailEZ.Domain.Common;

namespace EmailEZ.Domain.Entities;

public class EmailAttachment : BaseEntity
{
    public Guid EmailId { get; set; }
    public Guid WorkspaceId { get; set; } // Explicitly defined WorkspaceId

    public string FileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public string? FileContentBase64 { get; set; }
    public int FileSizeKB { get; set; }

    // Navigation property
    public Email Email { get; set; } = null!;
}