namespace EmailEZ.Client.Exceptions;

/// <summary>
/// Represents an exception that occurs during interaction with the EmailEZ API.
/// </summary>
public class EmailEZClientException : Exception
{
    public int? StatusCode { get; }
    public string ErrorResponse { get; }

    public EmailEZClientException() { }

    public EmailEZClientException(string message) : base(message) { }

    public EmailEZClientException(string message, Exception innerException) : base(message, innerException) { }

    public EmailEZClientException(string message, int? statusCode, string errorResponse = null, Exception innerException = null)
        : base(message, innerException)
    {
        StatusCode = statusCode;
        ErrorResponse = errorResponse;
    }
}
