namespace EmailEZ.Application.Interfaces;

public interface IEmailStatistics
{
    Task<int> GetSentEmailCountAsync(Guid tenantId, CancellationToken cancellationToken);
    //Task<int> GetReceivedEmailCountAsync(Guid tenantId, CancellationToken cancellationToken);
    //Task<int> GetDraftedEmailCountAsync(Guid tenantId, CancellationToken cancellationToken);
    //Task<int> GetScheduledEmailCountAsync(Guid tenantId, CancellationToken cancellationToken);
    Task<int> GetFailedEmailCountAsync(Guid tenantId, CancellationToken cancellationToken);
    //Task<int> GetOpenedEmailCountAsync(Guid tenantId, CancellationToken cancellationToken);
    //Task<int> GetClickedEmailCountAsync(Guid tenantId, CancellationToken cancellationToken);
    //Task<int> GetUnsubscribedEmailCountAsync(Guid tenantId, CancellationToken cancellationToken);
    Task<int> GetBouncedEmailCountAsync(Guid tenantId, CancellationToken cancellationToken);
    //Task<int> GetMarkedAsSpamEmailCountAsync(Guid tenantId, CancellationToken cancellationToken);
    //Task<int> GetRepliedEmailCountAsync(Guid tenantId, CancellationToken cancellationToken);
    //Task<int> GetForwardedEmailCountAsync(Guid tenantId, CancellationToken cancellationToken);
    //Task<int> GetArchivedEmailCountAsync(Guid tenantId, CancellationToken cancellationToken);
    //Task<int> GetDeletedEmailCountAsync(Guid tenantId, CancellationToken cancellationToken);
    //Task<int> GetPendingEmailCountAsync(Guid tenantId, CancellationToken cancellationToken);
    //Task<int> GetInProgressEmailCountAsync(Guid tenantId, CancellationToken cancellationToken);
    Task<int> GetTotalEmailCountAsync(Guid tenantId, CancellationToken cancellationToken);

}
