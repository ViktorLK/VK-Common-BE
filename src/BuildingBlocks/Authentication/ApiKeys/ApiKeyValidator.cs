using System;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using VK.Blocks.Authentication.Abstractions;
using VK.Blocks.Core.Results;

namespace VK.Blocks.Authentication.ApiKeys;

/// <summary>
/// Validates incoming API keys against the stored records.
/// </summary>
public sealed class ApiKeyValidator(
    IApiKeyStore store,
    ITokenBlacklist blacklist,
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
        // Enforce input presence
        if (string.IsNullOrWhiteSpace(rawApiKey))
        {
            return Result.Failure<ApiKeyContext>(new Error("ApiKey.Empty", "API key is empty", ErrorType.Validation));
        }

        var hashedKey = HashApiKey(rawApiKey);

        var apiKey = await store.FindByHashAsync(hashedKey, ct);
        if (apiKey is null)
        {
            logger.LogWarning("API key not found. Hash: {Hash}", hashedKey[..8]);
            return Result.Failure<ApiKeyContext>(new Error("ApiKey.Invalid", "Invalid API key", ErrorType.Unauthorized));
        }

        var blacklistKey = $"apikey:{apiKey.Id}";
        if (await blacklist.IsRevokedAsync(blacklistKey, ct))
        {
            logger.LogWarning("Revoked API key used. KeyId: {KeyId}", apiKey.Id);
            return Result.Failure<ApiKeyContext>(new Error("ApiKey.Revoked", "API key has been revoked", ErrorType.Unauthorized));
        }

        if (apiKey.ExpiresAt.HasValue && apiKey.ExpiresAt.Value < DateTimeOffset.UtcNow)
        {
            logger.LogWarning("Expired API key used. KeyId: {KeyId}", apiKey.Id);
            return Result.Failure<ApiKeyContext>(new Error("ApiKey.Expired", "API key has expired", ErrorType.Unauthorized));
        }

        if (!apiKey.IsEnabled)
        {
            return Result.Failure<ApiKeyContext>(new Error("ApiKey.Disabled", "API key is disabled", ErrorType.Unauthorized));
        }

        await UpdateLastUsedAsync(apiKey.Id, ct);

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
