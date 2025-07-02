namespace EmailEZ.Client.Exceptions;

/// <summary>
/// Represents an exception that occurs during interaction with the EmailEZ API.
/// </summary>
public class EmailEZClientException : Exception
{
    /// <summary>
    /// Gets the HTTP status code returned by the API, if available.
    /// </summary>
    public int? StatusCode { get; }

    /// <summary>
    /// Gets the raw error response content from the API, if available.
    /// </summary>
    public string? ErrorResponse { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="EmailEZClientException"/> class.
    /// </summary>
    public EmailEZClientException() { }

    /// <summary>
    /// Initializes a new instance of the <see cref="EmailEZClientException"/> class with a specified error message.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    public EmailEZClientException(string message) : base(message) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="EmailEZClientException"/> class with a specified error message and a reference to the inner exception that is the cause of this exception.
    /// </summary>
    /// <param name="message">The error message that explains the reason for the exception.</param>
    /// <param name="innerException">The exception that is the cause of the current exception, or a null reference if no inner exception is specified.</param>
    public EmailEZClientException(string message, Exception innerException) : base(message, innerException) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="EmailEZClientException"/> class with a specified error message, HTTP status code, and raw error response.
    /// </summary>
    /// <param name="message">The error message that explains the reason for the exception.</param>
    /// <param name="statusCode">The HTTP status code returned by the API.</param>
    /// <param name="errorResponse">The raw error response content from the API.</param>
    /// <param name="innerException">The exception that is the cause of the current exception, or a null reference if no inner exception is specified.</param>
    public EmailEZClientException(string message, int? statusCode, string? errorResponse = null, Exception? innerException = null)
        : base(message, innerException)
    {
        StatusCode = statusCode;
        ErrorResponse = errorResponse;
    }
}