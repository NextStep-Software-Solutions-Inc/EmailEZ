using EmailEZ.Application.Interfaces;
using EmailEZ.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace EmailEZ.Infrastructure.Persistence.Repositories;

public class EmailStatistics(IApplicationDbContext context) : IEmailStatistics
{

    private readonly IApplicationDbContext _context = context;

    public async Task<int> GetTotalEmailCountAsync(Guid tenantId, CancellationToken cancellationToken)
    {
        return await _context.Emails
            .Where(e => e.TenantId == tenantId && !e.IsDeleted)
            .CountAsync(cancellationToken);
    }
    public async Task<int> GetSentEmailCountAsync(Guid tenantId, CancellationToken cancellationToken)
    {
        var email =  await _context.Emails.IgnoreQueryFilters()
            .Where(e => e.TenantId == tenantId && e.Status == EmailStatus.Sent && !e.IsDeleted)
            .CountAsync(cancellationToken);
        return email;
    }
    //public async Task<int> GetReceivedEmailCountAsync(Guid tenantId, CancellationToken cancellationToken)
    //{
    //    return await _context.Emails
    //        .Where(e => e.TenantId == tenantId && e.Status == EmailStatus.Received)
    //        .CountAsync(cancellationToken);
    //}
    //public async Task<int> GetDraftedEmailCountAsync(Guid tenantId, CancellationToken cancellationToken)
    //{
    //    return await _context.Emails
    //        .Where(e => e.TenantId == tenantId && e.Status == EmailStatus.Drafted)
    //        .CountAsync(cancellationToken);
    //}
    //public async Task<int> GetScheduledEmailCountAsync(Guid tenantId, CancellationToken cancellationToken)
    //{
    //    return await _context.Emails
    //        .Where(e => e.TenantId == tenantId && e.Status == EmailStatus.Scheduled)
    //        .CountAsync(cancellationToken);
    //}
    public async Task<int> GetFailedEmailCountAsync(Guid tenantId, CancellationToken cancellationToken)
    {
        return await _context.Emails
            .Where(e => e.TenantId == tenantId && e.Status == EmailStatus.Failed && !e.IsDeleted)
            .CountAsync(cancellationToken);
    }
    //public async Task<int> GetOpenedEmailCountAsync(Guid tenantId, CancellationToken cancellationToken)
    //{
    //    return await _context.Emails
    //        .Where(e => e.TenantId == tenantId && e.Status == EmailStatus.Opened)
    //        .CountAsync(cancellationToken);
    //}
    //public async Task<int> GetClickedEmailCountAsync(Guid tenantId, CancellationToken cancellationToken)
    //{
    //    return await _context.Emails
    //        .Where(e => e.TenantId == tenantId && e.Status == EmailStatus.Clicked)
    //        .CountAsync(cancellationToken);
    //}
    //public async Task<int> GetUnsubscribedEmailCountAsync(Guid tenantId, CancellationToken cancellationToken)
    //{
    //    return await _context.Emails
    //        .Where(e => e.TenantId == tenantId && e.Status == EmailStatus.Unsubscribed)
    //        .CountAsync(cancellationToken);
    //}
    public async Task<int> GetBouncedEmailCountAsync(Guid tenantId, CancellationToken cancellationToken)
    {
        return await _context.Emails
            .Where(e => e.TenantId == tenantId && e.Status == EmailStatus.Bounced && !e.IsDeleted)
            .CountAsync(cancellationToken);
    }
    //public async Task<int> GetMarkedAsSpamEmailCountAsync(Guid tenantId, CancellationToken cancellationToken)
    //{
    //    return await _context.Emails
    //        .Where(e => e.TenantId == tenantId && e.Status == EmailStatus.MarkedAsSpam)
    //        .CountAsync(cancellationToken);
    //}
    //public async Task<int> GetRepliedEmailCountAsync(Guid tenantId, CancellationToken cancellationToken)
    //{
    //    return await _context.Emails
    //        .Where(e => e.TenantId == tenantId && e.Status == EmailStatus.Replied)
    //        .CountAsync(cancellationToken);
    //}
    //public async Task<int> GetForwardedEmailCountAsync(Guid tenantId, CancellationToken cancellationToken)
    //{
    //    return await _context.Emails
    //        .Where(e => e.TenantId == tenantId && e.Status == EmailStatus.Forwarded)
    //        .CountAsync(cancellationToken);
    //} public async Task<int> GetArchivedEmailCountAsync(Guid tenantId, CancellationToken cancellationToken)
    //{
    //    return await _context.Emails
    //        .Where(e => e.TenantId == tenantId && e.Status == EmailStatus.Archived)
    //        .CountAsync(cancellationToken);
    //}
    //public async Task<int> GetDeletedEmailCountAsync(Guid tenantId, CancellationToken cancellationToken)
    //{
    //    return await _context.Emails
    //        .Where(e => e.TenantId == tenantId && e.Status == EmailStatus.Deleted)
    //        .CountAsync(cancellationToken);
    //}
    //public async Task<int> GetPendingEmailCountAsync(Guid tenantId, CancellationToken cancellationToken)
    //{
    //    return await _context.Emails
    //        .Where(e => e.TenantId == tenantId && e.Status == EmailStatus.Pending)
    //        .CountAsync(cancellationToken);
    //}
}
