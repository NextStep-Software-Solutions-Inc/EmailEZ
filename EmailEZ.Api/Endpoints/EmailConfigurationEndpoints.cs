using Carter;
using EmailEZ.Application.Features.EmailConfigurations.Commands.CreateEmailConfiguration; // New command
using EmailEZ.Application.Features.EmailConfigurations.Commands.DeleteEmailConfiguration;
using EmailEZ.Application.Features.EmailConfigurations.Commands.UpdateEmailConfiguration;
using EmailEZ.Application.Features.EmailConfigurations.Queries.GetAllEmailConfigurations;
using EmailEZ.Application.Features.EmailConfigurations.Queries.GetEmailConfigurationById;
using EmailEZ.Application.Features.Emails.Commands.SendEmail;
using FluentValidation; // For ValidationException
using MediatR;
using Microsoft.AspNetCore.Mvc; // For [FromBody]

namespace EmailEZ.Api.Endpoints;

/// <summary>
/// Defines Email Configuration API endpoints using Carter.
/// </summary>
public class EmailConfigurationEndpoints : CarterModule
{
    // Define the base route string for email configurations nested under tenants
    private const string EmailConfigsBaseRoute = "/api/v1/tenants/{tenantId:guid}/email-configurations";

    public EmailConfigurationEndpoints() : base()
    {
        // No base route in constructor as we're using MapGroup for dynamic tenantId
    }

    public override void AddRoutes(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup(EmailConfigsBaseRoute)
                        .WithTags("Email Configurations") // Tag for Swagger UI grouping
                        .WithOpenApi()
                        .RequireAuthorization();

        // POST /api/v1/tenants/{tenantId}/email-configurations
        group.MapPost("/",
        async (
            Guid tenantId, // From the route
            [FromBody] CreateEmailConfigurationCommand command, // Request body
            IMediator mediator,
            ILogger<EmailConfigurationEndpoints> logger
        ) =>
        {
            try
            {
                // Ensure the tenantId from the route matches the command's tenantId if it's there
                // (Though for creation, it's often preferred to just take from route)
                if (tenantId != command.TenantId)
                {
                    return Results.BadRequest("Tenant ID in URL path must match Tenant ID in request body.");
                }

                var response = await mediator.Send(command);

                if (response.Success)
                {
                    // Return 201 Created with the location of the new resource
                    return Results.Created($"{EmailConfigsBaseRoute.Replace("{tenantId:guid}", tenantId.ToString())}/{response.EmailConfigurationId}", response);
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
                logger.LogError(ex, "An error occurred while creating an email configuration for Tenant ID: {TenantId}", tenantId);
                return Results.Problem("An unexpected error occurred.", statusCode: StatusCodes.Status500InternalServerError);
            }
        })
        .WithName("CreateEmailConfiguration")
        .Produces<CreateEmailConfigurationResponse>(StatusCodes.Status201Created)
        .Produces<CreateEmailConfigurationResponse>(StatusCodes.Status400BadRequest)
        .ProducesValidationProblem()
        .ProducesProblem(StatusCodes.Status500InternalServerError);

        // GET /api/v1/tenants/{tenantId}/email-configurations/{id}
        group.MapGet("/{id:guid}",
        async (Guid tenantId, Guid id, IMediator mediator, ILogger<EmailConfigurationEndpoints> logger) =>
        {
            try
            {
                var query = new GetEmailConfigurationByIdQuery(tenantId, id);
                var response = await mediator.Send(query);

                if (response == null)
                {
                    return Results.NotFound($"Email configuration with ID '{id}' not found for tenant '{tenantId}'.");
                }

                return Results.Ok(response);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred while retrieving email configuration ID: {ConfigId} for Tenant ID: {TenantId}", id, tenantId);
                return Results.Problem("An unexpected error occurred.", statusCode: StatusCodes.Status500InternalServerError);
            }
        })
        .WithName("GetEmailConfigurationById")
        .Produces<GetEmailConfigurationByIdResponse>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound)
        .ProducesProblem(StatusCodes.Status500InternalServerError);


        // GET /api/v1/tenants/{tenantId}/email-configurations
        group.MapGet("/",
        async (Guid tenantId, IMediator mediator, ILogger<EmailConfigurationEndpoints> logger) =>
        {
            try
            {
                var query = new GetAllEmailConfigurationsQuery(tenantId);
                var response = await mediator.Send(query);

                return Results.Ok(response); // Will return an empty list if no configs exist
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred while retrieving all email configurations for Tenant ID: {TenantId}", tenantId);
                return Results.Problem("An unexpected error occurred.", statusCode: StatusCodes.Status500InternalServerError);
            }
        })
        .WithName("GetAllEmailConfigurations")
        .Produces<List<GetAllEmailConfigurationsResponse>>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status500InternalServerError);



        // PUT /api/v1/tenants/{tenantId}/email-configurations/{id}
        group.MapPut("/{id:guid}",
        async (
            Guid tenantId,
            Guid id, // From the route
            [FromBody] UpdateEmailConfigurationCommand command, // Request body
            IMediator mediator,
            ILogger<EmailConfigurationEndpoints> logger
        ) =>
        {
            try
            {
                // Ensure the ID from the route matches the ID in the command body
                if (id != command.Id || tenantId != command.TenantId)
                {
                    return Results.BadRequest("ID or Tenant ID in URL path must match corresponding IDs in request body.");
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
                logger.LogError(ex, "An error occurred while updating email configuration ID: {ConfigId} for Tenant ID: {TenantId}", id, tenantId);
                return Results.Problem("An unexpected error occurred.", statusCode: StatusCodes.Status500InternalServerError);
            }
        })
        .WithName("UpdateEmailConfiguration")
        .Produces(StatusCodes.Status204NoContent)
        .Produces<UpdateEmailConfigurationResponse>(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status404NotFound)
        .ProducesValidationProblem()
        .ProducesProblem(StatusCodes.Status500InternalServerError);

        // DELETE /api/v1/tenants/{tenantId}/email-configurations/{id}
        group.MapDelete("/{id:guid}",
        async (
            Guid tenantId,
            Guid id, // From the route
            IMediator mediator,
            ILogger<EmailConfigurationEndpoints> logger
        ) =>
        {
            try
            {
                var command = new DeleteEmailConfigurationCommand(tenantId, id);
                var response = await mediator.Send(command);

                if (response.Success)
                {
                    return Results.NoContent(); // 204 No Content for successful deletion
                }
                else
                {
                    if (response.Message.Contains("not found"))
                    {
                        return Results.NotFound(response); // 404 Not Found
                    }
                    // For other business rule failures on delete (less common for simple deletes)
                    return Results.BadRequest(response); // 400 Bad Request
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred while deleting email configuration ID: {ConfigId} for Tenant ID: {TenantId}", id, tenantId);
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