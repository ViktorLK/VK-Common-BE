using System;
using System.Linq;

namespace VK.Blocks.Core;

/// <summary>
/// Concurrency token validation and formatting extensions for <see cref="IVKConcurrency"/>.
/// Follows AP.01, CS.01.
/// </summary>
public static class VKConcurrencyExtensions
{
    /// <summary>
    /// Validates whether the incoming Base64-encoded concurrency token matches the entity's RowVersion.
    /// Returns Success if matching or if token is omitted/empty; returns ConcurrentUpdate failure on mismatch.
    /// </summary>
    /// <param name="entity">The concurrency-tracked entity.</param>
    /// <param name="clientRowVersionBase64">The Base64 concurrency token passed from the client or request DTO.</param>
    /// <returns>A result indicating success or concurrency conflict / validation error.</returns>
    public static VKResult ValidateConcurrencyToken(this IVKConcurrency entity, string? clientRowVersionBase64)
    {
        VKGuard.NotNull(entity); // [AP.01]

        if (string.IsNullOrWhiteSpace(clientRowVersionBase64))
        {
            return VKResult.Success(); // [CS.01]
        }

        byte[] clientBytes;
        try
        {
            clientBytes = Convert.FromBase64String(clientRowVersionBase64);
        }
        catch (FormatException)
        {
            return VKResult.Failure(VKCoreErrors.Concurrency.InvalidTokenFormat); // [CS.01]
        }

        return entity.ValidateConcurrencyToken(clientBytes);
    }

    /// <summary>
    /// Validates whether the incoming binary RowVersion matches the entity's RowVersion.
    /// Returns Success if matching or if expectedRowVersion is null/empty; returns ConcurrentUpdate failure on mismatch.
    /// </summary>
    /// <param name="entity">The concurrency-tracked entity.</param>
    /// <param name="clientRowVersion">The binary row version expected by the caller.</param>
    /// <returns>A result indicating success or concurrency conflict.</returns>
    public static VKResult ValidateConcurrencyToken(this IVKConcurrency entity, byte[]? clientRowVersion)
    {
        VKGuard.NotNull(entity); // [AP.01]

        if (clientRowVersion is { Length: > 0 } &&
            entity.RowVersion is { Length: > 0 } &&
            !entity.RowVersion.SequenceEqual(clientRowVersion))
        {
            return VKResult.Failure(VKCoreErrors.Concurrency.ConcurrentUpdate); // [CS.01]
        }

        return VKResult.Success(); // [CS.01]
    }

    /// <summary>
    /// Encodes the entity's RowVersion as a Base64 string for API responses or serialization.
    /// </summary>
    /// <param name="entity">The concurrency-tracked entity.</param>
    /// <returns>Base64-encoded string, or null if entity or RowVersion is empty.</returns>
    public static string? ToRowVersionBase64(this IVKConcurrency? entity)
    {
        return entity?.RowVersion is { Length: > 0 } ? Convert.ToBase64String(entity.RowVersion) : null;
    }
}
