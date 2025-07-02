using EmailEZ.Application.Features.Dashboard.Dto;
using Hangfire.Dashboard;
using MediatR;

namespace EmailEZ.Application.Features.Dashboard.Requests.Queries
{
    public class GetDashboardEmailRequest : IRequest<DashboardDto>
    {
        public Guid TenantId { get; set; }
        
        public GetDashboardEmailRequest(Guid tenantId)
        {
            TenantId = tenantId;
        }
    }
}
