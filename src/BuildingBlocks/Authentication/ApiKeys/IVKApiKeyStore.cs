using System;
using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.Core;

namespace VK.Blocks.Authentication;

/// <summary>
/// Defines a store for managing and retrieving API keys.
/// </summary>
public interface IVKApiKeyStore
{
    /// <summary>
    /// Finds an API key record asynchronously by its hashed key.
    /// </summary>
    /// <param name="hashedApiKey">The hashed representation of the API key.</param>
    /// <param name="ct">A cancellation token.</param>
    /// <returns>A result containing the matching API key record.</returns>
    ValueTask<VKResult<VKApiKeyRecord>> FindByHashAsync(string hashedApiKey, CancellationToken ct = default);

    /// <summary>
    /// Updates the last used timestamp for a specific API key asynchronously.
    /// </summary>
    /// <param name="keyId">The unique identifier of the API key.</param>
    /// <param name="usedAt">The timestamp when the API key was last used.</param>
    /// <param name="ct">A cancellation token.</param>
    /// <returns>A task that represents the asynchronous update operation.</returns>
    ValueTask UpdateLastUsedAtAsync(Guid keyId, DateTimeOffset usedAt, CancellationToken ct = default);
}
