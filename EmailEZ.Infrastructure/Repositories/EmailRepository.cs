using EmailEZ.Application.Features.Workspaces.Queries.GetWorkspaceAnalytics;
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
    public EmailRepository(IApplicationDbContext context) : base(context) { }

    public async Task<IEnumerable<Email>> GetByWorkspaceIdAsync(Guid workspaceId, CancellationToken cancellationToken = default)
    {
        return await _context.Emails
            .Where(e => e.WorkspaceId == workspaceId)
            .OrderByDescending(e => e.QueuedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Email>> GetByStatusAsync(EmailStatus status, CancellationToken cancellationToken = default)
    {
        return await _context.Emails
            .Where(e => e.Status == status)
            .OrderByDescending(e => e.QueuedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Email>> GetByWorkspaceAndStatusAsync(Guid workspaceId, EmailStatus status, CancellationToken cancellationToken = default)
    {
        return await _context.Emails
            .Where(e => e.WorkspaceId == workspaceId && e.Status == status)
            .OrderByDescending(e => e.QueuedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Email>> GetQueuedEmailsAsync(int maxRetries = 3, CancellationToken cancellationToken = default)
    {
        return await _context.Emails
            .Where(e => e.Status == EmailStatus.Queued && e.AttemptCount < maxRetries)
            .OrderBy(e => e.QueuedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Email>> GetWithAttachmentsAsync(Guid workspaceId, CancellationToken cancellationToken = default)
    {
        return await _context.Emails
            .Where(e => e.WorkspaceId == workspaceId)
            .Include(e => e.Attachments)
            .OrderByDescending(e => e.QueuedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Email>> GetWithEventsAsync(Guid workspaceId, CancellationToken cancellationToken = default)
    {
        return await _context.Emails
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
        var query = _context.Emails.Where(e => e.WorkspaceId == workspaceId);

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
        return await _context.Emails
            .Where(e => e.WorkspaceId == workspaceId &&
                       e.QueuedAt >= fromDate &&
                       e.QueuedAt <= toDate)
            .OrderByDescending(e => e.QueuedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<EmailStatsDto> GetWorkspaceEmailStatsAsync(Guid workspaceId, DateTimeOffset periodStart, CancellationToken cancellationToken)
    {
        return await _context.Emails
            .AsNoTracking()
            .Where(e => e.WorkspaceId == workspaceId && e.QueuedAt >= periodStart)
            .GroupBy(e => 1)
            .Select(g => new EmailStatsDto
            {
                Total = g.Count(),
                Sent = g.Count(e => e.Status == EmailStatus.Sent),
                Failed = g.Count(e => e.Status == EmailStatus.Failed),
                Queued = g.Count(e => e.Status == EmailStatus.Queued),
                AvgAttempts = g.Where(e => e.Status == EmailStatus.Sent).Average(e => (double?)e.AttemptCount) ?? 0
            })
            .FirstOrDefaultAsync(cancellationToken)
            ?? new EmailStatsDto();
    }

    public async Task<List<EmailVolumePointDto>> GetEmailVolumeOverTimeAsync(
    Guid workspaceId,
    DateTimeOffset fromDate,
    DateTimeOffset toDate,
    CancellationToken cancellationToken)
    {
        return await _context.Emails
            .AsNoTracking()
            .Where(e => e.WorkspaceId == workspaceId
                        && e.QueuedAt >= fromDate
                        && e.QueuedAt <= toDate)
            .GroupBy(e => e.QueuedAt.Date)
            .OrderBy(g => g.Key)
            .Select(g => new EmailVolumePointDto
            {
                Date = g.Key,
                Sent = g.Count(e => e.Status == EmailStatus.Sent),
                Failed = g.Count(e => e.Status == EmailStatus.Failed)
            })
            .ToListAsync(cancellationToken);


    }
        
    public async Task<List<RecentPerformanceDto>> GetRecentPerformanceAsync(
        Guid workspaceId,
        CancellationToken cancellationToken)
    {
        var today = DateTime.UtcNow.Date;
        var yesterday = today.AddDays(-1);
        var last7Days = today.AddDays(-7);

        var stats = await _dbSet
            .Where(e => e.WorkspaceId == workspaceId && e.QueuedAt.UtcDateTime.Date >= last7Days)
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        int CountSent(IEnumerable<Email> emails) => emails.Count(e => e.Status == EmailStatus.Sent);
        int CountFailed(IEnumerable<Email> emails) => emails.Count(e => e.Status == EmailStatus.Failed);
        double CalcDeliveryRate(int sent, int failed) =>
            (sent + failed) == 0 ? 0 : Math.Round(100.0 * sent / (sent + failed), 1);

        var todayStats = stats.Where(e => e.QueuedAt.UtcDateTime.Date == today);
        var yesterdayStats = stats.Where(e => e.QueuedAt.UtcDateTime.Date == yesterday);
        var last7DaysStats = stats.Where(e => e.QueuedAt.UtcDateTime.Date >= last7Days);

        var todaySent = CountSent(todayStats);
        var todayFailed = CountFailed(todayStats);
        var yesterdaySent = CountSent(yesterdayStats);
        var yesterdayFailed = CountFailed(yesterdayStats);
        var last7DaysSent = CountSent(last7DaysStats);
        var last7DaysFailed = CountFailed(last7DaysStats);

        var result = new List<RecentPerformanceDto>
        {
            new() {
                Label = "Today",
                Sent = todaySent,
                Failed = todayFailed,
                DeliveryRate = CalcDeliveryRate(todaySent, todayFailed)
            },
            new() {
                Label = "Yesterday",
                Sent = yesterdaySent,
                Failed = yesterdayFailed,
                DeliveryRate = CalcDeliveryRate(yesterdaySent, yesterdayFailed)
            },
            new() {
                Label = "Last 7 Days",
                Sent = last7DaysSent,
                Failed = last7DaysFailed,
                DeliveryRate = CalcDeliveryRate(last7DaysSent, last7DaysFailed)
            }
        };

        return result;
    }
}