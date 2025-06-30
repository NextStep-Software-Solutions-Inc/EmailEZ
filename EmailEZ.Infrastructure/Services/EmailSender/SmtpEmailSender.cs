using EmailEZ.Application.Interfaces;
using EmailEZ.Domain.Entities; // For EmailConfiguration
using MailKit.Net.Smtp;
using MailKit.Security; // For SecureSocketOptions
using Microsoft.Extensions.Logging; // For logging
using MimeKit; 
using MimeKit.Text;


namespace EmailEZ.Infrastructure.Services;

public class SmtpEmailSender : IEmailSender
{
    private readonly IEncryptionService _encryptionService;
    private readonly ILogger<SmtpEmailSender> _logger;

    public SmtpEmailSender(IEncryptionService encryptionService, ILogger<SmtpEmailSender> logger)
    {
        _encryptionService = encryptionService;
        _logger = logger;
    }

    public async Task<EmailSendResult> SendEmailAsync(EmailMessage message, EmailConfiguration config)
    {
        string decryptedPassword;
        try
        {
            decryptedPassword = _encryptionService.Decrypt(config.Password);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to decrypt password for email configuration '{ConfigId}' for tenant '{TenantId}'. Email not sent.", config.Id, config.TenantId);
            return new EmailSendResult
            {
                Success = false,
                ErrorMessage = "Failed to decrypt email configuration password."
            };
        }

        var email = new MimeMessage();
        email.From.Add(new MailboxAddress(message.FromDisplayName ?? config.DisplayName, config.FromEmail));
        email.To.AddRange(message.To.Select(MailboxAddress.Parse));
        email.Cc.AddRange(message.Cc?.Select(MailboxAddress.Parse) ?? Enumerable.Empty<MailboxAddress>());
        email.Bcc.AddRange(message.Bcc?.Select(MailboxAddress.Parse) ?? Enumerable.Empty<MailboxAddress>());
        email.Subject = message.Subject;
        email.Body = message.IsHtml ? new TextPart(TextFormat.Html) { Text = message.Body } : new TextPart(TextFormat.Plain) { Text = message.Body };

        using var client = new SmtpClient();
        try
        {
            _logger.LogInformation("Attempting to connect to SMTP server: {Host}:{Port} with SSL: {UseSsl}", config.SmtpHost, config.SmtpPort, config.UseSsl);
            var secureSocketOption = config.UseSsl ? SecureSocketOptions.Auto : SecureSocketOptions.None;
            await client.ConnectAsync(config.SmtpHost, config.SmtpPort, secureSocketOption);

            _logger.LogInformation("Authenticating with username: {Username}", config.Username);
            await client.AuthenticateAsync(config.Username, decryptedPassword);

            _logger.LogInformation("Sending email from '{From}' to '{To}'...", config.Username, message.To);
            await client.SendAsync(email);
            _logger.LogInformation("Email sent successfully from '{From}' to '{To}'.", config.Username, message.To);

            return new EmailSendResult
            {
                Success = true,
                SmtpResponse = "Email sent successfully."
            };
        }
        catch (AuthenticationException authEx)
        {
            _logger.LogError(authEx, "Authentication error sending email from '{From}' to '{To}': {Message}", config.Username, message.To, authEx.Message);
            return new EmailSendResult
            {
                Success = false,
                ErrorMessage = $"Authentication error with SMTP server: {authEx.Message}"
            };
        }
        catch (SmtpCommandException cmdEx)
        {
            _logger.LogError(cmdEx, "SMTP command error sending email from '{From}' to '{To}'. Error Code: {ErrorCode}, StatusCode: {StatusCode}", config.Username, message.To, cmdEx.ErrorCode, cmdEx.StatusCode);
            return new EmailSendResult
            {
                Success = false,
                ErrorMessage = $"SMTP command error: {cmdEx.Message}"
            };
        }
        catch (SmtpProtocolException protoEx)
        {
            _logger.LogError(protoEx, "SMTP protocol error sending email from '{From}' to '{To}': {Message}", config.Username, message.To, protoEx.Message);
            return new EmailSendResult
            {
                Success = false,
                ErrorMessage = $"SMTP protocol error: {protoEx.Message}"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "General error sending email from '{From}' to '{To}' using config '{ConfigId}'.", config.Username, message.To, config.Id);
            return new EmailSendResult
            {
                Success = false,
                ErrorMessage = $"Error sending email: {ex.Message}"
            };
        }
        finally
        {
            if (client.IsConnected)
            {
                await client.DisconnectAsync(true);
            }
        }
    }

    
}