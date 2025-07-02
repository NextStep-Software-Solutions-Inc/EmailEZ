using System.Net.Http.Headers;
using System.Net.Http.Json; // For PostAsJsonAsync extension method
using EmailEZ.Client.Exceptions;
using EmailEZ.Client.Interfaces;
using EmailEZ.Client.Models;
using Microsoft.Extensions.Logging; // For logging abstraction

namespace EmailEZ.Client
{
   
    /// <summary>
    /// A client for sending emails via the EmailEZ API.
    /// Designed for dependency injection with IHttpClientFactory.
    /// </summary>
    public class EmailSenderClient : IEmailSenderClient // Implementing an interface for better testability
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<EmailSenderClient> _logger;
        private readonly EmailSenderClientOptions _options; // To hold base URI and API Key

        /// <summary>
        /// Initializes a new instance of the <see cref="EmailSenderClient"/> class.
        /// Use this constructor with dependency injection (e.g., IHttpClientFactory).
        /// </summary>
        /// <param name="httpClient">The HttpClient instance, typically injected via IHttpClientFactory.</param>
        /// <param name="logger">The logger instance for this client.</param>
        /// <param name="options">Configuration options for the client, including base URI and API key.</param>
        /// <exception cref="ArgumentNullException">Thrown if any required parameters are null.</exception>
        public EmailSenderClient(HttpClient httpClient, ILogger<EmailSenderClient> logger, EmailSenderClientOptions options)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _options = options ?? throw new ArgumentNullException(nameof(options));

            if (string.IsNullOrWhiteSpace(_options.BaseUri))
                throw new ArgumentNullException(nameof(options.BaseUri), "Base URI cannot be null or empty in options.");
            if (string.IsNullOrWhiteSpace(_options.ApiKey))
                throw new ArgumentNullException(nameof(options.ApiKey), "API Key cannot be null or empty in options.");

            // Configure HttpClient default headers here, as HttpClientFactory provides it pre-configured
            // However, adding headers for a specific service is still often done here.
            _httpClient.DefaultRequestHeaders.Add("apiKey", _options.ApiKey);
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            _logger.LogDebug("EmailSenderClient initialized with BaseUri: {BaseUri}", _options.BaseUri);
        }

        /// <summary>
        /// Sends an email using the EmailEZ API.
        /// </summary>
        /// <param name="emailRequest">The email request payload containing all email details.</param>
        /// <returns>A <see cref="Task{TResult}"/> that represents the asynchronous operation, containing true if the email was sent successfully.</returns>
        /// <exception cref="ArgumentNullException">Thrown if the emailRequest is null.</exception>
        /// <exception cref="EmailEZClientException">Thrown if the API call fails, containing details about the error.</exception>
        public async Task<bool> SendEmailAsync(EmailRequest emailRequest)
        {
            _logger.LogInformation("Attempting to send email with subject: '{Subject}' to {ToEmails}",
                emailRequest.Subject, string.Join(", ", emailRequest.ToEmail ?? new List<string>()));

            // Basic input validation
            
            if (emailRequest == null)
            {
                _logger.LogError("Email request object is null.");
                throw new ArgumentNullException(nameof(emailRequest), "Email request cannot be null.");
            }

            if (emailRequest.ToEmail == null || emailRequest.ToEmail.Count == 0) // Requires System.Linq, add it if not already present
            {
                _logger.LogError("Email request 'ToEmail' list is null or empty.");
                throw new ArgumentException("ToEmail list cannot be null or empty.", nameof(emailRequest.ToEmail));
            }
                
            if (emailRequest.ToEmail.Count == 0) // Requires System.Linq, add it if not already present
            {
                _logger.LogError("Email request 'ToEmail' list is empty.");
                throw new ArgumentException("ToEmail list cannot be empty.", nameof(emailRequest.ToEmail));
            }
            if (string.IsNullOrWhiteSpace(emailRequest.Subject))
            {
                _logger.LogWarning("Email subject is empty.");
                // Depending on API requirements, this might be an error or just a warning.
            }

            var requestUri = $"{_options.BaseUri.TrimEnd('/')}/api/v1/send-email";
            _logger.LogDebug("Sending email to API endpoint: {RequestUri}", requestUri);
            string? responseContent = null;

            try
            {
                HttpResponseMessage? response = await _httpClient.PostAsJsonAsync(requestUri, emailRequest);
                _logger.LogDebug("Received API response with status code: {StatusCode}", (int)response.StatusCode);

                if (!response.IsSuccessStatusCode)
                {
                    responseContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError("Email send failed. Status: {StatusCode}. Response: {ResponseContent}",
                        (int)response.StatusCode, responseContent);

                    // Throw custom exception with more details
                    throw new EmailEZClientException(
                        $"EmailEZ API call failed with status code {response.StatusCode}.",
                        (int)response.StatusCode,
                        responseContent);
                }

                _logger.LogInformation("Email with subject '{Subject}' successfully sent.", emailRequest.Subject);
                return true;
            }
            catch (HttpRequestException httpEx)
            {
                // This catches network errors, DNS issues, connection refused, etc.
                _logger.LogError(httpEx, "Network or HTTP error while calling EmailEZ API: {Message}", httpEx.Message);
                throw new EmailEZClientException($"Network or HTTP error: {httpEx.Message}", httpEx.StatusCode.HasValue ? (int)httpEx.StatusCode.Value : (int?)null, null, httpEx);
            }
            catch (EmailEZClientException)
            {
                // Re-throw our specific exception to prevent it from being caught by generic Exception
                throw;
            }
            catch (Exception ex)
            {
                // Catch any other unexpected errors
                _logger.LogError(ex, "An unexpected error occurred during email sending: {Message}", ex.Message);
                throw new EmailEZClientException($"An unexpected error occurred: {ex.Message}", null, null, ex);
            }
        }
    }
}