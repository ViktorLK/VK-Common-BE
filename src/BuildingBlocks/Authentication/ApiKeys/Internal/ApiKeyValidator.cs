using System;
using System.Buffers;
using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using VK.Blocks.Authentication.Diagnostics.Internal;
using VK.Blocks.Core;

namespace VK.Blocks.Authentication.ApiKeys.Internal;

/// <summary>
/// Validates incoming API keys against the stored records.
/// </summary>
internal sealed class ApiKeyValidator(
    IVKApiKeyStore apiKeyStore,
    IVKApiKeyRevocationProvider revocationProvider,
    IVKApiKeyRateLimiter rateLimiter,
    IOptions<VKApiKeyOptions> options,
    TimeProvider timeProvider,
    ILogger<ApiKeyValidator> logger)
{
    private readonly IVKApiKeyStore _apiKeyStore = VKGuard.NotNull(apiKeyStore);
    private readonly IVKApiKeyRevocationProvider _revocationProvider = VKGuard.NotNull(revocationProvider);
    private readonly IVKApiKeyRateLimiter _rateLimiter = VKGuard.NotNull(rateLimiter);
    private readonly VKApiKeyOptions _options = VKGuard.NotNull(options).Value;
    private readonly TimeProvider _timeProvider = VKGuard.NotNull(timeProvider);
    private readonly ILogger<ApiKeyValidator> _logger = VKGuard.NotNull(logger);

    /// <summary>
    /// Validates a raw API key asynchronously.
    /// </summary>
    /// <param name="rawApiKey">The raw API key string to be validated.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A result containing the API key context if successful; otherwise, an error.</returns>
    public async ValueTask<VKResult<ApiKeyContext>> ValidateAsync(
        string rawApiKey,
        CancellationToken cancellationToken = default)
    {
        using Activity? activity = AuthenticationDiagnostics.StartApiKeyValidation();

        VKApiKeyOptions settings = _options;

        // Enforce input presence and minimum length
        if (string.IsNullOrWhiteSpace(rawApiKey) || rawApiKey.Length < settings.MinLength)
        {
            if (rawApiKey?.Length < settings.MinLength && !string.IsNullOrWhiteSpace(rawApiKey))
            {
                _logger.LogApiKeyTooShort(settings.MinLength);
            }
            AuthenticationDiagnostics.RecordAuthAttempt(VKAuthenticationDiagnosticsConstants.TypeApiKey, false, VKApiKeyErrors.Invalid.Code);
            return VKResult.Failure<ApiKeyContext>(VKApiKeyErrors.Invalid);
        }

        string hashedApiKey = HashApiKey(rawApiKey);

        VKResult<VKApiKeyRecord> storeResult = await _apiKeyStore.FindByHashAsync(hashedApiKey, cancellationToken).ConfigureAwait(false);
        if (storeResult.IsFailure)
        {
            // Mask the hash in logs for security (Rule 7 / Audit 2026-04-01)
            _logger.LogApiKeyNotFound(hashedApiKey[..4]);
            AuthenticationDiagnostics.RecordAuthAttempt(VKAuthenticationDiagnosticsConstants.TypeApiKey, false, VKApiKeyErrors.Invalid.Code);
            return VKResult.Failure<ApiKeyContext>(VKApiKeyErrors.Invalid);
        }

        VKApiKeyRecord apiKeyRecord = storeResult.Value;

        if (await _revocationProvider.IsRevokedAsync(apiKeyRecord.Id.ToString(), cancellationToken).ConfigureAwait(false))
        {
            _logger.LogRevokedApiKeyUsed(apiKeyRecord.Id.ToString());
            AuthenticationDiagnostics.RecordRevocationHit(VKAuthenticationDiagnosticsConstants.TypeApiKey);
            AuthenticationDiagnostics.RecordAuthAttempt(VKAuthenticationDiagnosticsConstants.TypeApiKey, false, VKApiKeyErrors.Revoked.Code);
            return VKResult.Failure<ApiKeyContext>(VKApiKeyErrors.Revoked);
        }

        DateTimeOffset now = _timeProvider.GetUtcNow();
        if (apiKeyRecord.ExpiresAt.HasValue && apiKeyRecord.ExpiresAt.Value < now)
        {
            _logger.LogExpiredApiKeyUsed(apiKeyRecord.Id.ToString());
            AuthenticationDiagnostics.RecordAuthAttempt(VKAuthenticationDiagnosticsConstants.TypeApiKey, false, VKApiKeyErrors.Expired.Code);
            return VKResult.Failure<ApiKeyContext>(VKApiKeyErrors.Expired);
        }

        if (!apiKeyRecord.IsEnabled)
        {
            _logger.LogDisabledApiKeyUsed(apiKeyRecord.Id.ToString());
            AuthenticationDiagnostics.RecordAuthAttempt(VKAuthenticationDiagnosticsConstants.TypeApiKey, false, VKApiKeyErrors.Disabled.Code);
            return VKResult.Failure<ApiKeyContext>(VKApiKeyErrors.Disabled);
        }

        // Apply Rate Limiting
        if (settings.EnableRateLimiting)
        {
            if (!await _rateLimiter.IsAllowedAsync(apiKeyRecord.Id, settings.RateLimitPerMinute, settings.RateLimitWindowSeconds, cancellationToken).ConfigureAwait(false))
            {
                _logger.LogRateLimitExceeded(apiKeyRecord.Id.ToString());
                AuthenticationDiagnostics.RecordTooManyRequests(apiKeyRecord.Id.ToString(), apiKeyRecord.TenantId ?? string.Empty);
                AuthenticationDiagnostics.RecordAuthAttempt(VKAuthenticationDiagnosticsConstants.TypeApiKey, false, VKApiKeyErrors.RateLimitExceeded.Code);
                return VKResult.Failure<ApiKeyContext>(VKApiKeyErrors.RateLimitExceeded);
            }
        }

        if (settings.TrackLastUsedAt)
        {
            await UpdateLastUsedAsync(apiKeyRecord.Id, cancellationToken).ConfigureAwait(false);
        }

        AuthenticationDiagnostics.RecordAuthAttempt(VKAuthenticationDiagnosticsConstants.TypeApiKey, true);
        activity?.SetTag(VKAuthenticationDiagnosticsConstants.TagKeyId, apiKeyRecord.Id.ToString());
        activity?.SetTag(VKAuthenticationDiagnosticsConstants.TagTenantId, apiKeyRecord.TenantId);

        return VKResult.Success(new ApiKeyContext
        {
            KeyId = apiKeyRecord.Id,
            OwnerId = apiKeyRecord.OwnerId,
            TenantId = apiKeyRecord.TenantId,
            Scopes = apiKeyRecord.Scopes
        });
    }

    /// <summary>
    /// Hashes the raw API key using SHA256.
    /// </summary>
    /// <param name="rawApiKey">The raw API key to hash.</param>
    /// <returns>The hexadecimal string representation of the hashed key.</returns>
    private static string HashApiKey(string rawApiKey)
    {
        // PERF: Using stackalloc to avoid memory allocation for frequent calls.
        // Rule 4.3: Use ArrayPool for large temporary buffers (> 256 bytes) to reduce GC pressure.
        int maxByteCount = Encoding.UTF8.GetMaxByteCount(rawApiKey.Length);
        byte[]? sharedBuffer = null;
        Span<byte> buffer = maxByteCount <= 256
            ? stackalloc byte[maxByteCount]
            : (sharedBuffer = ArrayPool<byte>.Shared.Rent(maxByteCount));

        try
        {
            int byteCount = Encoding.UTF8.GetBytes(rawApiKey, buffer);

            Span<byte> hashBuffer = stackalloc byte[32]; // SHA256 produces 32 bytes
            SHA256.HashData(buffer[..byteCount], hashBuffer);

            // PERF: High-performance hex string generation with only one allocation (Audit 2026-04-01).
            Span<char> hexBuffer = stackalloc char[64];
            for (int i = 0; i < hashBuffer.Length; i++)
            {
                hashBuffer[i].TryFormat(hexBuffer[(i * 2)..], out _, "x2");
            }

            return new string(hexBuffer);
        }
        finally
        {
            if (sharedBuffer is not null)
            {
                ArrayPool<byte>.Shared.Return(sharedBuffer);
            }
        }
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
            await _apiKeyStore.UpdateLastUsedAtAsync(keyId, _timeProvider.GetUtcNow(), ct).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogLastUsedUpdateFailed(ex, keyId.ToString());
        }
    }
}
