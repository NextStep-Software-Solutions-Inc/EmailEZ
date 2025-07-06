using Carter;
using EmailEZ.Application.Features.EmailConfigurations.Commands.CreateEmailConfiguration; // New command
using EmailEZ.Application.Features.EmailConfigurations.Commands.DeleteEmailConfiguration;
using EmailEZ.Application.Features.EmailConfigurations.Commands.UpdateEmailConfiguration;
using EmailEZ.Application.Features.EmailConfigurations.Queries.GetAllEmailConfigurations;
using EmailEZ.Application.Features.EmailConfigurations.Queries.GetEmailConfigurationById;
using FluentValidation; // For ValidationException
using MediatR;
using Microsoft.AspNetCore.Mvc; // For [FromBody]

namespace EmailEZ.Api.Endpoints;

/// <summary>
/// Defines Email Configuration API endpoints using Carter.
/// </summary>
public class EmailConfigurationEndpoints : CarterModule
{
    // Define the base route string for email configurations nested under workspaces
    private const string EmailConfigsBaseRoute = "/api/v1/workspaces/{workspaceId:guid}/email-configurations";

    public EmailConfigurationEndpoints() : base()
    {
        // No base route in constructor as we're using MapGroup for dynamic workspaceId
    }

    public override void AddRoutes(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup(EmailConfigsBaseRoute)
                        .WithTags("Email Configurations") // Tag for Swagger UI grouping
                        .WithOpenApi()
                        .RequireAuthorization();

        // POST /api/v1/workspaces/{workspaceId}/email-configurations
        group.MapPost("/",
        async (
            Guid workspaceId, // From the route
            [FromBody] CreateEmailConfigurationCommand command, // Request body
            IMediator mediator,
            ILogger<EmailConfigurationEndpoints> logger
        ) =>
        {
            try
            {
                // Ensure the workspaceId from the route matches the command's workspaceId if it's there
                // (Though for creation, it's often preferred to just take from route)
                if (workspaceId != command.WorkspaceId)
                {
                    return Results.BadRequest("Workspace ID in URL path must match Workspace ID in request body.");
                }

                var response = await mediator.Send(command);

                if (response.Success)
                {
                    // Return 201 Created with the location of the new resource
                    return Results.Created($"{EmailConfigsBaseRoute.Replace("{workspaceId:guid}", workspaceId.ToString())}/{response.EmailConfigurationId}", response);
                }
                else
                {
                    return Results.BadRequest(response); // For business rule failures
                }
            }
            catch (ValidationException ex)
            {
                var errors = ex.Errors.ToDictionary(e => e.PropertyName, e => new[] { e.ErrorMessage });
                return Results.ValidationProblem(errors);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred while creating an email configuration for Workspace ID: {WorkspaceId}", workspaceId);
                return Results.Problem("An unexpected error occurred.", statusCode: StatusCodes.Status500InternalServerError);
            }
        })
        .WithName("CreateEmailConfiguration")
        .Produces<CreateEmailConfigurationResponse>(StatusCodes.Status201Created)
        .Produces<CreateEmailConfigurationResponse>(StatusCodes.Status400BadRequest)
        .ProducesValidationProblem()
        .ProducesProblem(StatusCodes.Status500InternalServerError);

        // GET /api/v1/workspaces/{workspaceId}/email-configurations/{id}
        group.MapGet("/{id:guid}",
        async (Guid workspaceId, Guid id, IMediator mediator, ILogger<EmailConfigurationEndpoints> logger) =>
        {
            try
            {
                var query = new GetEmailConfigurationByIdQuery(workspaceId, id);
                var response = await mediator.Send(query);

                if (response == null)
                {
                    return Results.NotFound($"Email configuration with ID '{id}' not found for workspace '{workspaceId}'.");
                }

                return Results.Ok(response);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred while retrieving email configuration ID: {ConfigId} for Workspace ID: {WorkspaceId}", id, workspaceId);
                return Results.Problem("An unexpected error occurred.", statusCode: StatusCodes.Status500InternalServerError);
            }
        })
        .WithName("GetEmailConfigurationById")
        .Produces<GetEmailConfigurationByIdResponse>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound)
        .ProducesProblem(StatusCodes.Status500InternalServerError);


        // GET /api/v1/workspaces/{workspaceId}/email-configurations
        group.MapGet("/",
        async (Guid workspaceId, IMediator mediator, ILogger<EmailConfigurationEndpoints> logger) =>
        {
            try
            {
                var query = new GetAllEmailConfigurationsQuery(workspaceId);
                var response = await mediator.Send(query);

                return Results.Ok(response); // Will return an empty list if no configs exist
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred while retrieving all email configurations for Workspace ID: {WorkspaceId}", workspaceId);
                return Results.Problem("An unexpected error occurred.", statusCode: StatusCodes.Status500InternalServerError);
            }
        })
        .WithName("GetAllEmailConfigurations")
        .Produces<List<GetAllEmailConfigurationsResponse>>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status500InternalServerError);



        // PUT /api/v1/workspaces/{workspaceId}/email-configurations/{id}
        group.MapPut("/{id:guid}",
        async (
            Guid workspaceId,
            Guid id, // From the route
            [FromBody] UpdateEmailConfigurationCommand command, // Request body
            IMediator mediator,
            ILogger<EmailConfigurationEndpoints> logger
        ) =>
        {
            try
            {
                // Ensure the ID from the route matches the ID in the command body
                if (id != command.Id || workspaceId != command.WorkspaceId)
                {
                    return Results.BadRequest("ID or Workspace ID in URL path must match corresponding IDs in request body.");
                }

                var response = await mediator.Send(command);

                if (response.Success)
                {
                    return Results.NoContent(); // 204 No Content for successful update
                }
                else
                {
                    if (!string.IsNullOrEmpty(response.Message) && response.Message.Contains("not found"))
                    {
                        return Results.NotFound(response); // 404 Not Found
                    }
                    // For other business validation errors (e.g., duplicate host/username combo)
                    return Results.BadRequest(response); // 400 Bad Request
                }
            }
            catch (ValidationException ex) // If you have FluentValidation for UpdateEmailConfigurationCommand
            {
                var errors = ex.Errors.ToDictionary(e => e.PropertyName, e => new[] { e.ErrorMessage });
                return Results.ValidationProblem(errors);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred while updating email configuration ID: {ConfigId} for Workspace ID: {WorkspaceId}", id, workspaceId);
                return Results.Problem("An unexpected error occurred.", statusCode: StatusCodes.Status500InternalServerError);
            }
        })
        .WithName("UpdateEmailConfiguration")
        .Produces(StatusCodes.Status204NoContent)
        .Produces<UpdateEmailConfigurationResponse>(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status404NotFound)
        .ProducesValidationProblem()
        .ProducesProblem(StatusCodes.Status500InternalServerError);

        // DELETE /api/v1/workspaces/{workspaceId}/email-configurations/{id}
        group.MapDelete("/{id:guid}",
        async (
            Guid workspaceId,
            Guid id, // From the route
            IMediator mediator,
            ILogger<EmailConfigurationEndpoints> logger
        ) =>
        {
            try
            {
                var command = new DeleteEmailConfigurationCommand(workspaceId, id);
                var response = await mediator.Send(command);

                if (response.Success)
                {
                    return Results.NoContent(); // 204 No Content for successful deletion
                }
                else
                {
                    if (!string.IsNullOrWhiteSpace(response.Message) && response.Message.Contains("not found"))
                    {
                        return Results.NotFound(response); // 404 Not Found
                    }
                    // For other business rule failures on delete (less common for simple deletes)
                    return Results.BadRequest(response); // 400 Bad Request
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred while deleting email configuration ID: {ConfigId} for Workspace ID: {WorkspaceId}", id, workspaceId);
                return Results.Problem("An unexpected error occurred.", statusCode: StatusCodes.Status500InternalServerError);
            }
        })
        .WithName("DeleteEmailConfiguration")
        .Produces(StatusCodes.Status204NoContent)
        .Produces<DeleteEmailConfigurationResponse>(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status404NotFound)
        .ProducesProblem(StatusCodes.Status500InternalServerError);

    }
}