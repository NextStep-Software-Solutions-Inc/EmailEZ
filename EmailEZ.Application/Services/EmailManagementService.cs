using EmailEZ.Application.Interfaces;
using EmailEZ.Domain.Entities;
using EmailEZ.Domain.Enums;
using Hangfire;

namespace EmailEZ.Application.Services
{
    /// <summary>
    /// Service for managing email-related business operations.
    /// </summary>
    public class EmailManagementService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IEmailSender _emailSender;
        private readonly IBackgroundJobClient _backgroundJobClient;

        public EmailManagementService(
            IUnitOfWork unitOfWork,
            IEmailSender emailSender,
            IBackgroundJobClient backgroundJobClient)
        {
            _unitOfWork = unitOfWork;
            _emailSender = emailSender;
            _backgroundJobClient = backgroundJobClient;
        }

        /// <summary>
        /// Enqueues an email for sending with proper business logic validation.
        /// </summary>
        /// <param name="request">The email send request.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The email sending result.</returns>
        public async Task<EmailSendingResult> EnqueueEmailAsync(
            EmailSendRequest request,
            CancellationToken cancellationToken = default)
        {
            // ? Business Rule: Validate email configuration exists
            var emailConfig = await _unitOfWork.EmailConfigurations
                .Query()
                .Where(ec => ec.Id == request.EmailConfigurationId && ec.WorkspaceId == request.WorkspaceId)
                .FirstOrDefaultAsync(cancellationToken);

            if (emailConfig == null)
            {
                throw new InvalidOperationException($"Email configuration with ID '{request.EmailConfigurationId}' not found for workspace '{request.WorkspaceId}'.");
            }

            // ? Business Rule: Validate workspace is active
            var workspace = await _unitOfWork.Workspaces.GetByIdAsync(request.WorkspaceId, cancellationToken);
            if (workspace == null || !workspace.IsActive)
            {
                throw new InvalidOperationException($"Workspace '{request.WorkspaceId}' is not active or does not exist.");
            }

            // ? Create email message for sending
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

            // ? Create email entity for tracking
            var emailToLog = new Email
            {
                WorkspaceId = request.WorkspaceId,
                EmailConfigurationId = request.EmailConfigurationId,
                FromAddress = emailConfig.FromEmail,
                ToAddresses = request.ToEmail,
                CcAddresses = request.CcEmail,
                BccAddresses = request.BccEmail,
                Subject = request.Subject,
                IsHtml = request.IsHtml,
                BodyHtml = request.IsHtml ? TruncateBody(request.Body) : null,
                BodyPlainText = !request.IsHtml ? TruncateBody(request.Body) : null,
                Status = EmailStatus.Queued,
                QueuedAt = DateTimeOffset.UtcNow,
                AttemptCount = 0
            };

            // ? Save email record
            var createdEmail = await _unitOfWork.Emails.AddAsync(emailToLog, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // ? Enqueue background job
            var jobId = _backgroundJobClient.Enqueue(() => SendEmailAndLogAsync(emailMessage, emailConfig, createdEmail.Id, null));

            // ? Update email with job ID
            createdEmail.HangfireJobId = jobId;
            await _unitOfWork.Emails.UpdateAsync(createdEmail, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return new EmailSendingResult
            {
                EmailId = createdEmail.Id,
                JobId = jobId,
                Success = true,
                Message = "Email sending job enqueued successfully."
            };
        }

        /// <summary>
        /// Sends an email and logs the result (executed by Hangfire).
        /// </summary>
        /// <param name="message">The email message.</param>
        /// <param name="config">The email configuration.</param>
        /// <param name="emailId">The email entity ID.</param>
        /// <param name="jobId">The Hangfire job ID.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        [AutomaticRetry(Attempts = 3, DelaysInSeconds = new int[] { 30, 120, 300 }, OnAttemptsExceeded = AttemptsExceededAction.Delete)]
        public async Task SendEmailAndLogAsync(
            EmailMessage message,
            EmailConfiguration config,
            Guid emailId,
            string? jobId)
        {
            // Note: In a real implementation, you'd want to use a service locator pattern
            // or dependency injection to get a fresh UnitOfWork instance in the Hangfire context
            
            var email = await _unitOfWork.Emails.GetByIdAsync(emailId, CancellationToken.None);
            if (email == null)
            {
                return; // Log error in production
            }

            email.AttemptCount++;
            email.SentAt = DateTimeOffset.UtcNow;

            EmailSendResult sendResult;
            try
            {
                sendResult = await _emailSender.SendEmailAsync(message, config);

                email.Status = sendResult.Success ? EmailStatus.Sent : EmailStatus.Failed;
                email.ErrorMessage = sendResult.ErrorMessage;
                email.SmtpResponse = sendResult.SmtpResponse;
            }
            catch (Exception ex)
            {
                email.Status = EmailStatus.Failed;
                email.ErrorMessage = $"Unexpected error during email sending: {ex.Message}";
                email.SmtpResponse = ex.ToString();
            }
            finally
            {
                await _unitOfWork.Emails.UpdateAsync(email, CancellationToken.None);
                await _unitOfWork.SaveChangesAsync(CancellationToken.None);
            }
        }

        /// <summary>
        /// Retries failed emails.
        /// </summary>
        /// <param name="workspaceId">The workspace ID.</param>
        /// <param name="maxRetries">Maximum retry attempts.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Number of emails retried.</returns>
        public async Task<int> RetryFailedEmailsAsync(
            Guid workspaceId,
            int maxRetries = 3,
            CancellationToken cancellationToken = default)
        {
            // ? Business logic for finding retryable emails
            var retryableEmails = await _unitOfWork.Emails
                .Query()
                .Where(e => e.WorkspaceId == workspaceId && 
                           e.Status == EmailStatus.Failed && 
                           e.AttemptCount < maxRetries)
                .Include(e => e.EmailConfiguration)
                .ToListAsync(cancellationToken);

            var retryCount = 0;
            foreach (var email in retryableEmails)
            {
                var emailMessage = new EmailMessage
                {
                    To = email.ToAddresses,
                    Cc = email.CcAddresses,
                    Bcc = email.BccAddresses,
                    Subject = email.Subject,
                    Body = email.IsHtml ? email.BodyHtml : email.BodyPlainText,
                    IsHtml = email.IsHtml
                };

                // Re-queue the email
                var jobId = _backgroundJobClient.Enqueue(() => SendEmailAndLogAsync(emailMessage, email.EmailConfiguration, email.Id, null));
                
                email.HangfireJobId = jobId;
                email.Status = EmailStatus.Queued;
                
                await _unitOfWork.Emails.UpdateAsync(email, cancellationToken);
                retryCount++;
            }

            if (retryCount > 0)
            {
                await _unitOfWork.SaveChangesAsync(cancellationToken);
            }

            return retryCount;
        }

        /// <summary>
        /// Truncates email body for storage efficiency.
        /// </summary>
        /// <param name="body">The email body.</param>
        /// <param name="maxLength">Maximum length.</param>
        /// <returns>Truncated body.</returns>
        private static string? TruncateBody(string? body, int maxLength = 2000)
        {
            if (string.IsNullOrEmpty(body))
                return body;

            return body.Length > maxLength 
                ? string.Concat(body.AsSpan(0, maxLength), "...")
                : body;
        }
    }

    /// <summary>
    /// Request for sending an email.
    /// </summary>
    public class EmailSendRequest
    {
        public Guid WorkspaceId { get; set; }
        public Guid EmailConfigurationId { get; set; }
        public List<string> ToEmail { get; set; } = new();
        public List<string>? CcEmail { get; set; }
        public List<string>? BccEmail { get; set; }
        public string Subject { get; set; } = string.Empty;
        public string Body { get; set; } = string.Empty;
        public bool IsHtml { get; set; }
        public string? FromDisplayName { get; set; }
    }

    /// <summary>
    /// Result of email sending operation.
    /// </summary>
    public class EmailSendingResult
    {
        public Guid EmailId { get; set; }
        public string JobId { get; set; } = string.Empty;
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
    }
}