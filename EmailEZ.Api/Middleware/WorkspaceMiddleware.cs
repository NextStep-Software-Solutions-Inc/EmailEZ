using EmailEZ.Application.Interfaces;

namespace EmailEZ.Api.Middleware;

public class WorkspaceMiddleware(RequestDelegate next)
{
    private readonly RequestDelegate _next = next;

    public async Task InvokeAsync(HttpContext context, IWorkspaceContext workspaceContext)
    {
        // Try to get workspaceId from route values
        if (context.Request.RouteValues.TryGetValue("workspaceId", out var workspaceIdObj) &&
            Guid.TryParse(workspaceIdObj?.ToString(), out var workspaceId))
        {
            workspaceContext.SetWorkspace(workspaceId);
        }
        await _next(context);
    }
}
