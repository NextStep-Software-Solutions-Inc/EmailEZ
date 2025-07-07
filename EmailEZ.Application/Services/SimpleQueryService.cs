using EmailEZ.Application.Interfaces;
using EmailEZ.Domain.Entities;
using EmailEZ.Domain.Enums;

namespace EmailEZ.Application.Services;

/// <summary>
/// Simple service demonstrating the easy-to-use query builder.
/// </summary>
public class SimpleQueryService
{
    private readonly IUnitOfWork _unitOfWork;

    public SimpleQueryService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    /// <summary>
    /// Simple example: Get failed emails with details.
    /// </summary>
    public async Task<IEnumerable<Email>> GetFailedEmailsWithDetailsAsync(Guid workspaceId)
    {
        return await _unitOfWork.Emails
            .Query()
            .Where(e => e.WorkspaceId == workspaceId && e.Status == EmailStatus.Failed)
            .Include(e => e.Events)
            .Include(e => e.Attachments)
            .OrderByDescending(e => e.QueuedAt)
            .ToListAsync();
    }

    /// <summary>
    /// Simple example: Get recent emails with pagination.
    /// </summary>
    public async Task<(IEnumerable<Email> Items, int TotalCount)> GetRecentEmailsAsync(
        Guid workspaceId, 
        int pageNumber = 1, 
        int pageSize = 20)
    {
        return await _unitOfWork.Emails
            .Query()
            .Where(e => e.WorkspaceId == workspaceId)
            .Where(e => e.QueuedAt >= DateTimeOffset.UtcNow.AddDays(-7)) // Last 7 days
            .OrderByDescending(e => e.QueuedAt)
            .ToPagedListAsync(pageNumber, pageSize);
    }

    /// <summary>
    /// Simple example: Get workspace users by role.
    /// </summary>
    public async Task<IEnumerable<WorkspaceUser>> GetUsersByRoleAsync(
        Guid workspaceId, 
        WorkspaceUserRole role)
    {
        return await _unitOfWork.WorkspaceUsers
            .Query()
            .Where(wu => wu.WorkspaceId == workspaceId && wu.Role == role)
            .Include(wu => wu.Workspace)
            .OrderBy(wu => wu.CreatedAt)
            .ToListAsync();
    }

    /// <summary>
    /// Simple example: Count emails by status.
    /// </summary>
    public async Task<int> CountEmailsByStatusAsync(Guid workspaceId, EmailStatus status)
    {
        return await _unitOfWork.Emails
            .Query()
            .Where(e => e.WorkspaceId == workspaceId && e.Status == status)
            .CountAsync();
    }

    /// <summary>
    /// Simple example: Get first email matching criteria.
    /// </summary>
    public async Task<Email?> FindEmailBySubjectAsync(Guid workspaceId, string subject)
    {
        return await _unitOfWork.Emails
            .Query()
            .Where(e => e.WorkspaceId == workspaceId)
            .Where(e => e.Subject.Contains(subject))
            .OrderByDescending(e => e.QueuedAt)
            .FirstOrDefaultAsync();
    }

    /// <summary>
    /// Simple example: Get read-only data for reports.
    /// </summary>
    public async Task<IEnumerable<Email>> GetEmailsForReportAsync(Guid workspaceId)
    {
        return await _unitOfWork.Emails
            .Query()
            .Where(e => e.WorkspaceId == workspaceId)
            .Where(e => e.Status == EmailStatus.Sent)
            .AsNoTracking() // No tracking for better performance
            .OrderBy(e => e.QueuedAt)
            .ToListAsync();
    }

    /// <summary>
    /// Complex example: Multiple conditions with chaining.
    /// </summary>
    public async Task<IEnumerable<Email>> GetComplexEmailQueryAsync(
        Guid workspaceId,
        DateTimeOffset? fromDate = null,
        EmailStatus? status = null,
        int? minAttempts = null,
        bool includeDetails = false)
    {
        var query = _unitOfWork.Emails.Query()
            .Where(e => e.WorkspaceId == workspaceId);

        // Conditionally add filters
        if (fromDate.HasValue)
            query = query.Where(e => e.QueuedAt >= fromDate.Value);

        if (status.HasValue)
            query = query.Where(e => e.Status == status.Value);

        if (minAttempts.HasValue)
            query = query.Where(e => e.AttemptCount >= minAttempts.Value);

        // Conditionally add includes
        if (includeDetails)
        {
            query = query
                .Include(e => e.Events)
                .Include(e => e.Attachments);
        }

        return await query
            .OrderByDescending(e => e.QueuedAt)
            .Take(100) // Limit results
            .ToListAsync();
    }
}