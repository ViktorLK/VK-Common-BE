using System;

namespace VK.Blocks.Persistence;

/// <summary>
/// ORM-agnostic query hints. Providers MUST ignore unsupported options silently.
/// </summary>
public sealed record VKQueryOptions
{
    /// <summary>
    /// A shared default instance with all options at their default values.
    /// </summary>
    public static readonly VKQueryOptions Default = new();

    /// <summary>
    /// Change tracking behavior. Default is <see cref="VKQueryTracking.Default"/> (provider decides).
    /// </summary>
    public VKQueryTracking Tracking { get; init; } = VKQueryTracking.Default;

    /// <summary>
    /// Per-query timeout override. <c>null</c> means use the provider's default timeout.
    /// </summary>
    public TimeSpan? Timeout { get; init; }


    /// <summary>
    /// Diagnostic tag attached to the query for tracing or logging purposes.
    /// Example: EF Core translates this to TagWith().
    /// </summary>
    public string? QueryTag { get; init; }

    /// <summary>
    /// When <c>true</c>, uses split queries for related data loading.
    /// Only meaningful for providers with JOIN-based includes (like EF Core).
    /// </summary>
    public bool SplitQuery { get; init; }

    /// <summary>
    /// When <c>true</c>, hints the provider to route this query to a read replica.
    /// Only meaningful for providers that support read/write splitting.
    /// Default is <c>false</c>.
    /// </summary>
    public bool UseReadReplica { get; init; }
}
