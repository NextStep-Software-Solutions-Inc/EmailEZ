using EmailEZ.Client.Models;

namespace EmailEZ.Client.Clients
{
    public interface IEmailSenderClient
    {
        Task<(bool Success, string ErrorMessage)> SendEmailAsync(SendEmailRequest request);
    }
}