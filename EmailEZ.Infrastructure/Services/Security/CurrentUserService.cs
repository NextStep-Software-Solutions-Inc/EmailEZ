using EmailEZ.Application.Interfaces;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace EmailEZ.Infrastructure.Services.Security;

public class CurrentUserService : ICurrentUserService
{
    private readonly IWorkspaceContext _workspaceContext; // Inject the workspace context
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IWorkspaceContext workspaceContext, IHttpContextAccessor httpContextAccessor)
    {
        _workspaceContext = workspaceContext;
        _httpContextAccessor = httpContextAccessor;
    }

    public string? GetCurrentUserId()
    {
        var userId = _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!string.IsNullOrEmpty(userId))
        {
            return userId;
        }

        if (_workspaceContext.WorkspaceId.HasValue)
        {
            return _workspaceContext.WorkspaceId.Value.ToString();
        }

        return "System";
    }

    public Guid? GetCurrentWorkspaceId()
    {
        if (_workspaceContext.WorkspaceId.HasValue)
        {
            return _workspaceContext.WorkspaceId;
        }

        return null;
    }
}