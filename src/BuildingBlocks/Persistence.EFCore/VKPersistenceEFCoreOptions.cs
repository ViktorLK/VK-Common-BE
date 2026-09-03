using VK.Blocks.Core;

namespace VK.Blocks.Persistence.EFCore;

/// <summary>
/// Configuration options for the EF Core persistence layer.
/// </summary>
public sealed partial record VKPersistenceEFCoreOptions : IVKBlockOptions
{
    /// <summary>
    /// Gets a value indicating whether auditing is enabled.
    /// Default is true.
    /// </summary>
    public bool EnableAuditing { get; init; } = true;

    /// <summary>
    /// Gets a value indicating whether soft delete is enabled.
    /// Default is true.
    /// </summary>
    public bool EnableSoftDelete { get; init; } = true;

    /// <summary>
    /// Gets a value indicating whether automatic domain event dispatching is enabled on SaveChanges.
    /// Default is true.
    /// </summary>
    public bool EnableDomainEvents { get; init; } = true;

    /// <summary>
    /// Gets a value indicating whether outbox message storage is enabled.
    /// Default is false.
    /// </summary>
    public bool EnableOutbox { get; init; } = false;
}
