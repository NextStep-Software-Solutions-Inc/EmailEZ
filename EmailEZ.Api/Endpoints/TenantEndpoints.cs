using System.Net; // For HttpStatusCode
using Carter; // Required for CarterModule
using Clerk.Net.AspNetCore.Security;
using EmailEZ.Application.Features.Tenants.Commands.CreateTenant;
using EmailEZ.Application.Features.Tenants.Commands.DeleteTenant;
using EmailEZ.Application.Features.Tenants.Commands.UpdateTenant;
using EmailEZ.Application.Features.Tenants.Queries.GetAllTenants;
using EmailEZ.Application.Features.Tenants.Queries.GetTenantById;
using FluentValidation; // For ValidationException
using MediatR; // For IMediator
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc; // For [FromBody]

namespace EmailEZ.Api.Endpoints;

/// <summary>
/// Defines Tenant-related API endpoints using Carter.
/// </summary>
public class TenantEndpoints : CarterModule
{
    private const string TenantsBaseRoute = "/api/v1/tenants";

    public TenantEndpoints() : base() // Use the constant here
    {
        // Optionally, you can configure the group directly here if not using WithTags() later
    }

    public override void AddRoutes(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup(TenantsBaseRoute) 
                       .WithTags("Tenants")
                       .WithOpenApi()
                       .RequireAuthorization(ClerkAuthenticationDefaults.AuthenticationScheme); // Ensure all endpoints in this group require authorization

        // POST /api/v1/tenants (because group is /api/v1/tenants and we map to "/")
        group.MapPost("/",
            async ([FromBody] CreateTenantCommand command, IMediator mediator, ILogger<TenantEndpoints> logger) =>
            {
                try
                {
                    var response = await mediator.Send(command);

                    if (response.IsSuccess)
                    {
                        return Results.Created($"{TenantsBaseRoute}/{response.TenantId}", response);
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
                    logger.LogError(ex, "An error occurred while creating a tenant.");
                    return Results.Problem("An unexpected error occurred while processing your request.", statusCode: StatusCodes.Status500InternalServerError);
                }
            }
        )
        .WithName("CreateTenant")
        .Produces<CreateTenantResponse>((int)HttpStatusCode.Created)
        .Produces<CreateTenantResponse>((int)HttpStatusCode.BadRequest)
        .ProducesValidationProblem()
        .ProducesProblem((int)HttpStatusCode.InternalServerError);

        // GET /api/v1/tenants (because group is /api/v1/tenants and we map to "/")
        group.MapGet("/",
            async (IMediator mediator, ILogger<TenantEndpoints> logger) =>
            {
                try
                {
                    var query = new GetAllTenantsQuery();
                    var response = await mediator.Send(query);

                    // Always returns a list, even if empty, so 200 OK is appropriate.
                    return Results.Ok(response);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "An error occurred while retrieving all tenants.");
                    return Results.Problem("An unexpected error occurred while processing your request.", statusCode: StatusCodes.Status500InternalServerError);
                }
            }
        )
        .WithName("GetAllTenants")
        .Produces<List<GetAllTenantsResponse>>(StatusCodes.Status200OK) // Expects a list of responses
        .ProducesProblem(StatusCodes.Status500InternalServerError);

        // GET /api/v1/tenants/{id:guid}
        group.MapGet("/{id:guid}",
            async (Guid id, IMediator mediator, ILogger<TenantEndpoints> logger) =>
            {
                try
                {
                    var query = new GetTenantByIdQuery(id);
                    var response = await mediator.Send(query);

                    if (response == null)
                    {
                        return Results.NotFound($"Tenant with ID '{id}' not found.");
                    }

                    return Results.Ok(response);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "An error occurred while retrieving tenant with ID: {TenantId}", id);
                    return Results.Problem("An unexpected error occurred while processing your request.", statusCode: StatusCodes.Status500InternalServerError);
                }
            }
        )
        .WithName("GetTenantById")
        .Produces<GetTenantByIdResponse>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound)
        .ProducesProblem(StatusCodes.Status500InternalServerError);

        // PUT /api/v1/tenants/{id:guid}
        group.MapPut("/{id:guid}",
            async (Guid id, [FromBody] UpdateTenantCommand command, IMediator mediator, ILogger<TenantEndpoints> logger) =>
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
                catch (ValidationException ex) // If you have FluentValidation for UpdateTenantCommand
                {
                    var errors = ex.Errors.ToDictionary(e => e.PropertyName, e => new[] { e.ErrorMessage });
                    return Results.ValidationProblem(errors);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "An error occurred while updating tenant with ID: {TenantId}", id);
                    return Results.Problem("An unexpected error occurred while processing your request.", statusCode: StatusCodes.Status500InternalServerError);
                }
            }
        )
        .WithName("UpdateTenant")
        .Produces(StatusCodes.Status204NoContent) // Successful update, no content returned
        .Produces<UpdateTenantResponse>(StatusCodes.Status400BadRequest) // For business validation issues (e.g., duplicate)
        .Produces(StatusCodes.Status404NotFound) // If tenant not found
        .ProducesValidationProblem() // If FluentValidation for command input fails
        .ProducesProblem(StatusCodes.Status500InternalServerError);

        // DELETE /api/v1/tenants/{id}
        group.MapDelete("/{id:guid}",
            async (Guid id, IMediator mediator, ILogger<TenantEndpoints> logger) =>
            {
                try
                {
                    var command = new DeleteTenantCommand(id);
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
                    logger.LogError(ex, "An error occurred while deleting tenant with ID: {TenantId}", id);
                    return Results.Problem("An unexpected error occurred while processing your request.", statusCode: StatusCodes.Status500InternalServerError);
                }
            }
        )
        .WithName("DeleteTenant")
        .Produces(StatusCodes.Status204NoContent) // Successful deletion, no content returned
        .Produces<DeleteTenantResponse>(StatusCodes.Status400BadRequest) // For business validation (less common here)
        .Produces(StatusCodes.Status404NotFound) // If tenant not found
        .ProducesProblem(StatusCodes.Status500InternalServerError);


    }
}