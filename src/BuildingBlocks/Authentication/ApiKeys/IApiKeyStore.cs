using System;
using System.Threading;
using System.Threading.Tasks;

namespace VK.Blocks.Authentication.ApiKeys;

/// <summary>
/// Defines a store for managing and retrieving API keys.
/// </summary>
public interface IApiKeyStore
{
    /// <summary>
    /// Finds an API key record asynchronously by its hashed key.
    /// </summary>
    /// <param name="hashedKey">The hashed representation of the API key.</param>
    /// <param name="ct">A cancellation token.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the matching API key record, or <c>null</c> if not found.</returns>
    Task<ApiKeyRecord?> FindByHashAsync(string hashedKey, CancellationToken ct = default);

    /// <summary>
    /// Updates the last used timestamp for a specific API key asynchronously.
    /// </summary>
    /// <param name="keyId">The unique identifier of the API key.</param>
    /// <param name="usedAt">The timestamp when the API key was last used.</param>
    /// <param name="ct">A cancellation token.</param>
    /// <returns>A task that represents the asynchronous update operation.</returns>
    Task UpdateLastUsedAtAsync(Guid keyId, DateTimeOffset usedAt, CancellationToken ct = default);
}
