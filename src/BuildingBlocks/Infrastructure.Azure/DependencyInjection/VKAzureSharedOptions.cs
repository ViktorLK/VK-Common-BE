using VK.Blocks.Core;

namespace VK.Blocks.Infrastructure.Azure;

/// <summary>
/// Shared configuration model for Azure infrastructure.
/// </summary>
public sealed record VKAzureSharedOptions : IVKBlockOptions
{
    /// <inheritdoc />
    public static string SectionName => VKBlocksConstants.VKBlocksConfigPrefix + "Infrastructure:Azure";

    /// <inheritdoc />
    public bool Enabled { get; init; } = true;

    /// <summary>
    /// Gets the global connection string for Azure resources.
    /// </summary>
    public string? ConnectionString { get; init; }

    /// <summary>
    /// Gets the default Azure region.
    /// </summary>
    public string? Region { get; init; }

    /// <summary>
    /// Gets the shared identity configuration.
    /// </summary>
    public AzureIdentityOptions Identity { get; init; } = new();
}

/// <summary>
/// Identity configuration for Azure services.
/// </summary>
public sealed record AzureIdentityOptions
{
    public string? TenantId { get; init; }
    public string? ClientId { get; init; }
    public string? ClientSecret { get; init; }
    public bool UseManagedIdentity { get; init; } = false;
}
