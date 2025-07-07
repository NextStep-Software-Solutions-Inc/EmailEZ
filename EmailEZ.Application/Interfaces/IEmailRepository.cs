using EmailEZ.Domain.Entities;
using EmailEZ.Domain.Enums;

namespace EmailEZ.Application.Interfaces;

/// <summary>
/// Repository interface for Email entities with custom operations.
/// </summary>
public interface IEmailRepository : IGenericRepository<Email>
{
    /// <summary>
    /// Gets all emails for a specific workspace.
    /// </summary>
    /// <param name="workspaceId">The workspace identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A collection of emails.</returns>
    Task<IEnumerable<Email>> GetByWorkspaceIdAsync(Guid workspaceId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets emails by status.
    /// </summary>
    /// <param name="status">The email status.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A collection of emails with the specified status.</returns>
    Task<IEnumerable<Email>> GetByStatusAsync(EmailStatus status, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets emails by status for a specific workspace.
    /// </summary>
    /// <param name="workspaceId">The workspace identifier.</param>
    /// <param name="status">The email status.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A collection of emails with the specified status in the workspace.</returns>
    Task<IEnumerable<Email>> GetByWorkspaceAndStatusAsync(Guid workspaceId, EmailStatus status, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets emails that are queued and ready to be sent.
    /// </summary>
    /// <param name="maxRetries">Maximum number of retry attempts.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A collection of emails ready to be sent.</returns>
    Task<IEnumerable<Email>> GetQueuedEmailsAsync(int maxRetries = 3, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets emails with their attachments.
    /// </summary>
    /// <param name="workspaceId">The workspace identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A collection of emails with attachments loaded.</returns>
    Task<IEnumerable<Email>> GetWithAttachmentsAsync(Guid workspaceId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets emails with their events.
    /// </summary>
    /// <param name="workspaceId">The workspace identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A collection of emails with events loaded.</returns>
    Task<IEnumerable<Email>> GetWithEventsAsync(Guid workspaceId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets email statistics for a workspace.
    /// </summary>
    /// <param name="workspaceId">The workspace identifier.</param>
    /// <param name="fromDate">Optional start date for statistics.</param>
    /// <param name="toDate">Optional end date for statistics.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Email statistics grouped by status.</returns>
    Task<Dictionary<EmailStatus, int>> GetEmailStatisticsAsync(
        Guid workspaceId, 
        DateTimeOffset? fromDate = null, 
        DateTimeOffset? toDate = null, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets emails sent within a date range.
    /// </summary>
    /// <param name="workspaceId">The workspace identifier.</param>
    /// <param name="fromDate">Start date.</param>
    /// <param name="toDate">End date.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A collection of emails sent within the date range.</returns>
    Task<IEnumerable<Email>> GetEmailsByDateRangeAsync(
        Guid workspaceId, 
        DateTimeOffset fromDate, 
        DateTimeOffset toDate, 
        CancellationToken cancellationToken = default);
}