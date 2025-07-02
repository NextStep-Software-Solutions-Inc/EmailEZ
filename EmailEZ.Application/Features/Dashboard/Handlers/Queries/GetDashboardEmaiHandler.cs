using EmailEZ.Application.Features.Dashboard.Dto;
using EmailEZ.Application.Features.Dashboard.Requests.Queries;
using EmailEZ.Application.Interfaces;
using Hangfire.Dashboard;
using MediatR;

namespace EmailEZ.Application.Features.Dashboard.Handlers.Queries;

public class GetDashboardEmaiHandler : IRequestHandler<GetDashboardEmailRequest, DashboardDto>
{
    private readonly IEmailStatistics _emailStatistics;
    public GetDashboardEmaiHandler(IEmailStatistics emailStatistics)
    {
        _emailStatistics = emailStatistics;
    }
    public async Task<DashboardDto> Handle(GetDashboardEmailRequest request, CancellationToken cancellationToken)
    
    {
        var totalEmail = await _emailStatistics.GetTotalEmailCountAsync(request.TenantId, cancellationToken);
        var sentEmailCount = await _emailStatistics.GetSentEmailCountAsync(request.TenantId, cancellationToken);
        var failedEmailCount = await _emailStatistics.GetFailedEmailCountAsync(request.TenantId, cancellationToken);
        var bouncedEmailCount = await _emailStatistics.GetBouncedEmailCountAsync(request.TenantId, cancellationToken);
        // return Task.FromResult(response);
        var response = new DashboardDto
        {
            EmailsSent = sentEmailCount, // Example data, replace with actual logic
            EmailsReceived = 50,
            EmailsDrafted = 20,
            EmailsScheduled = 10,
            EmailsFailed = failedEmailCount,
            EmailsOpened = 30,
            EmailsClicked = 15,
            EmailsUnsubscribed = 2,
            EmailsBounced = bouncedEmailCount,
            EmailsMarkedAsSpam = 3,
            EmailsReplied = 8,
            EmailsForwarded = 4,
            EmailsArchived = 6,
            EmailsDeleted = 12,
            EmailsPending = 7,
            EmailsInProgress = 9,
            EmailsTotal = totalEmail // This could be a sum of all emails
       };
        
        return response;
    }
}
