
# 📧 EmailEZ.Client

## 🚀 Overview

**EmailEZ.Client** is a high-performance, robust, and easy-to-use C# client library designed to simplify integration with the **EmailEZ API**. It abstracts the complexity of HTTP communication and JSON serialization, allowing .NET applications to send emails effortlessly using modern .NET best practices, including `IHttpClientFactory`.

### 🔑 Key Features

- **Simplified Email Sending** – Clear and intuitive API for constructing and sending email requests.
- **Comprehensive Payload Support** – Supports all EmailEZ payload fields: `To`, `CC`, `BCC`, `Subject`, `Body`, `FromDisplayName`, `TenantId`, `EmailConfigurationId`, etc.
- **API Key Authentication** – Secure communication with automatic inclusion of API keys in headers.
- **Robust Error Handling** – Returns a `(bool Success, string ErrorMessage)` tuple to simplify error diagnostics.
- **Asynchronous Operations** – Fully async for scalability and responsiveness.
- **Dependency Injection Ready** – Optimized for ASP.NET Core DI using `IHttpClientFactory`.

---

## 📦 Installation

### Via .NET CLI

```bash
dotnet add package EmailEZ.Client
```

### Via NuGet Package Manager Console

```powershell
Install-Package EmailEZ.Client
```

### Via Visual Studio

1. Right-click your project → `Manage NuGet Packages`.
2. Go to the `Browse` tab.
3. Search for `EmailEZ.Client`.
4. Click `Install`.

---

## ⚙️ Configuration

### `appsettings.json` Example

```json
{
  "EmailApi": {
    "BaseUrl": "https://api.your-email-domain.com", 
    "ApiKey": "YOUR_SECRET_API_KEY_HERE"
  }
}
```

> 🔐 **Security Note**: Never store `ApiKey` directly in source-controlled files. Use:
- Environment Variables
- Azure Key Vault (or similar)
- .NET User Secrets (development only)

---

## 🧩 Dependency Injection Setup

Add the following to your `Program.cs` (.NET 6+):

```csharp
using EmailEZ.Client.Clients;
using Microsoft.Extensions.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

var emailApiBaseUrl = builder.Configuration["EmailApi:BaseUrl"];
var emailApiKey = builder.Configuration["EmailApi:ApiKey"];

if (string.IsNullOrEmpty(emailApiBaseUrl) || string.IsNullOrEmpty(emailApiKey))
{
    Console.WriteLine("CRITICAL: Missing EmailApi configuration.");
}

builder.Services.AddHttpClient("EmailSenderClient", client =>
{
    client.Timeout = TimeSpan.FromSeconds(30);
});

builder.Services.AddTransient<IEmailSenderClient>(sp =>
{
    var client = sp.GetRequiredService<IHttpClientFactory>().CreateClient("EmailSenderClient");
    return new EmailSenderClient(client, emailApiBaseUrl, emailApiKey);
});

var app = builder.Build();
app.MapControllers();
app.Run();
```

---

## 🚀 Usage

### Inject & Use in a Controller

```csharp
[ApiController]
[Route("[controller]")]
public class EmailController : ControllerBase
{
    private readonly IEmailSenderClient _client;
    private readonly ILogger<EmailController> _logger;

    public EmailController(IEmailSenderClient client, ILogger<EmailController> logger)
    {
        _client = client;
        _logger = logger;
    }

    [HttpPost("send")]
    public async Task<IActionResult> SendEmail([FromBody] EmailSendRequestDto dto)
    {
        var request = new SendEmailRequest
        {
            TenantId = Guid.Parse("3fa85f64-5717-4562-b3fc-2c963f66afa6"), // TODO: Replace
            EmailConfigurationId = Guid.Parse("3fa85f64-5717-4562-b3fc-2c963f66afa6"), // TODO: Replace
            ToEmail = new List<string> { dto.ToEmail },
            Subject = dto.Subject,
            Body = dto.Body,
            IsHtml = dto.IsHtml,
            FromDisplayName = dto.FromDisplayName,
            CcEmail = dto.CcEmail ?? new(),
            BccEmail = dto.BccEmail ?? new()
        };

        var (success, error) = await _client.SendEmailAsync(request);

        if (success)
        {
            return Ok(new { Message = "Email sent successfully." });
        }

        _logger.LogError("Send failed: {Error}", error);
        return StatusCode(500, new { Message = "Failed to send email.", Error = error });
    }
}

public class EmailSendRequestDto
{
    [Required] public string ToEmail { get; set; }
    [Required] public string Subject { get; set; }
    [Required] public string Body { get; set; }
    public bool IsHtml { get; set; } = true;
    public string FromDisplayName { get; set; } = "Your Application";
    public List<string> CcEmail { get; set; }
    public List<string> BccEmail { get; set; }
}
```

---

## ⚠️ Error Handling

All responses from `SendEmailAsync` return a `(bool Success, string ErrorMessage)` tuple:

- `Success = true`: Email was sent.
- `Success = false`: Check `ErrorMessage` for:
  - HTTP errors (e.g., `BadRequest`)
  - API-specific validation errors
  - Network or serialization issues

Always validate and log these results.

---

## 🧪 Testing (Unit Tests)

We recommend using:

- **xUnit / NUnit** for unit testing
- **Moq** for mocking `HttpMessageHandler`

---

## 🤝 Contributing

1. Fork the repository
2. Create a feature/bugfix branch
3. Commit and push your changes
4. Open a Pull Request
5. Ensure tests are included and passing

---

## 📄 License

MIT License. See `LICENSE` file.

---

## 📧 Contact

For questions, issues, or feedback:

- Open an issue
- Email: `your.email@example.com`

Thanks for using **EmailEZ.Client**!
