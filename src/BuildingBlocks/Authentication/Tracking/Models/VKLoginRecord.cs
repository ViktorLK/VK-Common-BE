using System;

namespace VK.Blocks.Authentication;

/// <summary>
/// Represents details of a user login attempt for auditing and tracking.
/// </summary>
public sealed record VKLoginRecord
{
    /// <summary>
    /// Gets the unique identifier of the user attempting to log in.
    /// </summary>
    public required string UserId { get; init; }

    /// <summary>
    /// Gets the authentication method used (e.g. "JWT", "Cookie", "ApiKey").
    /// </summary>
    public required string AuthenticationMethod { get; init; }

    /// <summary>
    /// Gets the IP address of the client request.
    /// </summary>
    public required string IpAddress { get; init; }

    /// <summary>
    /// Gets the User-Agent header value of the client request.
    /// </summary>
    public required string UserAgent { get; init; }

    /// <summary>
    /// Gets the device fingerprint identifier if provided by the client.
    /// </summary>
    public string? DeviceFingerprint { get; init; }

    /// <summary>
    /// Gets the timestamp when the login attempt occurred.
    /// </summary>
    public DateTimeOffset Timestamp { get; init; }

    /// <summary>
    /// Gets a value indicating whether the login attempt succeeded.
    /// </summary>
    public bool Success { get; init; }

    /// <summary>
    /// Gets the reason for failure if the login attempt was unsuccessful.
    /// </summary>
    public string? FailureReason { get; init; }
}
