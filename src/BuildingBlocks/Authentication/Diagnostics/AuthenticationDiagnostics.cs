using System.Collections.Generic;
using System.Diagnostics.Metrics;
using VK.Blocks.Core.Attributes;

namespace VK.Blocks.Authentication.Diagnostics;

/// <summary>
/// Centralized Diagnostics definition for the VK.Blocks.Authentication building block.
/// The Source Generator automatically emits the ActivitySource and Meter fields for this class.
/// </summary>
[VKBlockDiagnostics("VK.Blocks.Authentication")]
public static partial class AuthenticationDiagnostics
{
    // ActivitySource and Meter are generated automatically into a partial class.

    #region Fields

    /// <summary>
    /// Counter tracking the number of authentication attempts.
    /// Includes tags for "auth.type" (e.g., JWT, ApiKey) and "auth.result" (Success, Failure).
    /// </summary>
    public static readonly Counter<long> AuthenticationRequests;

    /// <summary>
    /// Counter tracking the number of API key rate limit violations.
    /// </summary>
    public static readonly Counter<long> RateLimitExceeded;

    #endregion

    #region Constructor

    static AuthenticationDiagnostics()
    {
        AuthenticationRequests = Meter.CreateCounter<long>(
            "authentication.requests",
            description: "Number of authentication requests processed"
        );

        RateLimitExceeded = Meter.CreateCounter<long>(
            "authentication.rate_limit_exceeded",
            description: "Number of times API key rate limits were exceeded"
        );
    }

    #endregion

    #region Public Methods

    /// <summary>
    /// Records an authentication attempt result.
    /// </summary>
    /// <param name="authType">The type of authentication (e.g., "jwt", "apikey").</param>
    /// <param name="isSuccess">Whether the authentication succeeded.</param>
    public static void RecordAuthAttempt(string authType, bool isSuccess)
    {
        AuthenticationRequests.Add(1, new KeyValuePair<string, object?>("auth.type", authType),
                                      new KeyValuePair<string, object?>("auth.result", isSuccess ? "Success" : "Failure"));
    }

    /// <summary>
    /// Records a rate limit exceeded event.
    /// </summary>
    public static void RecordRateLimitExceeded()
    {
        RateLimitExceeded.Add(1);
    }

    #endregion
}
