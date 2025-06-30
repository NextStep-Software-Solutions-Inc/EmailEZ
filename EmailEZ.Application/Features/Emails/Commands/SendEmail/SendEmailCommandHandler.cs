using EmailEZ.Application.Interfaces;
using EmailEZ.Domain.Entities;
using EmailEZ.Domain.Enums; // Ensure this is present for EmailStatus
using Hangfire;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EmailEZ.Application.Features.Emails.Commands.SendEmail;

public class SendEmailCommandHandler : IRequestHandler<SendEmailCommand, SendEmailResponse>
{
    private readonly IApplicationDbContext _context;
    private readonly IEmailSender _emailSender;
    private readonly IBackgroundJobClient _backgroundJobClient;

    public SendEmailCommandHandler(
        IApplicationDbContext context,
        IEmailSender emailSender,
        IBackgroundJobClient backgroundJobClient)
    {
        _context = context;
        _emailSender = emailSender;
        _backgroundJobClient = backgroundJobClient;
    }

    public async Task<SendEmailResponse> Handle(SendEmailCommand request, CancellationToken cancellationToken)
    {
        var emailConfig = await _context.EmailConfigurations
            .FirstOrDefaultAsync(ec => ec.Id == request.EmailConfigurationId && ec.TenantId == request.TenantId, cancellationToken);

        if (emailConfig == null)
        {
            return new SendEmailResponse(false, $"Email configuration with ID '{request.EmailConfigurationId}' not found for tenant '{request.TenantId}'.");
        }

        var emailMessage = new EmailMessage
        {
            To = request.ToEmail,
            Bcc = request.BccEmail,
            Cc = request.CcEmail,
            Subject = request.Subject,
            Body = request.Body,
            IsHtml = request.IsHtml,
            FromDisplayName = request.FromDisplayName
        };

        // Create the Email entity here with initial queued status
        var emailToLog = new Email
        {
            TenantId = request.TenantId,
            EmailConfigurationId = request.EmailConfigurationId,
            FromAddress = emailConfig.FromEmail,

            ToAddresses = request.ToEmail,
            CcAddresses = request.CcEmail,
            BccAddresses = request.BccEmail,

            Subject = request.Subject,

            IsHtml = request.IsHtml,
            BodyHtml = request.IsHtml ? (request.Body.Length > 2000 ? string.Concat(request.Body.AsSpan(0, 2000), "...") : request.Body) : null,
            BodyPlainText = !request.IsHtml ? (request.Body.Length > 2000 ? string.Concat(request.Body.AsSpan(0, 2000), "...") : request.Body) : null,
            // You can adjust the truncation length (e.g., 2000 characters)

            Status = EmailStatus.Queued, // Set initial status to Queued
            QueuedAt = DateTimeOffset.UtcNow,
            AttemptCount = 0 // Initial attempt count
        };

        _context.Emails.Add(emailToLog);
        await _context.SaveChangesAsync(cancellationToken); // Save the Email entity with Queued status to get its ID

        // Enqueue the actual email sending and logging to Hangfire
        var jobId = _backgroundJobClient.Enqueue(() => SendEmailAndLogAsync(emailMessage, emailConfig, emailToLog.Id, null)); // Pass the Email entity's ID

        // Update the Email entity with the Hangfire Job ID and save again
        emailToLog.HangfireJobId = jobId;
        await _context.SaveChangesAsync(cancellationToken);

        return new SendEmailResponse(true, "Email sending job enqueued successfully.", jobId);
    }

    // This method will be executed by Hangfire
    // It's public to allow Hangfire to call it.
    [AutomaticRetry(Attempts = 3, DelaysInSeconds = new int[] { 30, 120, 300 }, OnAttemptsExceeded = AttemptsExceededAction.Delete)]
    public async Task SendEmailAndLogAsync(
        EmailMessage message,
        EmailConfiguration config,
        Guid emailId, // Pass the ID of the Email entity to retrieve it
        string? jobId // Hangfire will automatically inject the Job ID if named `jobId`
    )
    {
        // Retrieve the Email entity from the database within the Hangfire job's context
        // This ensures we're working with a fresh, tracked entity
        var email = await _context.Emails.FirstOrDefaultAsync(e => e.Id == emailId, CancellationToken.None);

        if (email == null)
        {
            // Log an error if the email entity cannot be found (shouldn't happen if saved previously)
            // Consider logging to ILogger here if email is null
            return;
        }

        email.AttemptCount++; // Increment attempt count for this specific send attempt
        email.SentAt = DateTimeOffset.UtcNow; // Set the timestamp for this attempt

        EmailSendResult sendResult;
        try
        {
            sendResult = await _emailSender.SendEmailAsync(message, config);

            // Update Email.Status based on sendResult.Success
            email.Status = sendResult.Success ? EmailStatus.Sent : EmailStatus.Failed;
            email.ErrorMessage = sendResult.ErrorMessage;
            email.SmtpResponse = sendResult.SmtpResponse;
        }
        catch (Exception ex)
        {
            // Catch any unexpected exceptions not handled by SmtpEmailSender
            email.Status = EmailStatus.Failed; // Mark as failed
            email.ErrorMessage = $"Unexpected error during Hangfire job: {ex.Message}";
            email.SmtpResponse = ex.ToString(); // Log full exception
        }
        finally
        {
            // Save the updated Email entity status and details
            await _context.SaveChangesAsync(CancellationToken.None);
        }
    }
}