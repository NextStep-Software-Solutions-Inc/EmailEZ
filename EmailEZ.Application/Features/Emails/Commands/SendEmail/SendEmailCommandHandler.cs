using EmailEZ.Application.Interfaces;
using EmailEZ.Application.Services;
using MediatR;

namespace EmailEZ.Application.Features.Emails.Commands.SendEmail
{
    public class SendEmailCommandHandler : IRequestHandler<SendEmailCommand, SendEmailResponse>
    {
        private readonly EmailManagementService _emailManagementService;

        public SendEmailCommandHandler(EmailManagementService emailManagementService)
        {
            _emailManagementService = emailManagementService;
        }

        public async Task<SendEmailResponse> Handle(SendEmailCommand request, CancellationToken cancellationToken)
        {
            try
            {
                // ✅ Handler only coordinates - delegates complex business logic to service
                var emailRequest = new EmailSendRequest
                {
                    WorkspaceId = request.WorkspaceId,
                    EmailConfigurationId = request.EmailConfigurationId,
                    ToEmail = request.ToEmail,
                    CcEmail = request.CcEmail,
                    BccEmail = request.BccEmail,
                    Subject = request.Subject,
                    Body = request.Body,
                    IsHtml = request.IsHtml,
                    FromDisplayName = request.FromDisplayName
                };

                // ✅ Delegate the complex workflow to the application service
                var result = await _emailManagementService.EnqueueEmailAsync(emailRequest, cancellationToken);

                // ✅ Handler only handles the response mapping
                return new SendEmailResponse(result.Success, result.Message, result.JobId);
            }
            catch (InvalidOperationException ex)
            {
                // ✅ Handler handles application-specific exceptions
                return new SendEmailResponse(false, ex.Message);
            }
        }
    }
}