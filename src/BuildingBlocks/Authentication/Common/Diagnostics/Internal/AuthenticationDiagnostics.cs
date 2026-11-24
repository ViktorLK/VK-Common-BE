using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.DependencyInjection;
using VK.Blocks.Authentication.Generated;
using VK.Blocks.Core;

namespace VK.Blocks.Authentication.Common.Diagnostics.Internal;

/// <summary>
/// Centralized Diagnostics definition for the VK.Blocks.Authentication building block.
/// The Source Generator automatically emits the ActivitySource and Meter fields for this class.
/// </summary>
[VKBlockDiagnostics<VKAuthenticationBlock>]
internal static partial class AuthenticationDiagnostics
{
    // ActivitySource and Meter are generated automatically into a partial class.

    private static readonly Counter<long> _authenticationRequests;

    private static readonly Counter<long> _tooManyRequests;

    private static readonly Counter<long> _revocations;

    private static readonly Counter<long> _replayAttacks;

    private static readonly Counter<long> _claimsTransformations;

    /// <summary>
    /// Histogram tracking the duration of claims transformation.
    /// </summary>
    private static readonly Histogram<double> _claimsTransformationDuration;

    static AuthenticationDiagnostics()
    {
        _authenticationRequests = Meter.CreateCounter<long>(
            VKAuthenticationDiagnosticsConstants.AuthRequestCounterName,
            description: VKAuthenticationDiagnosticsConstants.AuthRequestCounterDescription
        );

        _tooManyRequests = Meter.CreateCounter<long>(
            VKAuthenticationDiagnosticsConstants.TooManyRequestsCounterName,
            unit: VKAuthenticationDiagnosticsConstants.TooManyRequestsCounterUnit,
            description: VKAuthenticationDiagnosticsConstants.TooManyRequestsCounterDescription
        );

        _revocations = Meter.CreateCounter<long>(
            VKAuthenticationDiagnosticsConstants.RevocationCounterName,
            description: VKAuthenticationDiagnosticsConstants.RevocationCounterDescription
        );

        _replayAttacks = Meter.CreateCounter<long>(
            VKAuthenticationDiagnosticsConstants.ReplayCounterName,
            description: VKAuthenticationDiagnosticsConstants.ReplayCounterDescription
        );

        _claimsTransformations = Meter.CreateCounter<long>(
            VKAuthenticationDiagnosticsConstants.ClaimsTransformationCounterName,
            description: VKAuthenticationDiagnosticsConstants.ClaimsTransformationCounterDescription
        );

        _claimsTransformationDuration = Meter.CreateHistogram<double>(
            VKAuthenticationDiagnosticsConstants.ClaimsTransformationDurationName,
            unit: VKAuthenticationDiagnosticsConstants.ClaimsTransformationDurationUnit,
            description: VKAuthenticationDiagnosticsConstants.ClaimsTransformationDurationDescription
        );
    }

    /// <summary>
    /// Records an authentication attempt result.
    /// </summary>
    /// <param name="authType">The type of authentication (e.g., "jwt", "apikey").</param>
    /// <param name="isSuccess">Whether the authentication succeeded.</param>
    /// <param name="failureReason">The reason for failure, if any.</param>
    public static void RecordAuthAttempt(string authType, bool isSuccess, string? failureReason = null)
    {
        var tags = new TagList
        {
            { VKAuthenticationDiagnosticsConstants.TagAuthType, authType },
            { VKAuthenticationDiagnosticsConstants.TagAuthResult, isSuccess ? VKAuthenticationDiagnosticsConstants.ValueSuccess : VKAuthenticationDiagnosticsConstants.ValueFailure }
        };

        if (!isSuccess && !string.IsNullOrEmpty(failureReason))
        {
            tags.Add(VKAuthenticationDiagnosticsConstants.TagFailureReason, failureReason);
        }

        _authenticationRequests.Add(1, tags);
    }

    /// <summary>
    /// Records an authentication rejection due to revocation.
    /// </summary>
    /// <param name="authType">The type of authentication.</param>
    public static void RecordRevocationHit(string authType)
    {
        _revocations.Add(1, new KeyValuePair<string, object?>(VKAuthenticationDiagnosticsConstants.TagAuthType, authType));
    }

    /// <summary>
    /// Records a detected refresh token replay attack.
    /// </summary>
    /// <param name="familyId">The family identifier of the token.</param>
    public static void RecordReplayAttack(string familyId)
    {
        _replayAttacks.Add(1, new KeyValuePair<string, object?>(VKAuthenticationDiagnosticsConstants.TagUserId, familyId));
    }

    /// <summary>
    /// Records a claims transformation attempt and its duration.
    /// </summary>
    /// <param name="durationMs">The duration of the transformation in milliseconds.</param>
    /// <param name="applied">Whether transformation was actually applied.</param>
    public static void RecordClaimsTransformation(double durationMs, bool applied)
    {
        var tags = new TagList
        {
            { VKAuthenticationDiagnosticsConstants.TagClaimsTransformed, applied }
        };

        _claimsTransformations.Add(1, tags);
        _claimsTransformationDuration.Record(durationMs, tags);
    }

    /// <summary>
    /// Records a rate limit exceeded event with specific key and tenant context.
    /// </summary>
    /// <param name="keyId">The identifier of the API key.</param>
    /// <param name="tenantId">The identifier of the tenant (optional).</param>
    public static void RecordTooManyRequests(string keyId, string? tenantId)
    {
        var tags = new TagList
        {
            { VKAuthenticationDiagnosticsConstants.TagKeyId, keyId },
            { VKAuthenticationDiagnosticsConstants.TagTenantId, tenantId ?? string.Empty }
        };

        _tooManyRequests.Add(1, tags);
    }

    /// <summary>
    /// Starts a new activity for JWT validation.
    /// </summary>
    public static Activity? StartJwtValidation()
        => Source.StartActivity(VKAuthenticationDiagnosticsConstants.ActivityAuthenticateJwt);

    /// <summary>
    /// Starts a new activity for API key validation.
    /// </summary>
    public static Activity? StartApiKeyValidation()
        => Source.StartActivity(VKAuthenticationDiagnosticsConstants.ActivityValidateApiKey);

    /// <summary>
    /// Starts a new activity for claims transformation.
    /// </summary>
    public static Activity? StartClaimsTransformation()
        => Source.StartActivity(VKAuthenticationDiagnosticsConstants.ActivityTransformClaims);

    /// <summary>
    /// Gets the compiled authentication metadata for all discovered endpoints.
    /// </summary>
    public static IReadOnlyDictionary<string, VKEndpointAuthInfo> GetAuthenticationMetadata() => Generated.AuthenticationMetadata.Endpoints;

    /// <summary>
    /// Records a failed authentication attempt.
    /// </summary>
    /// <param name="scheme">The authentication scheme that failed.</param>
    /// <param name="reason">The failure reason.</param>
    internal static void RecordAuthFailure(string scheme, string reason)
    {
        RecordAuthAttempt(scheme, false, reason);
    }

    /// <summary>
    /// Gets the deterministic hash of the authentication metadata.
    /// </summary>
    public static string GetMetadataHash() => AuthenticationMetadata.MetadataHash;

    /// <summary>
    /// Gets runtime information about all registered authentication schemes.
    /// </summary>
    /// <param name="serviceProvider">The service provider to resolve scheme information.</param>
    /// <returns>A collection of registered authentication schemes.</returns>
    internal static async Task<IEnumerable<AuthenticationSchemeInfo>> GetRegisteredSchemesAsync(IServiceProvider serviceProvider)
    {
        var schemeProvider = serviceProvider.GetRequiredService<IAuthenticationSchemeProvider>();
        var schemes = await schemeProvider.GetAllSchemesAsync().ConfigureAwait(false);
        var defaultScheme = await schemeProvider.GetDefaultAuthenticateSchemeAsync().ConfigureAwait(false);

        return schemes.Select(s => new AuthenticationSchemeInfo
        {
            Name = s.Name,
            DisplayName = s.DisplayName,
            HandlerType = s.HandlerType.Name,
            IsDefault = defaultScheme?.Name == s.Name
        });
    }

    // --- Merged from ApiKeyLog ---
    /// <summary>
    /// Logs that an API key is too short based on the minimum length requirement.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    /// <param name="minLength">The minimum required length.</param>
    [LoggerMessage(
        EventId = 2301,
        Level = LogLevel.Warning,
        Message = "API key is too short. MinLength: {MinLength}")]
    internal static partial void LogApiKeyTooShort(this ILogger logger, int minLength);

    /// <summary>
    /// Logs that an API key was not found in the store.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    /// <param name="hashPrefix">The prefix of the hashed API key for identification.</param>
    [LoggerMessage(
        EventId = 2302,
        Level = LogLevel.Warning,
        Message = "API key not found. Hash starting with: {HashPrefix}****")]
    internal static partial void LogApiKeyNotFound(this ILogger logger, string hashPrefix);

    /// <summary>
    /// Logs that a revoked API key was used.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    /// <param name="keyId">The unique identifier of the API key.</param>
    [LoggerMessage(
        EventId = 2311,
        Level = LogLevel.Warning,
        Message = "Revoked API key used. KeyId: {KeyId}")]
    internal static partial void LogRevokedApiKeyUsed(this ILogger logger, string keyId);

    /// <summary>
    /// Logs that an expired API key was used.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    /// <param name="keyId">The unique identifier of the API key.</param>
    [LoggerMessage(
        EventId = 2312,
        Level = LogLevel.Warning,
        Message = "Expired API key used. KeyId: {KeyId}")]
    internal static partial void LogExpiredApiKeyUsed(this ILogger logger, string keyId);

    /// <summary>
    /// Logs that a disabled API key was used.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    /// <param name="keyId">The unique identifier of the API key.</param>
    [LoggerMessage(
        EventId = 2313,
        Level = LogLevel.Warning,
        Message = "Disabled API key used. KeyId: {KeyId}")]
    internal static partial void LogDisabledApiKeyUsed(this ILogger logger, string keyId);

    /// <summary>
    /// Logs that an API key has exceeded its rate limit.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    /// <param name="keyId">The unique identifier of the API key.</param>
    [LoggerMessage(
        EventId = 2321,
        Level = LogLevel.Warning,
        Message = "API key rate limit exceeded. KeyId: {KeyId}")]
    internal static partial void LogRateLimitExceeded(this ILogger logger, string keyId);

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
    internal static partial void LogLastUsedUpdateFailed(this ILogger logger, Exception ex, string keyId);

    // --- Merged from ClaimsLog ---
    /// <summary>
    /// Logs an error that occurred during claims transformation for a specific user.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    /// <param name="ex">The exception that occurred.</param>
    /// <param name="userId">The identifier of the user.</param>
    [LoggerMessage(
        EventId = 2201,
        Level = LogLevel.Error,
        Message = "VKError occurring during claims transformation for UserId: {UserId}")]
    internal static partial void LogClaimsTransformationError(this ILogger logger, Exception ex, string? userId);

    // --- Merged from InMemoryCleanupLog ---
    /// <summary>
    /// Logs that no active in-memory providers were detected and the service is stopping.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    [LoggerMessage(
        EventId = 2001,
        Level = LogLevel.Warning,
        Message = "In-Memory Cleanup Service: No active in-memory authentication providers detected. The background service will terminate to save system resources.")]
    public static partial void LogNoActiveProviders(this ILogger logger);

    /// <summary>
    /// Logs that the cleanup service is starting.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    /// <param name="count">The number of active providers.</param>
    [LoggerMessage(
        EventId = 2002,
        Level = LogLevel.Information,
        Message = "In-Memory Cleanup Service: Starting. Monitoring {Count} active in-memory providers.")]
    public static partial void LogServiceStarting(this ILogger logger, int count);

    /// <summary>
    /// Logs that the service is evaluating cleanup for providers.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    /// <param name="count">The number of monitored providers.</param>
    [LoggerMessage(
        EventId = 2003,
        Level = LogLevel.Debug,
        Message = "Evaluating cleanup for {Count} in-memory providers.")]
    public static partial void LogEvaluatingCleanup(this ILogger logger, int count);

    /// <summary>
    /// Logs that a provider is being skipped as it is not the active one in the container.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    /// <param name="providerType">The name of the provider type.</param>
    /// <param name="serviceType">The name of the service type.</param>
    [LoggerMessage(
        EventId = 2004,
        Level = LogLevel.Debug,
        Message = "Skipping cleanup for {ProviderType} as it is not the active provider for {ServiceType}.")]
    public static partial void LogSkippingProvider(this ILogger logger, string providerType, string serviceType);

    /// <summary>
    /// Logs an error that occurred during cleanup.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    /// <param name="ex">The exception that occurred.</param>
    /// <param name="providerType">The name of the provider type.</param>
    [LoggerMessage(
        EventId = 2005,
        Level = LogLevel.Error,
        Message = "VKError occurring during cleanup of {ProviderType}.")]
    public static partial void LogCleanupError(this ILogger logger, Exception ex, string providerType);

    /// <summary>
    /// Logs that the cleanup service is stopping.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    [LoggerMessage(
        EventId = 2006,
        Level = LogLevel.Information,
        Message = "In-Memory Cleanup Service: Stopping.")]
    public static partial void LogServiceStopping(this ILogger logger);

    // --- Merged from JwtLog ---
    /// <summary>
    /// Logs a message indicating that JWT validation options are not configured properly.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    [LoggerMessage(
        EventId = 2101,
        Level = LogLevel.Error,
        Message = "JWT Validation options are not configured properly.")]
    internal static partial void LogOptionsNotConfigured(this ILogger logger);

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
    internal static partial void LogAuthenticationFailed(this ILogger logger, Exception? ex, string errorCode);

    /// <summary>
    /// Logs a message indicating an unexpected error during JWT authentication.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    /// <param name="ex">The exception that occurred.</param>
    [LoggerMessage(
        EventId = 2103,
        Level = LogLevel.Warning,
        Message = "Unexpected error occurring during JWT authentication.")]
    internal static partial void LogUnexpectedAuthenticationError(this ILogger logger, Exception ex);

    /// <summary>
    /// Logs a message indicating an invalid refresh token validation request.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    [LoggerMessage(
        EventId = 2111,
        Level = LogLevel.Warning,
        Message = "Invalid refresh token validation request: JTI or FamilyId is missing.")]
    internal static partial void LogInvalidRefreshTokenRequest(this ILogger logger);

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
    internal static partial void LogRefreshTokenReplayDetected(this ILogger logger, string familyId, string jti);

    /// <summary>
    /// Logs a message indicating that refresh token attempts exceeded the rate limit.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    /// <param name="familyId">The token family identifier.</param>
    [LoggerMessage(
        EventId = 2113,
        Level = LogLevel.Warning,
        Message = "Refresh token attempts exceeded rate limit. FamilyId: {FamilyId}")]
    internal static partial void LogRefreshTokenRateLimitExceeded(this ILogger logger, string familyId);

    // --- Merged from TrackingLog ---
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
