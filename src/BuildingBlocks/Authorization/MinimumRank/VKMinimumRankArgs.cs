using System;

using VK.Blocks.Core;

namespace VK.Blocks.Authorization;

/// <summary>
/// Arguments for minimum rank evaluation.
/// Following AP.05: Local overrides for the global <see cref="VKMinimumRankOptions"/>.
/// </summary>
public sealed record VKMinimumRankArgs : IVKArgs<VKMinimumRankArgs>
{
    /// <summary>
    /// Gets an empty set of arguments (no overrides).
    /// </summary>
    public static VKMinimumRankArgs Empty { get; } = new();

    /// <summary>
    /// Gets the required minimum rank value.
    /// </summary>
    public int? MinimumRank { get; init; }

    /// <summary>
    /// Gets the Type of the rank enum for parsing string claims.
    /// If null, the value from global options is used.
    /// </summary>
    public Type? EnumType { get; init; }
}

