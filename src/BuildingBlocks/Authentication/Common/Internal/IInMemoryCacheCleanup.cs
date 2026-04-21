using System;

namespace VK.Blocks.Authentication.Common.Internal;

/// <summary>
/// Defines a contract for in-memory providers that require periodic cleanup of expired entries.
/// </summary>
internal interface IInMemoryCacheCleanup
{
    /// <summary>
    /// Gets the type of the service interface associated with this cleanup provider.
    /// Used by the background service to verify if this provider is the active one in the DI container.
    /// </summary>
    Type AssociatedServiceType { get; }

    /// <summary>
    /// Performs a scan and removes any expired entries from the internal storage.
    /// </summary>
    void CleanupExpiredEntries();
}
