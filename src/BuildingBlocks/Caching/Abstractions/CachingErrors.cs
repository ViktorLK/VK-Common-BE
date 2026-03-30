using VK.Blocks.Core.Results;

namespace VK.Blocks.Caching.Abstractions;

/// <summary>
/// Domain-specific errors for the Caching module.
/// </summary>
public static class CachingErrors
{
    public static readonly Error ProviderError = new("Caching.ProviderError", "An error occurred in the cache provider.");
    public static readonly Error LockAcquisitionFailed = new("Caching.LockAcquisitionFailed", "Failed to acquire the distributed lock.");
    public static readonly Error SerializationError = new("Caching.SerializationError", "An error occurred during cache serialization or deserialization.");
}

