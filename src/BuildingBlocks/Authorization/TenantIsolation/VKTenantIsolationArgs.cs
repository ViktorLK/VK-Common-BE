using VK.Blocks.Core;

namespace VK.Blocks.Authorization;

/// <summary>
/// Arguments for tenant isolation evaluation.
/// Following Rule 21: Local overrides for the global <see cref="VKTenantIsolationOptions"/>.
/// </summary>
public sealed record VKTenantIsolationArgs : IVKArgs<VKTenantIsolationArgs>
{
    /// <summary>
    /// Gets an empty set of arguments (no overrides).
    /// </summary>
    public static VKTenantIsolationArgs Empty { get; } = new();

    /// <summary>
    /// Gets the tenant ID of the resource being accessed.
    /// If null, the implementation should resolve it automatically (e.g. from context).
    /// </summary>
    public string? TargetTenantId { get; init; }
}
