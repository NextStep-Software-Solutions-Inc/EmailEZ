using EmailEZ.Application.Services;
using EmailEZ.Domain.Entities;

namespace EmailEZ.Application.Interfaces;

public interface IEmailManagementService
{
    Task<EmailSendingResult> EnqueueEmailAsync(EmailSendRequest request, CancellationToken cancellationToken = default);
    Task<int> RetryFailedEmailsAsync(Guid workspaceId, int maxRetries = 3, CancellationToken cancellationToken = default);
    Task SendEmailAndLogAsync(EmailMessage message, Guid emailConfigurationId, Guid emailId, string? jobId);
}