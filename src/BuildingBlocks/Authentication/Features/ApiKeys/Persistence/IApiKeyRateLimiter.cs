namespace VK.Blocks.Authentication.Features.ApiKeys.Persistence;

/// <summary>
/// Defines a mechanism for rate limiting incoming API Key validations.
/// </summary>
public interface IApiKeyRateLimiter
{
    #region Public Methods

    /// <summary>
    /// Determines whether the processing of an API key is allowed under the current rate limits.
    /// </summary>
    /// <param name="keyId">The identifier of the API key.</param>
    /// <param name="limit">The maximum allowed requests per window for this key.</param>
    /// <param name="windowSeconds">The window duration in seconds.</param>
    /// <param name="ct">A cancellation token.</param>
    /// <returns>True if the request is allowed; False if the rate limit has been exceeded.</returns>
    ValueTask<bool> IsAllowedAsync(Guid keyId, int limit, int windowSeconds, CancellationToken ct = default);

    #endregion
}
