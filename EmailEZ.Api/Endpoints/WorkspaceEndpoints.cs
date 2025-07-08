using System.Net; // For HttpStatusCode
using Carter; // Required for CarterModule
using EmailEZ.Application.Features.Workspaces.Commands.CreateWorkspace;
using EmailEZ.Application.Features.Workspaces.Commands.DeleteWorkspace;
using EmailEZ.Application.Features.Workspaces.Commands.UpdateWorkspace;
using EmailEZ.Application.Features.Workspaces.Queries.GetAllWorkspaces;
using EmailEZ.Application.Features.Workspaces.Queries.GetWorkspaceById;
using EmailEZ.Application.Features.Workspaces.Queries.GetWorkspaceAnalytics;
using FluentValidation; // For ValidationException
using MediatR; // For IMediator
using Microsoft.AspNetCore.Mvc; // For [FromBody]

namespace EmailEZ.Api.Endpoints;

/// <summary>
/// Defines Workspace-related API endpoints using Carter.
/// </summary>
public class WorkspaceEndpoints : CarterModule
{
    private const string WorkspacesBaseRoute = "/api/v1/workspaces";

    public WorkspaceEndpoints() : base() // Use the constant here
    {
        // Optionally, you can configure the group directly here if not using WithTags() later
    }

    public override void AddRoutes(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup(WorkspacesBaseRoute) 
                       .WithTags("Workspaces")
                       .WithOpenApi()
                       .RequireAuthorization();

        // POST /api/v1/workspaces (because group is /api/v1/workspaces and we map to "/")
        group.MapPost("/",
            async ([FromBody] CreateWorkspaceCommand command, IMediator mediator, ILogger<WorkspaceEndpoints> logger) =>
            {
                try
                {
                    var response = await mediator.Send(command);

                    if (response.IsSuccess)
                    {
                        return Results.Created($"{WorkspacesBaseRoute}/{response.WorkspaceId}", response);
                    }
                    else
                    {
                        return Results.BadRequest(response);
                    }
                }
                catch (ValidationException ex)
                {
                    // Fix: Convert ValidationProblemDetails to the expected type
                    var errors = ex.Errors.ToDictionary(e => e.PropertyName, e => new[] { e.ErrorMessage });
                    return Results.ValidationProblem(errors);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "An error occurred while creating a workspace.");
                    return Results.Problem("An unexpected error occurred while processing your request.", statusCode: StatusCodes.Status500InternalServerError);
                }
            }
        )
        .WithName("CreateWorkspace")
        .Produces<CreateWorkspaceResponse>((int)HttpStatusCode.Created)
        .Produces<CreateWorkspaceResponse>((int)HttpStatusCode.BadRequest)
        .ProducesValidationProblem()
        .ProducesProblem((int)HttpStatusCode.InternalServerError);

        // GET /api/v1/workspaces (because group is /api/v1/workspaces and we map to "/")
        group.MapGet("/",
            async (IMediator mediator, ILogger<WorkspaceEndpoints> logger) =>
            {
                try
                {
                    var query = new GetAllWorkspacesQuery();
                    var response = await mediator.Send(query);

                    // Always returns a list, even if empty, so 200 OK is appropriate.
                    return Results.Ok(response);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "An error occurred while retrieving all workspaces.");
                    return Results.Problem("An unexpected error occurred while processing your request.", statusCode: StatusCodes.Status500InternalServerError);
                }
            }
        )
        .WithName("GetAllWorkspaces")
        .Produces<List<GetAllWorkspacesResponse>>(StatusCodes.Status200OK) // Expects a list of responses
        .ProducesProblem(StatusCodes.Status500InternalServerError);

        // GET /api/v1/workspaces/{id:guid}
        group.MapGet("/{id:guid}",
            async (Guid id, IMediator mediator, ILogger<WorkspaceEndpoints> logger) =>
            {
                try
                {
                    var query = new GetWorkspaceByIdQuery(id);
                    var response = await mediator.Send(query);

                    if (response == null)
                    {
                        return Results.NotFound($"Workspace with ID '{id}' not found.");
                    }

                    return Results.Ok(response);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "An error occurred while retrieving workspace with ID: {WorkspaceId}", id);
                    return Results.Problem("An unexpected error occurred while processing your request.", statusCode: StatusCodes.Status500InternalServerError);
                }
            }
        )
        .WithName("GetWorkspaceById")
        .Produces<GetWorkspaceByIdResponse>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound)
        .ProducesProblem(StatusCodes.Status500InternalServerError);

        // PUT /api/v1/workspaces/{id:guid}
        group.MapPut("/{id:guid}",
            async (Guid id, [FromBody] UpdateWorkspaceCommand command, IMediator mediator, ILogger<WorkspaceEndpoints> logger) =>
            {
                try
                {
                    // Ensure the ID in the route matches the ID in the command body
                    if (id != command.Id)
                    {
                        return Results.BadRequest("ID in URL path must match ID in request body.");
                    }

                    var response = await mediator.Send(command);

                    if (response.Success)
                    {
                        return Results.NoContent(); // 204 No Content for successful update with no new resource
                    }
                    else
                    {
                        if (!string.IsNullOrEmpty(response.Message) && response.Message.Contains("not found"))
                        {
                            return Results.NotFound(response); // 404 Not Found
                        }
                        // For other business validation errors (e.g., duplicate name/domain)
                        return Results.BadRequest(response); // 400 Bad Request
                    }
                }
                catch (ValidationException ex) // If you have FluentValidation for UpdateWorkspaceCommand
                {
                    var errors = ex.Errors.ToDictionary(e => e.PropertyName, e => new[] { e.ErrorMessage });
                    return Results.ValidationProblem(errors);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "An error occurred while updating workspace with ID: {WorkspaceId}", id);
                    return Results.Problem("An unexpected error occurred while processing your request.", statusCode: StatusCodes.Status500InternalServerError);
                }
            }
        )
        .WithName("UpdateWorkspace")
        .Produces(StatusCodes.Status204NoContent) // Successful update, no content returned
        .Produces<UpdateWorkspaceResponse>(StatusCodes.Status400BadRequest) // For business validation issues (e.g., duplicate)
        .Produces(StatusCodes.Status404NotFound) // If workspace not found
        .ProducesValidationProblem() // If FluentValidation for command input fails
        .ProducesProblem(StatusCodes.Status500InternalServerError);

        // DELETE /api/v1/workspaces/{id}
        group.MapDelete("/{id:guid}",
            async (Guid id, IMediator mediator, ILogger<WorkspaceEndpoints> logger) =>
            {
                try
                {
                    var command = new DeleteWorkspaceCommand(id);
                    var response = await mediator.Send(command);

                    if (response.Success)
                    {
                        return Results.NoContent(); // 204 No Content for successful deletion
                    }
                    else
                    {
                        if (!string.IsNullOrEmpty(response.Message) && response.Message.Contains("not found"))
                        {
                            return Results.NotFound(response); // 404 Not Found
                        }
                        // For other potential business rule failures on delete (less common)
                        return Results.BadRequest(response); // 400 Bad Request
                    }
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "An error occurred while deleting workspace with ID: {WorkspaceId}", id);
                    return Results.Problem("An unexpected error occurred while processing your request.", statusCode: StatusCodes.Status500InternalServerError);
                }
            }
        )
        .WithName("DeleteWorkspace")
        .Produces(StatusCodes.Status204NoContent) // Successful deletion, no content returned
        .Produces<DeleteWorkspaceResponse>(StatusCodes.Status400BadRequest) // For business validation (less common here)
        .Produces(StatusCodes.Status404NotFound) // If workspace not found
        .ProducesProblem(StatusCodes.Status500InternalServerError);

        // GET /api/v1/workspaces/{id:guid}/analytics
        group.MapGet("/{id:guid}/analytics",
            async (Guid id, [FromQuery] int? daysBack, IMediator mediator, ILogger<WorkspaceEndpoints> logger) =>
            {
                var query = new GetWorkspaceAnalyticsQuery(id, daysBack);
                var response = await mediator.Send(query);

                if (response == null)
                {
                    return Results.NotFound($"Workspace with ID '{id}' not found.");
                }

                return Results.Ok(response);
            }
                
        )
        .WithName("GetWorkspaceAnalytics")
        .Produces<GetWorkspaceAnalyticsResponse>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound)
        .ProducesProblem(StatusCodes.Status500InternalServerError);
    }
}