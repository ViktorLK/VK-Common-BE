namespace VK.Blocks.Core;

/// <summary>
/// Defines the types of errors that can occur.
/// </summary>
public enum VKErrorType
{
    /// <summary>Represents no error.</summary>
    None = -1,

    /// <summary>A general failure.</summary>
    Failure = 0,

    // --- 4xx Client / Validation / Domain Errors ---

    /// <summary>A validation error.</summary>
    Validation = 1,

    /// <summary>A not found error.</summary>
    NotFound = 2,

    /// <summary>A conflict error.</summary>
    Conflict = 3,

    /// <summary>An unauthorized error.</summary>
    Unauthorized = 4,

    /// <summary>A forbidden error.</summary>
    Forbidden = 5,

    /// <summary>Too many requests (Rate limiting).</summary>
    TooManyRequests = 6,

    /// <summary>A precondition failed error.</summary>
    PreconditionFailed = 7,

    // --- 5xx Infrastructure / External Errors ---

    /// <summary>The service is temporarily unavailable.</summary>
    ServiceUnavailable = 8,

    /// <summary>A timeout occurred.</summary>
    Timeout = 9,

    /// <summary>An external service/gateway error.</summary>
    ExternalError = 10
}
