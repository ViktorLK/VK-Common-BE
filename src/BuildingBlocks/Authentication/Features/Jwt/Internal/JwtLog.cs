using System;
using Microsoft.Extensions.Logging;

namespace VK.Blocks.Authentication.Features.Jwt.Internal;

/// <summary>
/// Source-generated logger for JWT related events.
/// </summary>
internal static partial class JwtLog
{
    /// <summary>
    /// Logs a message indicating that JWT validation options are not configured properly.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    [LoggerMessage(
        EventId = 2101,
        Level = LogLevel.Error,
        Message = "JWT Validation options are not configured properly.")]
    public static partial void LogOptionsNotConfigured(this ILogger logger);

    /// <summary>
    /// Logs a message indicating that authentication failed for a token.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    /// <param name="ex">The exception that caused the failure, if any.</param>
    /// <param name="errorCode">The failure reason code.</param>
    [LoggerMessage(
        EventId = 2102,
        Level = LogLevel.Warning,
        Message = "Failed to authenticate token. Reason: {ErrorCode}")]
    public static partial void LogAuthenticationFailed(this ILogger logger, Exception? ex, string errorCode);

    /// <summary>
    /// Logs a message indicating an unexpected error during JWT authentication.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    /// <param name="ex">The exception that occurred.</param>
    [LoggerMessage(
        EventId = 2103,
        Level = LogLevel.Warning,
        Message = "Unexpected error occurring during JWT authentication.")]
    public static partial void LogUnexpectedAuthenticationError(this ILogger logger, Exception ex);

    /// <summary>
    /// Logs a message indicating an invalid refresh token validation request.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    [LoggerMessage(
        EventId = 2111,
        Level = LogLevel.Warning,
        Message = "Invalid refresh token validation request: JTI or FamilyId is missing.")]
    public static partial void LogInvalidRefreshTokenRequest(this ILogger logger);

    /// <summary>
    /// Logs a message indicating a potential refresh token replay attack.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    /// <param name="familyId">The token family identifier.</param>
    /// <param name="jti">The JWT identifier.</param>
    [LoggerMessage(
        EventId = 2112,
        Level = LogLevel.Warning,
        Message = "Potential refresh token replay attack detected! FamilyId: {FamilyId}, JTI: {Jti}. The token has been reused.")]
    public static partial void LogRefreshTokenReplayDetected(this ILogger logger, string familyId, string jti);
}
