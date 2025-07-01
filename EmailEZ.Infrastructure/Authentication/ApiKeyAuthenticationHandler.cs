using System.Security.Claims;
using System.Text.Encodings.Web;
using EmailEZ.Application.Interfaces;
using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore; // For ToListAsync
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace EmailEZ.Infrastructure.Authentication;

public class ApiKeyAuthenticationHandler : AuthenticationHandler<ApiKeyAuthenticationOptions>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly IApiKeyHasher _apiKeyHasher;
    private readonly ITenantContext _tenantContext;

    public ApiKeyAuthenticationHandler(
        IOptionsMonitor<ApiKeyAuthenticationOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder,
        ISystemClock clock,
        IApplicationDbContext dbContext, // Injected for database access
        IApiKeyHasher apiKeyHasher,
        ITenantContext tenantContext) // Injected for tenant context population
        : base(options, logger, encoder, clock)
    {
        _dbContext = dbContext;
        _apiKeyHasher = apiKeyHasher;
        _tenantContext = tenantContext;
    }

    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        // 1. Check if the API key header is present
        if (!Request.Headers.ContainsKey(Options.HeaderName))
        {
            // If the header is missing, this scheme cannot authenticate the request.
            // Other authentication schemes (like JWT) will still have a chance.
            return AuthenticateResult.NoResult();
        }

        string? incomingApiKey = Request.Headers[Options.HeaderName].ToString();

        if (string.IsNullOrEmpty(incomingApiKey))
        {
            // Header is present but empty. Authentication fails for this scheme.
            return AuthenticateResult.Fail($"Value for '{Options.HeaderName}' header is empty.");
        }

        // 2. Query the database for potential tenants and perform in-memory verification.
        //    Fetch only active tenants (and potentially filter by IsDeleted if applicable).
        //    The ToListAsync() call brings the data into application memory.
        var tenantsFromDb = await _dbContext.Tenants
                                            .Where(t => t.IsActive /* && !t.IsDeleted */) // Add your IsDeleted check if applicable
                                            .Select(t => new // Project to an anonymous type to retrieve only necessary fields efficiently
                                            {
                                                t.Id,
                                                t.Name,
                                                t.Domain,
                                                t.ApiKeyHash,
                                                t.ApiKeyLastUsedAt
                                            })
                                            .ToListAsync(); // Executes the DB query and loads data into memory

        // 3. Iterate through the in-memory list to find a matching tenant.
        //    The VerifyApiKey call happens in C# memory, not translated to SQL.
        var matchingTenantData = tenantsFromDb.FirstOrDefault(t =>
            !string.IsNullOrEmpty(t.ApiKeyHash) && _apiKeyHasher.VerifyApiKey(incomingApiKey, t.ApiKeyHash)
        );

        if (matchingTenantData == null)
        {
            Logger.LogWarning("Authentication failed for API key from header '{HeaderName}'. No matching active tenant found.", Options.HeaderName);
            return AuthenticateResult.Fail("Invalid API Key or Tenant Not Found.");
        }

        // 4. Update ApiKeyLastUsedAt timestamp for the tenant.
        //    Fetch the full entity if you only selected partial data for update.
        var fullTenantEntity = await _dbContext.Tenants.FindAsync(matchingTenantData.Id);
        if (fullTenantEntity != null)
        {
            fullTenantEntity.ApiKeyLastUsedAt = DateTimeOffset.UtcNow;
            await _dbContext.SaveChangesAsync(CancellationToken.None);
        }

        // 5. Create Claims Principal for the authenticated user/tenant.
        //    These claims will be accessible via HttpContext.User downstream.
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, matchingTenantData.Id.ToString()),
            new Claim("TenantId", matchingTenantData.Id.ToString()), // Custom claim for TenantId
            new Claim(ClaimTypes.Name, matchingTenantData.Name), // Tenant's name
            new Claim("TenantDomain", matchingTenantData.Domain) // Tenant's domain
            // Add any other specific claims related to API key usage or tenant properties
            // e.g., new Claim(ClaimTypes.Role, "ApiConsumer"),
        };

        var identity = new ClaimsIdentity(claims, Scheme.Name);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, Scheme.Name);

        // 6. Populate ITenantContext service for easy access within the request scope.
        _tenantContext.SetTenant(matchingTenantData.Id, matchingTenantData.Domain);
        Logger.LogDebug("TenantContext populated by ApiKeyAuthenticationHandler for TenantId: {TenantId}", matchingTenantData.Id);

        // Authentication successful
        return AuthenticateResult.Success(ticket);
    }
}