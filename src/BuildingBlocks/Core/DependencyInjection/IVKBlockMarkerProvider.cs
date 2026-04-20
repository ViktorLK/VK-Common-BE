namespace VK.Blocks.Core.DependencyInjection;

/// <summary>
/// Defines a provider for a building block marker instance via a static abstract member.
/// Usually implemented by Source Generators to bridge from a generic type parameter to a singleton instance.
/// </summary>
/// <typeparam name="TSelf">The concrete marker type.</typeparam>
public interface IVKBlockMarkerProvider<TSelf> where TSelf : IVKBlockMarkerProvider<TSelf>
{
    /// <summary>
    /// Gets the static singleton instance of the building block marker.
    /// </summary>
    static abstract IVKBlockMarker Instance { get; }
}
