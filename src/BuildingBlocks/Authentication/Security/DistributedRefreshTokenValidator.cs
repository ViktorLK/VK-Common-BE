using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Options;
using VK.Blocks.Authentication.Abstractions;
using VK.Blocks.Authentication.Options;
using VK.Blocks.Core.Results;

namespace VK.Blocks.Authentication.Security;

/// <summary>
/// Evaluates Refresh Token rotation policies using a distributed cache to track consumed JTIs.
/// </summary>
public sealed class DistributedRefreshTokenValidator(
    IDistributedCache cache,
    IOptionsMonitor<VKAuthenticationOptions> optionsFallback) : IRefreshTokenValidator
{
    #region Fields

    private const string KeyPrefix = "consumed_rt:";

    #endregion

    #region Public Methods

    /// <inheritdoc />
    public async Task<Result<bool>> ValidateTokenRotationAsync(string tokenJti, string familyId, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(tokenJti) || string.IsNullOrWhiteSpace(familyId))
        {
            return Result.Failure<bool>(AuthenticationErrors.RefreshToken.InvalidIds);
        }

        var cacheKey = $"{KeyPrefix}{familyId}:{tokenJti}";

        // Check if the JTI is already in the consumed list for this family
        var consumedValue = await cache.GetStringAsync(cacheKey, ct).ConfigureAwait(false);

        if (consumedValue != null)
        {
            // The JTI was already consumed - this is a replay attack!
            // In a real system, we'd want to emit an event to revoke the ENTIRE family.
            return Result.Failure<bool>(AuthenticationErrors.RefreshToken.Compromised);
        }

        // Cache the consumed JTI.
        // TTL should match or slightly exceed the max lifespan of a refresh token in the system (e.g., 30 days).
        var ttlDays = optionsFallback.CurrentValue.RefreshTokenLifetimeDays;
        var options = new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromDays(ttlDays)
        };

        await cache.SetStringAsync(cacheKey, "consumed", options, ct).ConfigureAwait(false);

        return Result.Success(true);
    }

    #endregion
}
