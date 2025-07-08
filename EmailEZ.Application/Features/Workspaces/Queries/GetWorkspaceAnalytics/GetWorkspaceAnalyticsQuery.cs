using EmailEZ.Application.Interfaces;
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
    public List<EmailVolumePointDto> EmailVolumeOverTime { get; set; } = new();
    public List<RecentPerformanceDto> RecentPerformance { get; set; } = new();
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
public class EmailStatsDto
{
    public int Total { get; set; } = 0;
    public int Sent { get; set; } = 0;
    public int Failed { get; set; } = 0;
    public int Queued { get; set; } = 0;
    public double AvgAttempts { get; set; } = 0;
}

public class EmailVolumePointDto
{
    public DateTime Date { get; set; }
    public int Sent { get; set; }
    public int Failed { get; set; }
}

public class RecentPerformanceDto
{
    public string Label { get; set; } = default!;
    public int Sent { get; set; }
    public int Failed { get; set; }
    public double DeliveryRate { get; set; }
}
