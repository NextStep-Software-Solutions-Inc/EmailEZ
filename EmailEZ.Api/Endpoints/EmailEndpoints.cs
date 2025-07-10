using System.ComponentModel.DataAnnotations;
using Carter;
using EmailEZ.Api.Filters;
using EmailEZ.Application.Common;
using EmailEZ.Application.Common.Models;
using EmailEZ.Application.Features.Emails.Commands.SendEmail;
using EmailEZ.Application.Features.Emails.Dtos;
using EmailEZ.Application.Features.Emails.Queries.GetAllEmails;
using EmailEZ.Application.Features.Emails.Queries.GetEmailById;
using EmailEZ.Domain.Enums;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace EmailEZ.Api.Endpoints; 

public class EmailEndpoints : CarterModule
{
    private const string EmailBaseRoute = "/api/v1/workspaces/{workspaceId:guid}/emails";

    public EmailEndpoints() : base() { }

    public override void AddRoutes(IEndpointRouteBuilder app)
    {
        // Define the base group for email-related endpoints under a workspace
        // This makes it easier to apply common configurations like tags or authorization if needed
        var group = app.MapGroup(EmailBaseRoute)
                            .WithTags("Emails")
                            .WithOpenApi()
                            .RequireAuthorization();

        // POST /api/v1/send-email
        app.MapPost("/api/v1/send-email",
        async (
            [FromBody] SendEmailCommand command, // The command contains all email details
            IMediator mediator,
            ILogger<EmailConfigurationEndpoints> logger
        ) =>
        {
            try
            {
                var response = await mediator.Send(command);

                if (response.Success)
                {
                    // 202 Accepted: The request has been accepted for processing, but the processing is not yet complete.
                    // This is ideal for asynchronous operations like background jobs.
                    return Results.Accepted(null, // No specific location for a queued job
                        value: new
                        {
                            response.Message,
                            response.HangfireJobId
                        }
                    );
                }
                else
                {
                    // Return 400 Bad Request for business rule failures (e.g., config not found)
                    return Results.BadRequest(response);
                }
            }
            catch (FluentValidation.ValidationException ex) // If you add FluentValidation later
            {
                var errors = ex.Errors.ToDictionary(e => e.PropertyName, e => new[] { e.ErrorMessage });
                return Results.ValidationProblem(errors);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred while enqueuing email send for command: {@Command}", command);
                return Results.Problem("An unexpected error occurred while processing your request.", statusCode: StatusCodes.Status500InternalServerError);
            }
        })
        .WithTags("Send Email using ApiKey")
        .WithName("SendEmail-APIKEY")
        .Produces(StatusCodes.Status202Accepted) // Indicates the job was accepted
        .Produces<SendEmailResponse>(StatusCodes.Status400BadRequest)
        .ProducesValidationProblem()
        .ProducesProblem(StatusCodes.Status500InternalServerError)
        .AddEndpointFilter<ApiKeyAuthenticationFilter>()
        .AllowAnonymous();

        // POST /api/v1/workspaces/{workspaceId}/send-email
        group.MapPost("/send-email",
        async (
            Guid workspaceId,
            [FromBody] SendEmailCommand command, // The command contains all email details
            IMediator mediator,
            ILogger<EmailConfigurationEndpoints> logger
        ) =>
        {
            try
            {
                // Ensure the workspaceId from the route matches the command's workspaceId
                if (workspaceId == Guid.Empty)
                {
                    return Results.BadRequest("Workspace ID in URL path cannot be empty.");
                }
                
                var response = await mediator.Send(command);

                if (response.Success)
                {
                    // 202 Accepted: The request has been accepted for processing, but the processing is not yet complete.
                    // This is ideal for asynchronous operations like background jobs.
                    return Results.Accepted(null, // No specific location for a queued job
                        value: new
                        {
                            response.Message,
                            response.HangfireJobId
                        }
                    );
                }
                else
                {
                    // Return 400 Bad Request for business rule failures (e.g., config not found)
                    return Results.BadRequest(response);
                }
            }
            catch (FluentValidation.ValidationException ex) // If you add FluentValidation later
            {
                var errors = ex.Errors.ToDictionary(e => e.PropertyName, e => new[] { e.ErrorMessage });
                return Results.ValidationProblem(errors);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred while enqueuing email send for Workspace ID: {WorkspaceId}", workspaceId);
                return Results.Problem("An unexpected error occurred while processing your request.", statusCode: StatusCodes.Status500InternalServerError);
            }
        })
        .WithName("SendEmail")
        .Produces(StatusCodes.Status202Accepted) // Indicates the job was accepted
        .Produces<SendEmailResponse>(StatusCodes.Status400BadRequest)
        .ProducesValidationProblem()
        .ProducesProblem(StatusCodes.Status500InternalServerError);

        // Map the GET /api/v1/workspaces/{workspaceId}/emails endpoint
        group.MapGet("/", async (
            Guid workspaceId,
            IMediator mediator,
            [Required][FromQuery] int pageNumber = 1, // Default to first page
            [FromQuery] int pageSize = 10,
            [FromQuery] string sortOder = SortOder.Descending,
            [FromQuery] string? emailStatus = null,
            [FromQuery] string? toEmailContains = null,
            [FromQuery] string? subjectContains = null,
            [FromQuery] DateTimeOffset? startDate = null,
            [FromQuery] DateTimeOffset? endDate = null
        ) =>
        {
            EmailStatus? status = null;
            if (!string.IsNullOrEmpty(emailStatus))
            {
                if (Enum.TryParse<EmailStatus>(emailStatus, ignoreCase: true, out var parsedStatus))
                {
                    status = parsedStatus;
                }
                else
                {
                    var validStatuses = string.Join(", ", Enum.GetNames(typeof(EmailStatus)));
                    return Results.BadRequest($"Invalid status value: {emailStatus}, ust be one of: {validStatuses}");
                }
            }
            var query = new GetAllEmailsQuery
            {
                WorkspaceId = workspaceId,
                PageNumber = pageNumber,
                PageSize = pageSize,
                SortOrder = sortOder,
                Status = status,
                ToEmailContains = toEmailContains,
                SubjectContains = subjectContains,
                StartDate = startDate,
                EndDate = endDate
            };

            return Results.Ok(await mediator.Send(query));
        })
        .WithName("GetEmailsForWorkspace")
        .Produces<PaginatedList<EmailDto>>(StatusCodes.Status200OK);
        //};


        // GET /api/v1/workspaces/{workspaceId}/emails/{id} - Retrieve a single email by ID
        group.MapGet("/{id:guid}", async (
            [FromRoute] Guid workspaceId,
            [FromRoute] Guid id,
            IMediator mediator) =>
        {
            var query = new GetEmailByIdQuery { WorkspaceId = workspaceId, EmailId = id };
            var result = await mediator.Send(query);

            return result != null ? Results.Ok(result) : Results.NotFound();
        })
        .WithName("GetEmailByIdForWorkspace")
        .Produces<EmailDetailsDto>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound);
    }
}
