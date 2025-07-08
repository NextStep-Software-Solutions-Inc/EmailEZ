using EmailEZ.Client.Models;
using Newtonsoft.Json;
using System.Net.Http.Headers;
using System.Text;

namespace EmailEZ.Client.Clients // Adjusted namespace
{
    public class EmailSenderClient : IEmailSenderClient
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;
        private readonly string _baseAddress;
        private const string SendEmailEndpoint = "/api/v1/send-email";

        /// <summary>
        /// Initializes a new instance of the EmailSenderClient.
        /// </summary>
        /// <param name="httpClient">The HttpClient instance (preferably configured via IHttpClientFactory).</param>
        /// <param name="baseAddress">The base URL of your email sender API (e.g., "https://yourapi.com").</param>
        /// <param name="apiKey">The API key for authentication.</param>
        public EmailSenderClient(HttpClient httpClient, string baseAddress, string apiKey)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _baseAddress = baseAddress ?? throw new ArgumentNullException(nameof(baseAddress));
            _apiKey = apiKey ?? throw new ArgumentNullException(nameof(apiKey));

            // Ensure the base address ends with a '/' for correct URI concatenation
            if (!_baseAddress.EndsWith("/"))
            {
                _baseAddress += "/";
            }

            _httpClient.BaseAddress = new Uri(_baseAddress);
            _httpClient.DefaultRequestHeaders.Accept.Clear();
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            _httpClient.DefaultRequestHeaders.Add("X-API-KEY", _apiKey); // Add the API key to the header
        }

        /// <summary>
        /// Sends an email using the configured API.
        /// </summary>
        /// <param name="request">The email send request payload.</param>
        /// <returns>A tuple indicating success and an optional error message.</returns>
        public async Task<(bool Success, string? ErrorMessage)> SendEmailAsync(SendEmailRequest request)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request), "Email send request cannot be null.");
            }

            try
            {
                var jsonContent = JsonConvert.SerializeObject(request);
                var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                // Construct the full URI relative to the base address
                var baseUrl = _httpClient.BaseAddress ?? throw new InvalidOperationException("Base address is not set.");
                var requestUri = new Uri(baseUrl, SendEmailEndpoint);

                using (var response = await _httpClient.PostAsync(requestUri, content))
                {
                    if (response.IsSuccessStatusCode)
                    {
                        return (true, null); // Email sent successfully
                    }
                    else
                    {
                        string errorMessage = $"Failed to send email. Status Code: {response.StatusCode}";
                        var responseContent = await response.Content.ReadAsStringAsync();
                        if (!string.IsNullOrEmpty(responseContent))
                        {
                            errorMessage += $", Details: {responseContent}";
                        }
                        Console.Error.WriteLine(errorMessage); // Log the error (consider a proper logging framework)
                        return (false, errorMessage);
                    }
                }
            }
            catch (HttpRequestException httpEx)
            {
                string errorMessage = $"Network error occurred while sending email: {httpEx.Message}";
                Console.Error.WriteLine(errorMessage);
                return (false, errorMessage);
            }
            catch (JsonException jsonEx)
            {
                string errorMessage = $"Error serializing email request: {jsonEx.Message}";
                Console.Error.WriteLine(errorMessage);
                return (false, errorMessage);
            }
            catch (Exception ex)
            {
                string errorMessage = $"An unexpected error occurred: {ex.Message}";
                Console.Error.WriteLine(errorMessage);
                return (false, errorMessage);
            }
        }
    }
}