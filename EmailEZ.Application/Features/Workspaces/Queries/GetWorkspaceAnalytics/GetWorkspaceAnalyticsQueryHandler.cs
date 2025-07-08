using EmailEZ.Application.Interfaces;
using EmailEZ.Domain.Enums;
using MediatR;

namespace EmailEZ.Application.Features.Workspaces.Queries.GetWorkspaceAnalytics;

public class GetWorkspaceAnalyticsQueryHandler : IRequestHandler<GetWorkspaceAnalyticsQuery, GetWorkspaceAnalyticsResponse>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetWorkspaceAnalyticsQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<GetWorkspaceAnalyticsResponse> Handle(GetWorkspaceAnalyticsQuery request, CancellationToken cancellationToken)
    {
        var daysBack = request.DaysBack ?? 30;
        var periodStart = DateTimeOffset.UtcNow.AddDays(-daysBack);
        var periodEnd = DateTimeOffset.UtcNow;

        // Get workspace details (must be awaited first)
        var workspace = await _unitOfWork.Workspaces.FirstOrDefaultAsync(w => w.Id == request.WorkspaceId, cancellationToken)
            ?? throw new InvalidOperationException($"Workspace with ID {request.WorkspaceId} not found.");


        var emailStats = await _unitOfWork.Emails.GetWorkspaceEmailStatsAsync(request.WorkspaceId, periodStart, cancellationToken) ?? 
            throw new InvalidOperationException($"No email statistics found for workspace ID {request.WorkspaceId} in the specified period.");

        var emailVolumeOverTime = await _unitOfWork.Emails.GetEmailVolumeOverTimeAsync(
            request.WorkspaceId,
            periodStart,
            periodEnd,
            cancellationToken
        );
        var recentPerformance = await _unitOfWork.Emails.GetRecentPerformanceAsync(request.WorkspaceId, cancellationToken);

        var statusBreakdown = await _unitOfWork.Emails.GroupByAsync(
            e => e.Status,
            g => new { Status = g.Key.ToString(), Count = g.Count() },
            e => e.WorkspaceId == request.WorkspaceId && e.QueuedAt >= periodStart,
            cancellationToken);

        var userCounts = await _unitOfWork.WorkspaceUsers.GroupByAsync(
            wu => wu.Role,
            g => new { Role = g.Key, Count = g.Count() },
            wu => wu.WorkspaceId == request.WorkspaceId,
            cancellationToken);

        var userCountDict = userCounts.ToDictionary(uc => uc.Role, uc => uc.Count);

        var emailMetrics = new EmailMetrics
        {
            TotalEmails = emailStats.Total,
            SentEmails = emailStats.Sent,
            FailedEmails = emailStats.Failed,
            QueuedEmails = emailStats.Queued,
            SuccessRate = emailStats.Total > 0 ? Math.Round((decimal)emailStats.Sent / emailStats.Total * 100, 2) : 0,
            AverageAttempts = Math.Round((double)emailStats.AvgAttempts, 2),
            StatusBreakdown = statusBreakdown.ToDictionary(sb => sb.Status, sb => sb.Count)
        };

        var userMetrics = new UserMetrics
        {
            TotalUsers = userCountDict.Values.Sum(),
            Owners = userCountDict.GetValueOrDefault(WorkspaceUserRole.Owner, 0),
            Admins = userCountDict.GetValueOrDefault(WorkspaceUserRole.Admin, 0),
            Members = userCountDict.GetValueOrDefault(WorkspaceUserRole.Member, 0)
        };

        return new GetWorkspaceAnalyticsResponse
        {
            WorkspaceId = request.WorkspaceId,
            WorkspaceName = workspace.Name,
            EmailMetrics = emailMetrics,
            UserMetrics = userMetrics,
            AnalysisPeriodStart = periodStart,
            AnalysisPeriodEnd = periodEnd,
            EmailVolumeOverTime = emailVolumeOverTime,
            RecentPerformance = recentPerformance
        };
    }
}