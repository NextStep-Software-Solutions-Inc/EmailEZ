using EmailEZ.Application.Interfaces; // For IApiKeyHasher, IApplicationDbContext, ITenantContext
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
        var tenantContext = scope.ServiceProvider.GetRequiredService<ITenantContext>();

        // 2. Query the database for potential tenants and perform in-memory verification.
        var tenantsFromDb = await dbContext.Tenants
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

        // 3. Iterate through the in-memory list to find a matching tenant.
        var matchingTenantData = tenantsFromDb.FirstOrDefault(t =>
            !string.IsNullOrEmpty(t.ApiKeyHash) && apiKeyHasher.VerifyApiKey(extractedApiKey.ToString(), t.ApiKeyHash)
        );

        if (matchingTenantData == null)
        {
            // API Key is invalid or no matching tenant found
            return Results.Unauthorized();
        }

        // 4. Populate ITenantContext if a tenant is found
        tenantContext.SetTenant(matchingTenantData.Id, matchingTenantData.Domain);

        // Optional: Update ApiKeyLastUsedAt timestamp for the tenant.
        var fullTenantEntity = await dbContext.Tenants.FindAsync(matchingTenantData.Id);
        if (fullTenantEntity != null)
        {
            fullTenantEntity.ApiKeyLastUsedAt = DateTimeOffset.UtcNow;
            await dbContext.SaveChangesAsync();
        }

        // 5. If authentication is successful, proceed to the next filter or the endpoint handler
        return await next.Invoke(context);
    }
    
}