using System;
using System.Threading;
using System.Threading.Tasks;

namespace VK.Blocks.Authentication.ApiKeys;

/// <summary>
/// Defines a mechanism for rate limiting incoming API Key validations.
/// </summary>
public interface IApiKeyRateLimiter
{
    /// <summary>
    /// Determines whether the processing of an API key is allowed under the current rate limits.
    /// </summary>
    /// <param name="keyId">The identifier of the API key.</param>
    /// <param name="limitPerMinute">The maximum allowed requests per minute for this key.</param>
    /// <param name="ct">A cancellation token.</param>
    /// <returns>True if the request is allowed; False if the rate limit has been exceeded.</returns>
    Task<bool> IsAllowedAsync(Guid keyId, int limitPerMinute, CancellationToken ct = default);
}
