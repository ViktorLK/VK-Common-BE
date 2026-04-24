using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Logging;

namespace VK.Blocks.Authentication.ApiKeys.Internal;

/// <summary>
/// Source-generated logger for API Key related events.
/// </summary>
[ExcludeFromCodeCoverage(Justification = "Generated boilerplate by LoggerMessage source generator")]
internal static partial class ApiKeyLog
{
    /// <summary>
    /// Logs that an API key is too short based on the minimum length requirement.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    /// <param name="minLength">The minimum required length.</param>
    [LoggerMessage(
        EventId = 2301,
        Level = LogLevel.Warning,
        Message = "API key is too short. MinLength: {MinLength}")]
    public static partial void LogApiKeyTooShort(this ILogger logger, int minLength);

    /// <summary>
    /// Logs that an API key was not found in the store.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    /// <param name="hashPrefix">The prefix of the hashed API key for identification.</param>
    [LoggerMessage(
        EventId = 2302,
        Level = LogLevel.Warning,
        Message = "API key not found. Hash starting with: {HashPrefix}****")]
    public static partial void LogApiKeyNotFound(this ILogger logger, string hashPrefix);

    /// <summary>
    /// Logs that a revoked API key was used.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    /// <param name="keyId">The unique identifier of the API key.</param>
    [LoggerMessage(
        EventId = 2311,
        Level = LogLevel.Warning,
        Message = "Revoked API key used. KeyId: {KeyId}")]
    public static partial void LogRevokedApiKeyUsed(this ILogger logger, string keyId);

    /// <summary>
    /// Logs that an expired API key was used.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    /// <param name="keyId">The unique identifier of the API key.</param>
    [LoggerMessage(
        EventId = 2312,
        Level = LogLevel.Warning,
        Message = "Expired API key used. KeyId: {KeyId}")]
    public static partial void LogExpiredApiKeyUsed(this ILogger logger, string keyId);

    /// <summary>
    /// Logs that a disabled API key was used.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    /// <param name="keyId">The unique identifier of the API key.</param>
    [LoggerMessage(
        EventId = 2313,
        Level = LogLevel.Warning,
        Message = "Disabled API key used. KeyId: {KeyId}")]
    public static partial void LogDisabledApiKeyUsed(this ILogger logger, string keyId);

    /// <summary>
    /// Logs that an API key has exceeded its rate limit.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    /// <param name="keyId">The unique identifier of the API key.</param>
    [LoggerMessage(
        EventId = 2321,
        Level = LogLevel.Warning,
        Message = "API key rate limit exceeded. KeyId: {KeyId}")]
    public static partial void LogRateLimitExceeded(this ILogger logger, string keyId);

    /// <summary>
    /// Logs a failure to update the last used timestamp for an API key.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    /// <param name="ex">The exception that occurred.</param>
    /// <param name="keyId">The unique identifier of the API key.</param>
    [LoggerMessage(
        EventId = 2391,
        Level = LogLevel.Warning,
        Message = "Failed to update LastUsedAt for KeyId: {KeyId}")]
    public static partial void LogLastUsedUpdateFailed(this ILogger logger, Exception ex, string keyId);
}
