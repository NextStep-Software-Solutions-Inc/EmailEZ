using MediatR;
using EmailEZ.Application.Interfaces;
using EmailEZ.Domain.Entities;
using EmailEZ.Application.Features.WorkspaceApiKeys.Queries;

namespace EmailEZ.Application.Features.WorkspaceApiKeys.Handlers;

public class GetWorkspaceApiKeysQueryHandler : IRequestHandler<GetWorkspaceApiKeysQuery, List<GetWorkspaceApiKeyResponse>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetWorkspaceApiKeysQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<List<GetWorkspaceApiKeyResponse>> Handle(GetWorkspaceApiKeysQuery request, CancellationToken cancellationToken)
    {
        var apiKeysQuery = _unitOfWork.WorkspaceApiKeys
            .Query()
            .Where(x => x.WorkspaceUser.WorkspaceId == request.WorkspaceId && x.WorkspaceUser.UserId == request.UserId && x.IsActive)
            .Select(x => new GetWorkspaceApiKeyResponse
            {
                Id = x.Id,
                WorkspaceUserId = x.WorkspaceUserId,
                LastUsedAt = x.LastUsedAt,
                IsActive = x.IsActive,
                Name = x.Name
            });

        var apiKeys = await _unitOfWork.WorkspaceApiKeys.ToListAsync(apiKeysQuery, cancellationToken);
        return apiKeys.ToList();
    }
}
