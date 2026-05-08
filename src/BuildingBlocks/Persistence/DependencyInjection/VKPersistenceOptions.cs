using VK.Blocks.Core;

namespace VK.Blocks.Persistence;

/// <summary>
/// Configuration options for the Persistence building block.
/// </summary>
public sealed record VKPersistenceOptions : IVKBlockOptions
{
    /// <summary>
    /// The configuration section name.
    /// </summary>
    public static string SectionName => $"{VKBlocksConstants.VKBlocksConfigPrefix}:Persistence";

    /// <summary>
    /// Gets or sets a value indicating whether the Persistence block is enabled.
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
    /// Gets a value indicating whether multi-tenancy is enabled.
    /// Default is <c>false</c>.
    /// </summary>
    public bool EnableMultiTenancy { get; init; } = false;
}
