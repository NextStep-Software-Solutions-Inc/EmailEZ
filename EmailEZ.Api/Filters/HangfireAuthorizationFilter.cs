using Hangfire.Dashboard; // For IDashboardAuthorizationFilter

namespace EmailEZ.Api.Filters;

public class HangfireAuthorizationFilter : IDashboardAuthorizationFilter
{
    public bool Authorize(DashboardContext context)
    {
        // Implement your authorization logic here.
        // For development, you might allow all. For production,
        // you'll need proper authentication (e.g., check if user is admin).
        // Example: Only allow if in Development environment or if a specific admin user is logged in.
        // For now, let's allow access in development.
        return true; // For development, allow all. In production, implement real auth.
    }
}