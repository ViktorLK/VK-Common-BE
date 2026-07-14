using VK.Blocks.Core;

namespace VK.Blocks.Persistence.EFCore;

/// <summary>
/// Configuration options for the EF Core persistence layer.
/// </summary>
public sealed partial record VKPersistenceEFCoreOptions : IVKBlockOptions
{
    /// <summary>
    /// Gets a value indicating whether auditing is enabled.
    /// If null, falls back to the global Persistence options.
    /// </summary>
    public bool? EnableAuditing { get; init; }

    /// <summary>
    /// Gets a value indicating whether soft delete is enabled.
    /// If null, falls back to the global Persistence options.
    /// </summary>
    public bool? EnableSoftDelete { get; init; }

    /// <summary>
    /// Gets a value indicating whether multi-tenancy is enabled.
    /// If null, falls back to the global Persistence options.
    /// </summary>
    public bool? EnableMultiTenancy { get; init; }

}
