using System;
using Microsoft.Extensions.Logging;

namespace VK.Blocks.Authentication.Tracking.Diagnostics.Internal;

/// <summary>
/// Source-generated logging extensions for the Tracking feature.
/// </summary>
internal static partial class TrackingLog
{
    [LoggerMessage(
        EventId = 3001,
        Level = LogLevel.Information,
        Message = "Audit: User {UserId} logged in successfully via {AuthMethod}. IP: {Ip}, UA: {UserAgent}, Fingerprint: {Fingerprint}")]
    public static partial void LogLoginSuccess(
        this ILogger logger,
        string userId,
        string authMethod,
        string ip,
        string userAgent,
        string fingerprint);

    [LoggerMessage(
        EventId = 3002,
        Level = LogLevel.Warning,
        Message = "Audit: User {UserId} login failed via {AuthMethod}. Reason: {Reason}. IP: {Ip}, UA: {UserAgent}")]
    public static partial void LogLoginFailure(
        this ILogger logger,
        string userId,
        string authMethod,
        string reason,
        string ip,
        string userAgent);

    [LoggerMessage(
        EventId = 3003,
        Level = LogLevel.Warning,
        Message = "Missing client device fingerprint header '{Header}' for user '{UserId}'")]
    public static partial void LogMissingFingerprint(
        this ILogger logger,
        string header,
        string userId);
}
