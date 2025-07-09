using EmailEZ.Client.Clients;
using EmailEZ.Client.Models;
using Microsoft.AspNetCore.Mvc;

namespace EmailEZ.Client.TestWebApp.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastController : ControllerBase
    {
        private readonly IEmailSenderClient _emailSenderClient;
        private static readonly string[] Summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        private readonly ILogger<WeatherForecastController> _logger;

        public WeatherForecastController(IEmailSenderClient emailSenderClient, ILogger<WeatherForecastController> logger)
        {
            _emailSenderClient = emailSenderClient;
            _logger = logger;
        }

        [HttpGet(Name = "GetWeatherForecast")]
        public IEnumerable<WeatherForecast> Get()
        {
            return Enumerable.Range(1, 5).Select(index => new WeatherForecast
            {
                Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                TemperatureC = Random.Shared.Next(-20, 55),
                Summary = Summaries[Random.Shared.Next(Summaries.Length)]
            })
            .ToArray();
        }

        [HttpPost("send-test")] // API path will be /email/send-test
        public async Task<IActionResult> SendTestEmail([FromBody] TestEmailRequest request)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid model state for SendTestEmail: {ModelState}", ModelState);
                return BadRequest(ModelState);
            }

            // Create the SendEmailRequest object for the EmailEZ.Client
            var sendEmailRequest = new SendEmailRequest
            {
                // NOTE: Replace these with actual WorkspaceId and EmailConfigurationId as needed by your API
                EmailConfigurationId = Guid.Parse("81228be6-73b7-4c0f-835b-c198c734a955"),
                ToEmail = new List<string> { request.ToEmail },
                Subject = request.Subject,
                Body = request.Body,
                IsHtml = request.IsHtml,
                FromDisplayName = request.FromDisplayName,
                // These can be null or empty lists if not used
                CcEmail = request.CcEmail ?? new List<string>(),
                BccEmail = request.BccEmail ?? new List<string>()
            };

            _logger.LogInformation("Attempting to send email to: {ToEmail} with Subject: {Subject}", request.ToEmail, request.Subject);

            // Call the client's SendEmailAsync method
           try
            {
                var (success, errorMessage) = await _emailSenderClient.SendEmailAsync(sendEmailRequest);

                if (success)
                {
                    _logger.LogInformation("Email sent successfully to: {ToEmail}", request.ToEmail);
                    return Ok(new { Message = "Email sent successfully!" });
                }
                else
                {
                    _logger.LogError("Failed to send email to {ToEmail}. Error: {ErrorMessage}", request.ToEmail, errorMessage);
                    return StatusCode(500, new { Message = "Failed to send email.", Error = errorMessage });
                }
            } catch (Exception ex)
            {
                _logger.LogError($"{ex.Message}");
                return StatusCode(500, new {
                    Message = "An unexpected error occurred while sending the email.",
                    Error = ex.Message
                });
            }
        }
    }
}


public class TestEmailRequest
{
    public string ToEmail { get; set; }
    public string Subject { get; set; }
    public string Body { get; set; }
    public bool IsHtml { get; set; } = true; // Default to HTML
    public string FromDisplayName { get; set; } = "My App Name";
    public List<string> CcEmail { get; set; }
    public List<string> BccEmail { get; set; }
}