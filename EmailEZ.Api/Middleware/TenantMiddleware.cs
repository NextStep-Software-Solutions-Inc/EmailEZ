using EmailEZ.Application.Interfaces;

namespace EmailEZ.Api.Middleware;

public class TenantMiddleware(RequestDelegate next)
{
    private readonly RequestDelegate _next = next;

    public async Task InvokeAsync(HttpContext context, ITenantContext tenantContext)
    {
        // Try to get tenantId from route values
        if (context.Request.RouteValues.TryGetValue("tenantId", out var tenantIdObj) &&
            Guid.TryParse(tenantIdObj?.ToString(), out var tenantId))
        {
            tenantContext.SetTenant(tenantId);
        }
        await _next(context);
    }
}
