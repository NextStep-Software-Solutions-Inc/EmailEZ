namespace EmailEZ.Client.Models;

/// <summary>
/// Configuration options for the EmailSenderClient.
/// </summary>
public class EmailSenderClientOptions
{
    /// <summary>
    /// Gets or sets the base URI of the EmailEZ API (e.g., "https://yourapi.com").
    /// </summary>
    public string? BaseUri { get; set; }

    /// <summary>
    /// Gets or sets the API key to be sent in the header for authentication.
    /// </summary>
    public string? ApiKey { get; set; }
}