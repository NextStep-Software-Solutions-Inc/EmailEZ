using EmailEZ.Application.Interfaces;
using EmailEZ.Domain.Enums;
using MediatR;

namespace EmailEZ.Application.Features.Workspaces.Queries.GetWorkspaceAnalytics;

public record GetWorkspaceAnalyticsQuery(Guid WorkspaceId, int? DaysBack = 30) : IRequest<GetWorkspaceAnalyticsResponse>;

public record GetWorkspaceAnalyticsResponse
{
    public Guid WorkspaceId { get; init; }
    public string WorkspaceName { get; init; } = string.Empty;
    public EmailMetrics EmailMetrics { get; init; } = new();
    public UserMetrics UserMetrics { get; init; } = new();
    public DateTimeOffset AnalysisPeriodStart { get; init; }
    public DateTimeOffset AnalysisPeriodEnd { get; init; }
}

public record EmailMetrics
{
    public int TotalEmails { get; init; }
    public int SentEmails { get; init; }
    public int FailedEmails { get; init; }
    public int QueuedEmails { get; init; }
    public decimal SuccessRate { get; init; }
    public double AverageAttempts { get; init; }
    public Dictionary<string, int> StatusBreakdown { get; init; } = new();
}

public record UserMetrics
{
    public int TotalUsers { get; init; }
    public int Owners { get; init; }
    public int Admins { get; init; }
    public int Members { get; init; }
}

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

        // Get workspace details
        var workspace = await _unitOfWork.Workspaces
            .Query()
            .Where(w => w.Id == request.WorkspaceId)
            .FirstOrDefaultAsync(cancellationToken);

        if (workspace == null)
        {
            throw new InvalidOperationException($"Workspace with ID {request.WorkspaceId} not found.");
        }

        // Get email metrics using repository aggregation methods
        var totalEmails = await _unitOfWork.Emails.CountAsync(
            e => e.WorkspaceId == request.WorkspaceId && e.QueuedAt >= periodStart,
            cancellationToken);

        var sentEmails = await _unitOfWork.Emails.CountAsync(
            e => e.WorkspaceId == request.WorkspaceId && 
                 e.Status == EmailStatus.Sent && 
                 e.QueuedAt >= periodStart,
            cancellationToken);

        var failedEmails = await _unitOfWork.Emails.CountAsync(
            e => e.WorkspaceId == request.WorkspaceId && 
                 e.Status == EmailStatus.Failed && 
                 e.QueuedAt >= periodStart,
            cancellationToken);

        var queuedEmails = await _unitOfWork.Emails.CountAsync(
            e => e.WorkspaceId == request.WorkspaceId && 
                 e.Status == EmailStatus.Queued && 
                 e.QueuedAt >= periodStart,
            cancellationToken);

        // Calculate average attempts for sent emails
        var avgAttempts = sentEmails > 0 
            ? await _unitOfWork.Emails.AverageAsync(
                e => e.AttemptCount,
                e => e.WorkspaceId == request.WorkspaceId && 
                     e.Status == EmailStatus.Sent && 
                     e.QueuedAt >= periodStart,
                cancellationToken)
            : 0;

        // Get status breakdown using GroupBy
        var statusBreakdown = await _unitOfWork.Emails.GroupByAsync(
            e => e.Status,
            g => new { Status = g.Key.ToString(), Count = g.Count() },
            e => e.WorkspaceId == request.WorkspaceId && e.QueuedAt >= periodStart,
            cancellationToken);

        // Get user metrics
        var userCounts = await _unitOfWork.WorkspaceUsers.GroupByAsync(
            wu => wu.Role,
            g => new { Role = g.Key, Count = g.Count() },
            wu => wu.WorkspaceId == request.WorkspaceId,
            cancellationToken);

        var userCountDict = userCounts.ToDictionary(uc => uc.Role, uc => uc.Count);

        var emailMetrics = new EmailMetrics
        {
            TotalEmails = totalEmails,
            SentEmails = sentEmails,
            FailedEmails = failedEmails,
            QueuedEmails = queuedEmails,
            SuccessRate = totalEmails > 0 ? Math.Round((decimal)sentEmails / totalEmails * 100, 2) : 0,
            AverageAttempts = Math.Round((double)avgAttempts, 2),
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
            AnalysisPeriodEnd = periodEnd
        };
    }
}