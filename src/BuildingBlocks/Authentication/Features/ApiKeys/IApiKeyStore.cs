using VK.Blocks.Core.Results;

namespace VK.Blocks.Authentication.Features.ApiKeys;

/// <summary>
/// Defines a store for managing and retrieving API keys.
/// </summary>
public interface IApiKeyStore
{
    #region Public Methods

    /// <summary>
    /// Finds an API key record asynchronously by its hashed key.
    /// </summary>
    /// <param name="hashedApiKey">The hashed representation of the API key.</param>
    /// <param name="ct">A cancellation token.</param>
    /// <returns>A result containing the matching API key record.</returns>
    Task<Result<ApiKeyRecord>> FindByHashAsync(string hashedApiKey, CancellationToken ct = default);

    /// <summary>
    /// Updates the last used timestamp for a specific API key asynchronously.
    /// </summary>
    /// <param name="keyId">The unique identifier of the API key.</param>
    /// <param name="usedAt">The timestamp when the API key was last used.</param>
    /// <param name="ct">A cancellation token.</param>
    /// <returns>A task that represents the asynchronous update operation.</returns>
    Task UpdateLastUsedAtAsync(Guid keyId, DateTimeOffset usedAt, CancellationToken ct = default);

    #endregion
}
