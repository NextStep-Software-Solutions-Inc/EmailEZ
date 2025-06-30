// EmailEZ.Api/Endpoints/EmailEndpoints.cs

using Carter;
using EmailEZ.Application.Common.Models;
using EmailEZ.Application.Features.Emails.Commands.SendEmail;
using EmailEZ.Application.Features.Emails.Queries.GetEmailById;
using EmailEZ.Application.Features.Emails.Queries.GetEmails;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace EmailEZ.Api.Endpoints; 

public class EmailEndpoints : CarterModule 
{
    private const string EmailConfigsBaseRoute = "/api/v1/tenants/{tenantId:guid}/emails";

    public EmailEndpoints() : base()
    {

    }

    public override void AddRoutes(IEndpointRouteBuilder app)
    {
        // Define the base group for email-related endpoints under a tenant
        // This makes it easier to apply common configurations like tags or authorization if needed
        var group = app.MapGroup(EmailConfigsBaseRoute)
                            .WithTags("Emails")
                            .WithOpenApi();

        // POST /api/v1/tenants/{tenantId}/send-email
        group.MapPost("/send-email",
        async (
            Guid tenantId,
            [FromBody] SendEmailCommand command, // The command contains all email details
            IMediator mediator,
            ILogger<EmailConfigurationEndpoints> logger
        ) =>
        {
            try
            {
                // Ensure the tenantId from the route matches the command's tenantId
                if (tenantId != command.TenantId)
                {
                    return Results.BadRequest("Tenant ID in URL path must match Tenant ID in request body.");
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
            catch (ValidationException ex) // If you add FluentValidation later
            {
                var errors = ex.Errors.ToDictionary(e => e.PropertyName, e => new[] { e.ErrorMessage });
                return Results.ValidationProblem(errors);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred while enqueuing email send for Tenant ID: {TenantId}", tenantId);
                return Results.Problem("An unexpected error occurred while processing your request.", statusCode: StatusCodes.Status500InternalServerError);
            }
        })
        .WithName("SendEmail")
        .Produces(StatusCodes.Status202Accepted) // Indicates the job was accepted
        .Produces<SendEmailResponse>(StatusCodes.Status400BadRequest)
        .ProducesValidationProblem()
        .ProducesProblem(StatusCodes.Status500InternalServerError);
        // Map the GET /api/v1/tenants/{tenantId}/emails endpoint
        group.MapGet("/", async (
            Guid tenantId,
            IMediator mediator) =>
        {

            var query = new GetAllEmailsQuery
            {
                TenantId = tenantId,
                PageNumber = 1, // Default to first page
                PageSize = 10,
                EndDate = DateTime.Now,
                StartDate = DateTime.Now.AddDays(-30), // Default to last 30 days
                Status = null, // No filter by status
                ToEmailContains = null, // No filter by recipient email
                SubjectContains = null // No filter by subject
            };

            return Results.Ok(await mediator.Send(query));
        })
        .WithName("GetEmailsForTenant")
        .Produces<PaginatedList<EmailDto>>(StatusCodes.Status200OK);

        // GET /api/v1/tenants/{tenantId}/emails/{emailId} - Retrieve a single email by ID
        group.MapGet("/{id:guid}", async (
            Guid tenantId,
            Guid emailId,
            IMediator mediator) =>
        {
            var query = new GetEmailByIdQuery { TenantId = tenantId, EmailId = emailId };
            var result = await mediator.Send(query);

            return result != null ? Results.Ok(result) : Results.NotFound();
        })
        .WithName("GetEmailByIdForTenant")
        .Produces<EmailDetailsDto>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound);
    }
}
