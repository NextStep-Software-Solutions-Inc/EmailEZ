using Carter;
using MediatR;
using EmailEZ.Application.Features.WorkspaceApiKeys.Queries;
using EmailEZ.Application.Features.WorkspaceApiKeys.Commands;

namespace EmailEZ.Api.Endpoints;

public class WorkspaceApiKeyEndpoints : CarterModule
{   
    private const string WorkspaceApiKeyBaseRoute = "/api/v1/workspaces/{workspaceId:guid}/users/{userId}/apikeys";
    public WorkspaceApiKeyEndpoints() : base() { }

    public override void AddRoutes(IEndpointRouteBuilder app)
    {

        var group = app.MapGroup(WorkspaceApiKeyBaseRoute)
                            .WithTags("Workspace API Keys")
                            .WithOpenApi()
                            .RequireAuthorization();

        // POST /api/v1/workspaces/{workspaceId}/users/{userId}/apikeys
        // This endpoint creates a new API key for the specified workspace user and returns the plain key
        group.MapPost("/", async (
            Guid workspaceId,
            string userId,
            CreateWorkspaceApiKeyCommand command,
            IMediator mediator,
            CancellationToken cancellationToken) =>
        {
            var result = await mediator.Send(command with { UserId = userId, WorkspaceId = workspaceId }, cancellationToken);
            if (result == null || !result.Success)
            {
                return Results.BadRequest(new { Message = result?.Message ?? "Failed to create API key." });
            }

            return Results.Ok(new CreateWorkspaceApiKeyResponse(
                result.ApiKeyId,
                result.PlainKey,
                result.Success,
                result.Message
            ));
        })
        .WithName("CreateWorkspaceApiKey")
        .WithSummary("Create a new API key for a workspace user")
        .WithDescription("Generates a new API key for the specified workspace user. The plain key is shown only once.")
        .Produces(StatusCodes.Status400BadRequest)
        .ProducesValidationProblem()
        .ProducesProblem(StatusCodes.Status500InternalServerError)
        .Produces<CreateWorkspaceApiKeyResponse>(StatusCodes.Status200OK);

        // GET /api/v1/workspaces/{workspaceId}/users/{userId}/apikeys
        // This endpoint retrieves all API keys for the specified workspace user
        group.MapGet("/", async (
            Guid workspaceId,
            string userId,
            IMediator mediator,
            CancellationToken cancellationToken) =>
        {
            var result = await mediator.Send(new GetWorkspaceApiKeysQuery(workspaceId, userId), cancellationToken);
            return Results.Ok(result);
        })
        .WithName("GetWorkspaceApiKeys")
        .WithSummary("List all API keys for a workspace user")
        .WithDescription("Retrieves all API keys for the specified workspace user.")
        .Produces(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status500InternalServerError)
        .Produces<List<GetWorkspaceApiKeyResponse>>(StatusCodes.Status200OK);

        // POST /api/v1/workspaces/{workspaceId}/users/{userId}/apikeys/{apiKeyId}/regenerate
        // This endpoint regenerates the API key and returns the new plain key
        group.MapPost("/{apiKeyId}/regenerate", async (
            Guid workspaceId,
            string userId,
            Guid apiKeyId,
            IMediator mediator,
            CancellationToken cancellationToken) =>
        {
            var result = await mediator.Send(new RegenerateWorkspaceApiKeyCommand(workspaceId, userId, apiKeyId), cancellationToken);
            return result.Success ? Results.Ok(result) : Results.BadRequest(result);
        })
        .WithName("RegenerateWorkspaceApiKey")
        .WithSummary("Regenerate an API key")
        .WithDescription("Regenerates the specified API key and returns the new plain key.")
        .Produces(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status500InternalServerError)
        .Produces<string>(StatusCodes.Status200OK);

        // DELETE /api/v1/workspaces/{workspaceId}/users/{userId}/apikeys/{apiKeyId} 
        // This endpoint revokes (soft-deletes) the API key
        group.MapDelete("/{apiKeyId}", async (
            Guid workspaceId,
            string userId,
            Guid apiKeyId,
            IMediator mediator,
            CancellationToken cancellationToken) =>
        {
            var result = await mediator.Send(new RevokeWorkspaceApiKeyCommand(workspaceId, userId, apiKeyId), cancellationToken);
            return result.Success ? Results.Ok(result) : Results.NotFound(result);
        })
        .WithName("RevokeWorkspaceApiKey")
        .WithSummary("Revoke an API key")
        .WithDescription("Revokes (soft-deletes) the specified API key.")
        .Produces(StatusCodes.Status404NotFound)
        .ProducesProblem(StatusCodes.Status500InternalServerError)
        .Produces<RevokeWorkspaceApiKeyResponse>(StatusCodes.Status200OK);
    }
}