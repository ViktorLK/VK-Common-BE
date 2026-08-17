using VK.Blocks.Core;

namespace VK.Blocks.Caching;

/// <summary>
/// Domain-specific error constants for the Caching building block.
/// Follows CS.01: {ModuleName}.{Category}.{Reason} format.
/// </summary>
public static class VKCachingErrors
{
    public static class Operation
    {
        public static readonly VKError ReadFailed = VKError.Failure(
            "Caching.Operation.ReadFailed",
            "Failed to read item from the cache.");

        public static readonly VKError WriteFailed = VKError.Failure(
            "Caching.Operation.WriteFailed",
            "Failed to write item to the cache.");

        public static readonly VKError RemoveFailed = VKError.Failure(
            "Caching.Operation.RemoveFailed",
            "Failed to remove item from the cache.");

        public static readonly VKError EvictionFailed = VKError.Failure(
            "Caching.Operation.EvictionFailed",
            "Failed to evict items from the cache.");

        public static readonly VKError Unavailable = VKError.Failure(
            "Caching.Operation.Unavailable",
            "The caching service is currently unavailable.");
    }

    public static class Lock
    {
        public static readonly VKError AcquireFailed = VKError.Conflict(
            "Caching.Lock.AcquireFailed",
            "Failed to acquire distributed lock for the specified key.");

        public static readonly VKError ReleaseFailed = VKError.Failure(
            "Caching.Lock.ReleaseFailed",
            "Failed to release distributed lock for the specified key.");

        public static readonly VKError LockExpired = VKError.Conflict(
            "Caching.Lock.LockExpired",
            "The distributed lock has expired before operation completion.");
    }
}
