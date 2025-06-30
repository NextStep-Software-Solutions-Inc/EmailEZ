using Microsoft.AspNetCore.Authentication;

namespace EmailEZ.Infrastructure.Authentication;

public class ApiKeyAuthenticationOptions : AuthenticationSchemeOptions
{
    public const string DefaultScheme = "ApiKey";
    public string HeaderName { get; set; } = "X-API-KEY"; // The header where the API key is expected
}