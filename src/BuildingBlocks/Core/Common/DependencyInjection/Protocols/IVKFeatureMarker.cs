namespace VK.Blocks.Core;

/// <summary>
/// Specialized marker for a sub-feature within a larger building block.
/// Features represent modular slices of functionality that can be independently enabled or disabled.
/// </summary>
/// <remarks>
/// Following the "Proxy Diagnostics" pattern, features typically share the telemetry source
/// of their parent block to avoid dashboard explosion in large-scale systems.
/// </remarks>
public interface IVKFeatureMarker : IVKBlockMarker
{
    /// <summary>
    /// Gets the identifier of the parent building block.
    /// Used for telemetry aggregation and discovery.
    /// </summary>
    string ParentBlockIdentifier { get; }

    /// <summary>
    /// Gets a value indicating whether this feature is optional within the block.
    /// Defaults to true.
    /// </summary>
    bool IsOptional => true;
}
