using EmailEZ.Application.Interfaces; // For IApiKeyHasher, IApplicationDbContext, IWorkspaceContext

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
            return Results.Unauthorized();
        }

        using var scope = httpContext.RequestServices.CreateScope();
        var apiKeyHasher = scope.ServiceProvider.GetRequiredService<IApiKeyHasher>();
        var workspaceContext = scope.ServiceProvider.GetRequiredService<IWorkspaceContext>();
        var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

        // 2. Compute fast hash of the incoming API key
        var fastHash = apiKeyHasher.ComputeFastHash(extractedApiKey.ToString());

        // 3. Query only candidates with matching fast hash
        var waksQuery = unitOfWork.WorkspaceApiKeys
            .Query()
            .Where(t => t.IsActive && t.ApiKeyFastHash == fastHash)
            .Select(t => new
            {
                t.Id,
                t.ApiKeyHash,
                t.LastUsedAt,
                t.Workspace.Domain,
                WorkspaceId = t.Workspace.Id,
            });

        var waks = await unitOfWork.WorkspaceApiKeys.ToListAsync(waksQuery, CancellationToken.None);

        // 4. Verify Argon2id hash in memory
        var matchingWorkspaceData = waks.FirstOrDefault(t =>
            !string.IsNullOrEmpty(t.ApiKeyHash) &&
            apiKeyHasher.VerifyApiKey(extractedApiKey.ToString(), t.ApiKeyHash)
        );

        if (matchingWorkspaceData == null)
        {
            return Results.Unauthorized();
        }

        workspaceContext.SetWorkspace(matchingWorkspaceData.WorkspaceId, matchingWorkspaceData.Domain);

        var fullWorkspaceApiKeyEntity = await unitOfWork.WorkspaceApiKeys.GetByIdAsync(matchingWorkspaceData.Id, CancellationToken.None);
        if (fullWorkspaceApiKeyEntity != null)
        {
            fullWorkspaceApiKeyEntity.LastUsedAt = DateTimeOffset.UtcNow;
            await unitOfWork.SaveChangesAsync();
        }

        return await next.Invoke(context);
    }
    
}