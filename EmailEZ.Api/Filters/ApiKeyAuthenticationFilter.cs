using EmailEZ.Application.Interfaces; // For IApiKeyHasher, IApplicationDbContext, IWorkspaceContext
using Microsoft.EntityFrameworkCore;

namespace EmailEZ.Api.Filters;

public class ApiKeyAuthenticationFilter : IEndpointFilter
{
    private const string ApiKeyHeaderName = "X-API-KEY"; // Your API Key header name

    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        var httpContext = context.HttpContext;

        // 1. Get the API Key from the request header
        if (!httpContext.Request.Headers.TryGetValue(ApiKeyHeaderName, out var extractedApiKey))
        {
            // If the header is missing, the request is unauthorized for this filter.
            // This filter assumes the endpoint REQUIRES an API key.
            return Results.Unauthorized();
        }

        // Get services from the request scope
        using var scope = httpContext.RequestServices.CreateScope();
        var apiKeyHasher = scope.ServiceProvider.GetRequiredService<IApiKeyHasher>();
        var dbContext = scope.ServiceProvider.GetRequiredService<IApplicationDbContext>();
        var workspaceContext = scope.ServiceProvider.GetRequiredService<IWorkspaceContext>();

        // 2. Query the database for potential workspaces and perform in-memory verification.
        var workspacesFromDb = await dbContext.Workspaces
                                          .Where(t => t.IsActive /* && !t.IsDeleted */) // Add your IsDeleted check if applicable
                                          .Select(t => new // Project to an anonymous type for efficiency
                                          {
                                              t.Id,
                                              t.Name,
                                              t.Domain,
                                              t.ApiKeyHash,
                                              t.ApiKeyLastUsedAt
                                          })
                                          .ToListAsync();

        // 3. Iterate through the in-memory list to find a matching workspace.
        var matchingWorkspaceData = workspacesFromDb.FirstOrDefault(t =>
            !string.IsNullOrEmpty(t.ApiKeyHash) && apiKeyHasher.VerifyApiKey(extractedApiKey.ToString(), t.ApiKeyHash)
        );

        if (matchingWorkspaceData == null)
        {
            // API Key is invalid or no matching workspace found
            return Results.Unauthorized();
        }

        // 4. Populate IWorkspaceContext if a workspace is found
        workspaceContext.SetWorkspace(matchingWorkspaceData.Id, matchingWorkspaceData.Domain);

        // Optional: Update ApiKeyLastUsedAt timestamp for the workspace.
        var fullWorkspaceEntity = await dbContext.Workspaces.FindAsync(matchingWorkspaceData.Id);
        if (fullWorkspaceEntity != null)
        {
            fullWorkspaceEntity.ApiKeyLastUsedAt = DateTimeOffset.UtcNow;
            await dbContext.SaveChangesAsync();
        }

        // 5. If authentication is successful, proceed to the next filter or the endpoint handler
        return await next.Invoke(context);
    }
    
}