namespace VK.Blocks.Core.Results;

/// <summary>
/// Defines the types of errors that can occur.
/// </summary>
public enum ErrorType
{
    /// <summary>Represents no error.</summary>
    None = -1,

    /// <summary>A validation error.</summary>
    Validation = 1,

    /// <summary>An unauthorized error.</summary>
    Unauthorized = 4,

    /// <summary>A forbidden error.</summary>
    Forbidden = 5,

    /// <summary>A not found error.</summary>
    NotFound = 2,

    /// <summary>A conflict error.</summary>
    Conflict = 3,

    /// <summary>A precondition failed error.</summary>
    PreconditionFailed = 10,

    /// <summary>Too many requests (Rate limiting).</summary>
    TooManyRequests = 6,

    /// <summary>A general failure.</summary>
    Failure = 0,

    /// <summary>An external service/gateway error.</summary>
    ExternalError = 9,

    /// <summary>The service is temporarily unavailable.</summary>
    ServiceUnavailable = 7,

    /// <summary>A timeout occurred.</summary>
    Timeout = 8
}
