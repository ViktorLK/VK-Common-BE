namespace VK.Blocks.Persistence;

/// <summary>
/// Specifies change tracking behavior for query operations.
/// Providers that do not support change tracking (e.g., Cosmos) MUST ignore this hint.
/// </summary>
public enum VKQueryTracking : byte
{
    /// <summary>
    /// Default behavior defined by the provider (typically NoTracking for read repositories, Tracked for write repositories).
    /// </summary>
    Default = 0,

    /// <summary>
    /// Explicitly disable change tracking (e.g., EF Core AsNoTracking).
    /// </summary>
    NoTracking = 1,

    /// <summary>
    /// Explicitly enable change tracking (e.g., EF Core default behavior).
    /// </summary>
    Tracked = 2,
}
