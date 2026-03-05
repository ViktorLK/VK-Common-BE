using System;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using VK.Blocks.Authentication.Abstractions;
using VK.Blocks.Authentication.Diagnostics;
using VK.Blocks.Authentication.Options;
using VK.Blocks.Core.Results;

namespace VK.Blocks.Authentication.ApiKeys;

/// <summary>
/// Validates incoming API keys against the stored records.
/// </summary>
public sealed class ApiKeyValidator(
    IApiKeyStore store,
    IApiKeyBlacklist blacklist,
    IApiKeyRateLimiter rateLimiter,
    IOptionsMonitor<VKAuthenticationOptions> optionsMonitor,
    ILogger<ApiKeyValidator> logger)
{
    #region Public Methods

    /// <summary>
    /// Validates a raw API key asynchronously.
    /// </summary>
    /// <param name="rawApiKey">The raw API key string to be validated.</param>
    /// <param name="ct">A cancellation token.</param>
    /// <returns>A result containing the API key context if successful; otherwise, an error.</returns>
    public async Task<Result<ApiKeyContext>> ValidateAsync(
        string rawApiKey,
        CancellationToken ct = default)
    {
        using var activity = AuthenticationDiagnostics.Source.StartActivity("ApiKeyValidator.ValidateAsync");

        // Enforce input presence
        if (string.IsNullOrWhiteSpace(rawApiKey))
        {
            AuthenticationDiagnostics.RecordAuthAttempt("apikey", false);
            return Result.Failure<ApiKeyContext>(AuthenticationErrors.ApiKey.Empty);
        }

        var hashedKey = HashApiKey(rawApiKey);

        var apiKey = await store.FindByHashAsync(hashedKey, ct);
        if (apiKey is null)
        {
            logger.LogWarning("API key not found. Hash: {Hash}", hashedKey[..8]);
            AuthenticationDiagnostics.RecordAuthAttempt("apikey", false);
            return Result.Failure<ApiKeyContext>(AuthenticationErrors.ApiKey.Invalid);
        }

        if (await blacklist.IsRevokedAsync(apiKey.Id.ToString(), ct))
        {
            logger.LogWarning("Revoked API key used. KeyId: {KeyId}", apiKey.Id);
            AuthenticationDiagnostics.RecordAuthAttempt("apikey", false);
            return Result.Failure<ApiKeyContext>(AuthenticationErrors.ApiKey.Revoked);
        }

        if (apiKey.ExpiresAt.HasValue && apiKey.ExpiresAt.Value < DateTimeOffset.UtcNow)
        {
            logger.LogWarning("Expired API key used. KeyId: {KeyId}", apiKey.Id);
            AuthenticationDiagnostics.RecordAuthAttempt("apikey", false);
            return Result.Failure<ApiKeyContext>(AuthenticationErrors.ApiKey.Expired);
        }

        if (!apiKey.IsEnabled)
        {
            AuthenticationDiagnostics.RecordAuthAttempt("apikey", false);
            return Result.Failure<ApiKeyContext>(AuthenticationErrors.ApiKey.Disabled);
        }

        // Apply Rate Limiting
        var authOptions = optionsMonitor.CurrentValue;
        if (!await rateLimiter.IsAllowedAsync(apiKey.Id, authOptions.ApiKeyRateLimitPerMinute, ct))
        {
            logger.LogWarning("API key rate limit exceeded. KeyId: {KeyId}", apiKey.Id);
            AuthenticationDiagnostics.RecordRateLimitExceeded();
            AuthenticationDiagnostics.RecordAuthAttempt("apikey", false);
            return Result.Failure<ApiKeyContext>(AuthenticationErrors.ApiKey.RateLimitExceeded);
        }

        await UpdateLastUsedAsync(apiKey.Id, ct);

        AuthenticationDiagnostics.RecordAuthAttempt("apikey", true);
        activity?.SetTag("auth.key_id", apiKey.Id.ToString());
        activity?.SetTag("auth.tenant_id", apiKey.TenantId);

        return Result.Success(new ApiKeyContext
        {
            KeyId = apiKey.Id,
            OwnerId = apiKey.OwnerId,
            TenantId = apiKey.TenantId,
            Scopes = apiKey.Scopes
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

        return Convert.ToHexString(hashBuffer).ToLowerInvariant();
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
            await store.UpdateLastUsedAtAsync(keyId, DateTimeOffset.UtcNow, ct);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to update LastUsedAt for KeyId: {KeyId}", keyId);
        }
    }

    #endregion
}
