using EmailEZ.Application.Interfaces;
using EmailEZ.Domain.Entities;
using EmailEZ.Domain.Enums;
using System.Linq.Expressions;

namespace EmailEZ.Application.Services;

/// <summary>
/// Advanced service demonstrating complex repository operations.
/// </summary>
public class AdvancedDataService
{
    private readonly IUnitOfWork _unitOfWork;

    public AdvancedDataService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    #region Batch Operations Examples

    /// <summary>
    /// Batch updates email statuses for a workspace.
    /// </summary>
    /// <param name="workspaceId">The workspace ID.</param>
    /// <param name="fromStatus">Current status to update from.</param>
    /// <param name="toStatus">New status to set.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Number of emails updated.</returns>
    public async Task<int> BatchUpdateEmailStatusAsync(
        Guid workspaceId, 
        EmailStatus fromStatus, 
        EmailStatus toStatus,
        CancellationToken cancellationToken = default)
    {
        await _unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            var updateCount = await _unitOfWork.Emails.BatchUpdateAsync(
                e => e.WorkspaceId == workspaceId && e.Status == fromStatus,
                e => new Email 
                { 
                    Status = toStatus,
                    UpdatedAt = DateTimeOffset.UtcNow
                },
                cancellationToken);

            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await _unitOfWork.CommitTransactionAsync(cancellationToken);

            return updateCount;
        }
        catch
        {
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }

    /// <summary>
    /// Batch soft deletes old emails in a workspace.
    /// </summary>
    /// <param name="workspaceId">The workspace ID.</param>
    /// <param name="olderThan">Delete emails older than this date.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Number of emails deleted.</returns>
    public async Task<int> ArchiveOldEmailsAsync(
        Guid workspaceId, 
        DateTimeOffset olderThan,
        CancellationToken cancellationToken = default)
    {
        await _unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            var deleteCount = await _unitOfWork.Emails.BatchSoftDeleteAsync(
                e => e.WorkspaceId == workspaceId && e.QueuedAt < olderThan,
                cancellationToken);

            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await _unitOfWork.CommitTransactionAsync(cancellationToken);

            return deleteCount;
        }
        catch
        {
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }

    /// <summary>
    /// Performs upsert operation for multiple workspace users.
    /// </summary>
    /// <param name="workspaceUsers">The workspace users to upsert.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Summary of insert/update operations.</returns>
    public async Task<(int Inserted, int Updated)> UpsertWorkspaceUsersAsync(
        IEnumerable<WorkspaceUser> workspaceUsers,
        CancellationToken cancellationToken = default)
    {
        await _unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            int inserted = 0, updated = 0;

            foreach (var user in workspaceUsers)
            {
                var (_, isInserted) = await _unitOfWork.WorkspaceUsers.UpsertAsync(
                    user,
                    wu => new { wu.WorkspaceId, wu.UserId },
                    cancellationToken);

                if (isInserted) inserted++;
                else updated++;
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await _unitOfWork.CommitTransactionAsync(cancellationToken);

            return (inserted, updated);
        }
        catch
        {
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }

    #endregion

    #region Complex Query Examples

    /// <summary>
    /// Gets email statistics with complex aggregations.
    /// </summary>
    /// <param name="workspaceId">The workspace ID.</param>
    /// <param name="fromDate">Start date for statistics.</param>
    /// <param name="toDate">End date for statistics.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Email statistics grouped by status and date.</returns>
    public async Task<object> GetAdvancedEmailStatisticsAsync(
        Guid workspaceId,
        DateTimeOffset fromDate,
        DateTimeOffset toDate,
        CancellationToken cancellationToken = default)
    {
        // Get email counts grouped by status and date
        var statusGroups = await _unitOfWork.Emails.GroupByAsync(
            e => new { e.Status, Date = e.QueuedAt.Date },
            g => new 
            { 
                Status = g.Key.Status,
                Date = g.Key.Date,
                Count = g.Count(),
                TotalAttempts = g.Sum(e => e.AttemptCount)
            },
            e => e.WorkspaceId == workspaceId && e.QueuedAt >= fromDate && e.QueuedAt <= toDate,
            cancellationToken);

        // Get success rate
        var totalEmails = await _unitOfWork.Emails.CountAsync(
            e => e.WorkspaceId == workspaceId && e.QueuedAt >= fromDate && e.QueuedAt <= toDate,
            cancellationToken);

        var successfulEmails = await _unitOfWork.Emails.CountAsync(
            e => e.WorkspaceId == workspaceId && 
                 e.Status == EmailStatus.Sent && 
                 e.QueuedAt >= fromDate && e.QueuedAt <= toDate,
            cancellationToken);

        var successRate = totalEmails > 0 ? (decimal)successfulEmails / totalEmails * 100 : 0;

        return new
        {
            StatusGroups = statusGroups,
            TotalEmails = totalEmails,
            SuccessfulEmails = successfulEmails,
            SuccessRate = Math.Round(successRate, 2)
        };
    }

    /// <summary>
    /// Gets workspace users with advanced filtering and projection.
    /// </summary>
    /// <param name="workspaceId">The workspace ID.</param>
    /// <param name="role">Optional role filter.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Projected user information.</returns>
    public async Task<IEnumerable<object>> GetWorkspaceUserSummaryAsync(
        Guid workspaceId,
        WorkspaceUserRole? role = null,
        CancellationToken cancellationToken = default)
    {
        Expression<Func<WorkspaceUser, bool>>? filter = wu => wu.WorkspaceId == workspaceId;
        
        if (role.HasValue)
        {
            filter = wu => wu.WorkspaceId == workspaceId && wu.Role == role.Value;
        }

        return await _unitOfWork.WorkspaceUsers.ProjectToAsync(
            wu => new 
            { 
                wu.UserId,
                Role = wu.Role.ToString(),
                MemberSince = wu.CreatedAt,
                LastActivity = wu.UpdatedAt
            },
            filter,
            cancellationToken);
    }

    /// <summary>
    /// Gets emails with advanced filtering, sorting, and includes.
    /// </summary>
    /// <param name="workspaceId">The workspace ID.</param>
    /// <param name="status">Optional status filter.</param>
    /// <param name="pageNumber">Page number for pagination.</param>
    /// <param name="pageSize">Page size for pagination.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Emails with attachments and events loaded.</returns>
    public async Task<IEnumerable<Email>> GetEmailsWithDetailsAsync(
        Guid workspaceId,
        EmailStatus? status = null,
        int pageNumber = 1,
        int pageSize = 50,
        CancellationToken cancellationToken = default)
    {
        Expression<Func<Email, bool>>? filter = e => e.WorkspaceId == workspaceId;
        
        if (status.HasValue)
        {
            filter = e => e.WorkspaceId == workspaceId && e.Status == status.Value;
        }

        return await _unitOfWork.Emails.GetAdvancedAsync(
            filter: filter,
            orderBy: query => query.OrderByDescending(e => e.QueuedAt),
            includes: new Expression<Func<Email, object>>[]
            {
                e => e.Attachments,
                e => e.Events,
                e => e.EmailConfiguration
            },
            skip: (pageNumber - 1) * pageSize,
            take: pageSize,
            cancellationToken: cancellationToken);
    }

    #endregion

    #region Aggregation Examples

    /// <summary>
    /// Gets workspace performance metrics using aggregations.
    /// </summary>
    /// <param name="workspaceId">The workspace ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Performance metrics.</returns>
    public async Task<object> GetWorkspaceMetricsAsync(
        Guid workspaceId,
        CancellationToken cancellationToken = default)
    {
        var filter = (Expression<Func<Email, bool>>)(e => e.WorkspaceId == workspaceId && e.Status == EmailStatus.Sent);

        var totalSent = await _unitOfWork.Emails.CountAsync(filter, cancellationToken);
        
        var avgAttempts = totalSent > 0 
            ? await _unitOfWork.Emails.AverageAsync(e => e.AttemptCount, filter, cancellationToken)
            : 0;

        var maxAttempts = totalSent > 0 
            ? await _unitOfWork.Emails.MaxAsync(e => e.AttemptCount, filter, cancellationToken)
            : 0;

        var minAttempts = totalSent > 0 
            ? await _unitOfWork.Emails.MinAsync(e => e.AttemptCount, filter, cancellationToken)
            : 0;

        var firstEmailDate = totalSent > 0 
            ? await _unitOfWork.Emails.MinAsync(e => e.QueuedAt, filter, cancellationToken)
            : (DateTimeOffset?)null;

        var lastEmailDate = totalSent > 0 
            ? await _unitOfWork.Emails.MaxAsync(e => e.QueuedAt, filter, cancellationToken)
            : (DateTimeOffset?)null;

        return new
        {
            TotalEmailsSent = totalSent,
            AverageAttempts = Math.Round(avgAttempts, 2),
            MaxAttempts = maxAttempts,
            MinAttempts = minAttempts,
            FirstEmailDate = firstEmailDate,
            LastEmailDate = lastEmailDate,
            ActivePeriod = firstEmailDate.HasValue && lastEmailDate.HasValue 
                ? lastEmailDate.Value - firstEmailDate.Value 
                : TimeSpan.Zero
        };
    }

    #endregion

    #region Utility Examples

    /// <summary>
    /// Performs read-only operations with no tracking for better performance.
    /// </summary>
    /// <param name="workspaceId">The workspace ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Read-only email data.</returns>
    public async Task<IEnumerable<object>> GetEmailSummaryNoTrackingAsync(
        Guid workspaceId,
        CancellationToken cancellationToken = default)
    {
        var emails = await _unitOfWork.Emails.GetAsNoTrackingAsync(
            e => e.WorkspaceId == workspaceId,
            cancellationToken);

        return emails.Select(e => new
        {
            e.Id,
            e.Subject,
            e.Status,
            e.QueuedAt,
            ToCount = e.ToAddresses.Count
        });
    }

    /// <summary>
    /// Demonstrates raw SQL execution for complex queries.
    /// </summary>
    /// <param name="workspaceId">The workspace ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Results from raw SQL query.</returns>
    public async Task<IEnumerable<Email>> GetEmailsFromRawSqlAsync(
        Guid workspaceId,
        CancellationToken cancellationToken = default)
    {
        var sql = @"
            SELECT * FROM ""Emails"" 
            WHERE ""WorkspaceId"" = {0} 
            AND ""Status"" = {1}
            AND ""QueuedAt"" >= {2}
            ORDER BY ""QueuedAt"" DESC
            LIMIT 100";

        return await _unitOfWork.Emails.FromSqlAsync(
            sql,
            new object[] { workspaceId, (int)EmailStatus.Sent, DateTimeOffset.UtcNow.AddDays(-30) },
            cancellationToken);
    }

    #endregion
}