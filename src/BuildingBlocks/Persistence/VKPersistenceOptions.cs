using VK.Blocks.Core;

namespace VK.Blocks.Persistence;

public sealed partial record VKPersistenceOptions : IVKToggleableBlockOptions
{
    /// <summary>
    /// Gets or sets a value indicating whether the block is enabled.
    /// Default is <c>true</c>.
    /// </summary>
    public bool Enabled { get; init; } = true;

    /// <summary>
    /// Gets a value indicating whether auditing is enabled.
    /// Default is <c>true</c>.
    /// </summary>
    public bool EnableAuditing { get; init; } = true;

    /// <summary>
    /// Gets a value indicating whether soft delete is enabled.
    /// Default is <c>true</c>.
    /// </summary>
    public bool EnableSoftDelete { get; init; } = true;

    /// <summary>
    /// Default command timeout in seconds. Providers MUST respect this value.
    /// Default is 30 seconds.
    /// </summary>
    public int DefaultCommandTimeoutSeconds { get; init; } = 30;

    /// <summary>
    /// Default page size for pagination when not explicitly specified.
    /// Default is 20.
    /// </summary>
    public int DefaultPageSize { get; init; } = 20;

    /// <summary>
    /// Maximum allowed page size to prevent memory exhaustion attacks.
    /// Default is 200.
    /// </summary>
    public int MaxPageSize { get; init; } = 200;

    /// <summary>
    /// Default query tracking behavior.
    /// Default is <see cref="VKQueryTracking.NoTracking"/> (safe default).
    /// </summary>
    public VKQueryTracking DefaultTracking { get; init; } = VKQueryTracking.NoTracking;

    /// <summary>
    /// Maximum number of retries for concurrency resolution.
    /// Default is 3.
    /// </summary>
    public int ConcurrencyRetryCount { get; init; } = 3;
}
