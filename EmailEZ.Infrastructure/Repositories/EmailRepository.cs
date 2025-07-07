using EmailEZ.Application.Interfaces;
using EmailEZ.Domain.Entities;
using EmailEZ.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace EmailEZ.Infrastructure.Repositories;

/// <summary>
/// Repository implementation for Email entities with custom operations.
/// </summary>
public class EmailRepository : GenericRepository<Email>, IEmailRepository
{
    public EmailRepository(IApplicationDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<Email>> GetByWorkspaceIdAsync(Guid workspaceId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(e => e.WorkspaceId == workspaceId)
            .OrderByDescending(e => e.QueuedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Email>> GetByStatusAsync(EmailStatus status, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(e => e.Status == status)
            .OrderByDescending(e => e.QueuedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Email>> GetByWorkspaceAndStatusAsync(Guid workspaceId, EmailStatus status, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(e => e.WorkspaceId == workspaceId && e.Status == status)
            .OrderByDescending(e => e.QueuedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Email>> GetQueuedEmailsAsync(int maxRetries = 3, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(e => e.Status == EmailStatus.Queued && e.AttemptCount < maxRetries)
            .OrderBy(e => e.QueuedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Email>> GetWithAttachmentsAsync(Guid workspaceId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(e => e.WorkspaceId == workspaceId)
            .Include(e => e.Attachments)
            .OrderByDescending(e => e.QueuedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Email>> GetWithEventsAsync(Guid workspaceId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(e => e.WorkspaceId == workspaceId)
            .Include(e => e.Events)
            .OrderByDescending(e => e.QueuedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<Dictionary<EmailStatus, int>> GetEmailStatisticsAsync(
        Guid workspaceId, 
        DateTimeOffset? fromDate = null, 
        DateTimeOffset? toDate = null, 
        CancellationToken cancellationToken = default)
    {
        var query = _dbSet.Where(e => e.WorkspaceId == workspaceId);

        if (fromDate.HasValue)
        {
            query = query.Where(e => e.QueuedAt >= fromDate.Value);
        }

        if (toDate.HasValue)
        {
            query = query.Where(e => e.QueuedAt <= toDate.Value);
        }

        var statistics = await query
            .GroupBy(e => e.Status)
            .Select(g => new { Status = g.Key, Count = g.Count() })
            .ToListAsync(cancellationToken);

        return statistics.ToDictionary(s => s.Status, s => s.Count);
    }

    public async Task<IEnumerable<Email>> GetEmailsByDateRangeAsync(
        Guid workspaceId, 
        DateTimeOffset fromDate, 
        DateTimeOffset toDate, 
        CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(e => e.WorkspaceId == workspaceId && 
                       e.QueuedAt >= fromDate && 
                       e.QueuedAt <= toDate)
            .OrderByDescending(e => e.QueuedAt)
            .ToListAsync(cancellationToken);
    }
}