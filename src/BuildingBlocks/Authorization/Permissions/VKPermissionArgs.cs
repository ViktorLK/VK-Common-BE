using System.Collections.Generic;

using VK.Blocks.Core;

namespace VK.Blocks.Authorization;

/// <summary>
/// Arguments for permission evaluation.
/// Following AP.05: Local overrides for the global <see cref="VKPermissionOptions"/>.
/// </summary>
public sealed record VKPermissionArgs : IVKArgs<VKPermissionArgs>
{
    /// <summary>
    /// Gets an empty set of arguments (no overrides).
    /// </summary>
    public static VKPermissionArgs Empty { get; } = new();

    /// <summary>
    /// Gets the collection of permissions to check for.
    /// </summary>
    public IEnumerable<string>? Permissions { get; init; }

    /// <summary>
    /// Gets the evaluation mode (All/Any).
    /// If null, the value from global options or the default (All) is used.
    /// </summary>
    public VKPermissionEvaluationMode? Mode { get; init; }
}

