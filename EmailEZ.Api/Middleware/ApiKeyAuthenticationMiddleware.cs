using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;
using EmailEZ.Application.Interfaces; // For IApiKeyHasher, IApplicationDbContext, ITenantContext
using Microsoft.Extensions.DependencyInjection; // To get services from HttpContext.RequestServices
using Microsoft.EntityFrameworkCore; 

namespace EmailEZ.Api.Middleware;

public class ApiKeyAuthenticationMiddleware
{
    private readonly RequestDelegate _next;
    private const string ApiKeyHeaderName = "X-API-KEY";

    public ApiKeyAuthenticationMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // 1. Check if the API Key header is present
        if (!context.Request.Headers.TryGetValue(ApiKeyHeaderName, out var extractedApiKey))
        {
            // If no API Key, allow the request to proceed IF the endpoint doesn't require authentication.
            // For endpoints that *do* require authentication, a separate authorization filter/policy
            // will reject it later. This middleware's job is primarily to *identify* the tenant.
            await _next(context);
            return;
        }

        // Get services from the request scope (services needed per request)
        // We use RequestServices because the middleware itself is a singleton,
        // but services like DbContext and TenantContext are scoped.
        using var scope = context.RequestServices.CreateScope();
        var apiKeyHasher = scope.ServiceProvider.GetRequiredService<IApiKeyHasher>();
        var dbContext = scope.ServiceProvider.GetRequiredService<IApplicationDbContext>();
        var tenantContext = scope.ServiceProvider.GetRequiredService<ITenantContext>();

        // 2. Hash the incoming API Key
        var hashedIncomingApiKey = apiKeyHasher.HashApiKey(extractedApiKey.ToString());

        // 3. Query the database for a matching tenant
        // IMPORTANT: Never query by plaintext API Key. Always hash and compare hashed values.
        var tenant = await dbContext.Tenants.FirstOrDefaultAsync(t => t.ApiKeyHash == hashedIncomingApiKey); 

        if (tenant == null)
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            await context.Response.WriteAsJsonAsync(new { Message = "Invalid API Key or Tenant Not Found." });
            return;
        }

        // 4. Populate ITenantContext if a tenant is found
        tenantContext.SetTenant(tenant.Id, tenant.Domain);

        // 5. Continue to the next middleware or endpoint
        await _next(context);
    }
}