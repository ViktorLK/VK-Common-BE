using VK.Blocks.Core;

namespace VK.Blocks.Authorization;

/// <summary>
/// Arguments for role evaluation.
/// Following AP.05: Local overrides for the global <see cref="VKRoleOptions"/>.
/// </summary>
public sealed record VKRoleArgs : IVKArgs<VKRoleArgs>
{
    /// <summary>
    /// Gets an empty set of arguments (no overrides).
    /// </summary>
    public static VKRoleArgs Empty { get; } = new();

    /// <summary>
    /// Gets the collection of roles to check for.
    /// </summary>
    public string[]? Roles { get; init; }
}

