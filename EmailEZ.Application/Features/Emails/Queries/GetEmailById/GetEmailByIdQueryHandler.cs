using EmailEZ.Application.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EmailEZ.Application.Features.Emails.Queries.GetEmailById;

public class GetEmailByIdQueryHandler : IRequestHandler<GetEmailByIdQuery, EmailDetailsDto?>
{
    private readonly IApplicationDbContext _context;

    public GetEmailByIdQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<EmailDetailsDto?> Handle(GetEmailByIdQuery request, CancellationToken cancellationToken)
    {
        var email = await _context.Emails
            .AsNoTracking() // For read-only queries, AsNoTracking improves performance
            .FirstOrDefaultAsync(e => e.Id == request.EmailId && e.TenantId == request.TenantId, cancellationToken);

        if (email == null)
        {
            return null; // Email not found for the given ID and TenantId
        }

        // Project the Email entity to EmailDetailsDto
        var emailDetailsDto = new EmailDetailsDto
        {
            Id = email.Id,
            TenantId = email.TenantId,
            EmailConfigurationId = email.EmailConfigurationId,
            FromAddress = email.FromAddress,
            ToAddresses = email.ToAddresses,
            CcAddresses = email.CcAddresses,
            BccAddresses = email.BccAddresses,
            Subject = email.Subject,
            BodyHtml = email.BodyHtml,
            BodyPlainText = email.BodyPlainText,
            Status = email.Status,
            ErrorMessage = email.ErrorMessage,
            SmtpResponse = email.SmtpResponse,
            HangfireJobId = email.HangfireJobId,
            QueuedAt = email.QueuedAt,
            SentAt = email.SentAt,
            AttemptCount = email.AttemptCount
        };

        return emailDetailsDto;
    }
}