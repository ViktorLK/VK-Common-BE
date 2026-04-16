namespace VK.Blocks.Core.Diagnostics;

/// <summary>
/// Defines a provider that exposes security metadata for discovery and diagnostics.
/// </summary>
public interface ISecurityMetadataProvider
{
    /// <summary>
    /// Gets the name of the module.
    /// </summary>
    string Module { get; }

    /// <summary>
    /// Gets the unified security topology provided by this module.
    /// </summary>
    /// <param name="serviceProvider">The service provider for resolving runtime dependencies.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A security topology object containing all metadata.</returns>
    ValueTask<SecurityTopology> GetSecurityTopologyAsync(IServiceProvider serviceProvider, CancellationToken cancellationToken);
}
