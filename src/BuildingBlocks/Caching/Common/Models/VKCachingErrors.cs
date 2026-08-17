using VK.Blocks.Core;

namespace VK.Blocks.Caching;

/// <summary>
/// Domain-specific errors for the Caching module.
/// </summary>
public static class VKCachingErrors
{
    public static readonly VKError ProviderError = new("Caching.Provider.Failed", "An error occurred in the cache provider.");
    public static readonly VKError LockAcquisitionFailed = new("Caching.Lock.AcquisitionFailed", "Failed to acquire the distributed lock.");
    public static readonly VKError SerializationError = new("Caching.Serialization.Failed", "An error occurred during cache serialization or deserialization.");
}
