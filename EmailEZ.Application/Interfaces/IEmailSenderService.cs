using EmailEZ.Domain.Entities;

namespace EmailEZ.Application.Interfaces;

public interface IEmailSenderService
{
    Task SendEmailAsync(Email email, CancellationToken cancellationToken);
}