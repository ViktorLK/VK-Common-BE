namespace VK.Blocks.ExceptionHandling.Options;

/// <summary>
/// Configuration options for exception handling.
/// </summary>
public sealed record ExceptionHandlingOptions
{
    /// <summary>
    /// Gets or sets a value indicating whether to expose the stack trace in the error response.
    /// Should be set to <c>false</c> in production environments.
    /// </summary>
    public bool ExposeStackTrace { get; set; } = false;
}
