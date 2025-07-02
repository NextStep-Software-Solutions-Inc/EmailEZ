using Carter;
using EmailEZ.Application.Features.Dashboard.Dto;
using EmailEZ.Application.Features.Dashboard.Requests.Queries;
using MediatR;

namespace EmailEZ.Api.Endpoints;

public class DashboardEndpoints : CarterModule
{
    private const string DashboardRoute = "api/v1/tenants/{tenantId:guid}/dashboard";
    
    public override void AddRoutes(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup(DashboardRoute)
            .WithTags("Dashboard")
            .RequireAuthorization();
        app.MapGet(DashboardRoute, async (Guid tenantId, IMediator mediator) =>
        {
            var request = new GetDashboardEmailRequest(tenantId);
            var response = await mediator.Send(request);
            return Results.Ok(response);
        })
        .WithName("GetDashboardEmail")
        .Produces<DashboardDto>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound)
        .WithTags("Dashboard");
    }
}
