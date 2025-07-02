namespace EmailEZ.Client.Interfaces;

public interface IEmailSenderClient
{
    Task<bool> SendEmailAsync(EmailRequest emailRequest);
}
