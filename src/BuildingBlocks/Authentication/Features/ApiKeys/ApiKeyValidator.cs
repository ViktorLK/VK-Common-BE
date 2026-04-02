using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using VK.Blocks.Authentication.Diagnostics;
using VK.Blocks.Core.Results;

namespace VK.Blocks.Authentication.Features.ApiKeys;

/// <summary>
/// Validates incoming API keys against the stored records.
/// </summary>
public sealed class ApiKeyValidator(
    IApiKeyStore store,
    IApiKeyRevocationProvider revocationProvider,
    IApiKeyRateLimiter rateLimiter,
    IOptionsMonitor<ApiKeyOptions> optionsMonitor,
    TimeProvider timeProvider,
    ILogger<ApiKeyValidator> logger)
{
    #region Public Methods

    /// <summary>
    /// Validates a raw API key asynchronously.
    /// </summary>
    /// <param name="rawApiKey">The raw API key string to be validated.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A result containing the API key context if successful; otherwise, an error.</returns>
    public async Task<Result<ApiKeyContext>> ValidateAsync(
        string rawApiKey,
        CancellationToken cancellationToken = default)
    {
        using var activity = AuthenticationDiagnostics.StartApiKeyValidation();

        var settings = optionsMonitor.CurrentValue;

        // Enforce input presence and minimum length
        if (string.IsNullOrWhiteSpace(rawApiKey) || rawApiKey.Length < settings.MinLength)
        {
            if (rawApiKey?.Length < settings.MinLength && !string.IsNullOrWhiteSpace(rawApiKey))
            {
                logger.LogApiKeyTooShort(settings.MinLength);
            }
            AuthenticationDiagnostics.RecordAuthAttempt(AuthenticationDiagnosticsConstants.TypeApiKey, false, ApiKeyErrors.Invalid.Code);
            return Result.Failure<ApiKeyContext>(ApiKeyErrors.Invalid);
        }

        var hashedApiKey = HashApiKey(rawApiKey);

        var storeResult = await store.FindByHashAsync(hashedApiKey, cancellationToken).ConfigureAwait(false);
        if (storeResult.IsFailure)
        {
            // Mask the hash in logs for security (Rule 7 / Audit 2026-04-01)
            logger.LogApiKeyNotFound(hashedApiKey[..4]);
            AuthenticationDiagnostics.RecordAuthAttempt(AuthenticationDiagnosticsConstants.TypeApiKey, false, ApiKeyErrors.Invalid.Code);
            return Result.Failure<ApiKeyContext>(ApiKeyErrors.Invalid);
        }

        var apiKeyRecord = storeResult.Value;

        if (await revocationProvider.IsRevokedAsync(apiKeyRecord.Id.ToString(), cancellationToken).ConfigureAwait(false))
        {
            logger.LogRevokedApiKeyUsed(apiKeyRecord.Id.ToString());
            AuthenticationDiagnostics.RecordRevocationHit(AuthenticationDiagnosticsConstants.TypeApiKey);
            AuthenticationDiagnostics.RecordAuthAttempt(AuthenticationDiagnosticsConstants.TypeApiKey, false, ApiKeyErrors.Revoked.Code);
            return Result.Failure<ApiKeyContext>(ApiKeyErrors.Revoked);
        }

        var now = timeProvider.GetUtcNow();
        if (apiKeyRecord.ExpiresAt.HasValue && apiKeyRecord.ExpiresAt.Value < now)
        {
            logger.LogExpiredApiKeyUsed(apiKeyRecord.Id.ToString());
            AuthenticationDiagnostics.RecordAuthAttempt(AuthenticationDiagnosticsConstants.TypeApiKey, false, ApiKeyErrors.Expired.Code);
            return Result.Failure<ApiKeyContext>(ApiKeyErrors.Expired);
        }

        if (!apiKeyRecord.IsEnabled)
        {
            logger.LogDisabledApiKeyUsed(apiKeyRecord.Id.ToString());
            AuthenticationDiagnostics.RecordAuthAttempt(AuthenticationDiagnosticsConstants.TypeApiKey, false, ApiKeyErrors.Disabled.Code);
            return Result.Failure<ApiKeyContext>(ApiKeyErrors.Disabled);
        }

        // Apply Rate Limiting
        if (settings.EnableRateLimiting)
        {
            if (!await rateLimiter.IsAllowedAsync(apiKeyRecord.Id, settings.RateLimitPerMinute, settings.RateLimitWindowSeconds, cancellationToken).ConfigureAwait(false))
            {
                logger.LogRateLimitExceeded(apiKeyRecord.Id.ToString());
                AuthenticationDiagnostics.RecordTooManyRequests(apiKeyRecord.Id.ToString(), apiKeyRecord.TenantId ?? string.Empty);
                AuthenticationDiagnostics.RecordAuthAttempt(AuthenticationDiagnosticsConstants.TypeApiKey, false, ApiKeyErrors.RateLimitExceeded.Code);
                return Result.Failure<ApiKeyContext>(ApiKeyErrors.RateLimitExceeded);
            }
        }

        if (settings.TrackLastUsedAt)
        {
            await UpdateLastUsedAsync(apiKeyRecord.Id, cancellationToken).ConfigureAwait(false);
        }

        AuthenticationDiagnostics.RecordAuthAttempt(AuthenticationDiagnosticsConstants.TypeApiKey, true);
        activity?.SetTag(AuthenticationDiagnosticsConstants.TagKeyId, apiKeyRecord.Id.ToString());
        activity?.SetTag(AuthenticationDiagnosticsConstants.TagTenantId, apiKeyRecord.TenantId);

        return Result.Success(new ApiKeyContext
        {
            KeyId = apiKeyRecord.Id,
            OwnerId = apiKeyRecord.OwnerId,
            TenantId = apiKeyRecord.TenantId,
            Scopes = apiKeyRecord.Scopes
        });
    }

    #endregion

    #region Private Methods

    /// <summary>
    /// Hashes the raw API key using SHA256.
    /// </summary>
    /// <param name="rawApiKey">The raw API key to hash.</param>
    /// <returns>The hexadecimal string representation of the hashed key.</returns>
    private static string HashApiKey(string rawApiKey)
    {
        // PERF: Using stackalloc to avoid memory allocation for frequent calls.
        var maxByteCount = Encoding.UTF8.GetMaxByteCount(rawApiKey.Length);
        Span<byte> buffer = maxByteCount <= 256 ? stackalloc byte[maxByteCount] : new byte[maxByteCount];
        var byteCount = Encoding.UTF8.GetBytes(rawApiKey, buffer);

        Span<byte> hashBuffer = stackalloc byte[32]; // SHA256 produces 32 bytes
        SHA256.HashData(buffer[..byteCount], hashBuffer);

        // PERF: High-performance hex string generation with only one allocation (Audit 2026-04-01).
        Span<char> hexBuffer = stackalloc char[64];
        for (var i = 0; i < hashBuffer.Length; i++)
        {
            hashBuffer[i].TryFormat(hexBuffer[(i * 2)..], out _, "x2");
        }

        return new string(hexBuffer);
    }

    /// <summary>
    /// Updates the last used timestamp of the API key asynchronously in a fire-and-forget fashion.
    /// </summary>
    /// <param name="keyId">The identifier of the API key.</param>
    /// <param name="ct">A cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    private async Task UpdateLastUsedAsync(Guid keyId, CancellationToken ct)
    {
        try
        {
            await store.UpdateLastUsedAtAsync(keyId, timeProvider.GetUtcNow(), ct).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            logger.LogLastUsedUpdateFailed(ex, keyId.ToString());
        }
    }

    #endregion
}
