using Microsoft.Extensions.DependencyInjection;

namespace VK.Blocks.Core.DependencyInjection;

/// <summary>
/// A generic builder for VK.Blocks modules to enable Fluent API configurations.
/// </summary>
/// <typeparam name="TMarker">A marker type representing the specific building block.</typeparam>
public interface IVKBlockBuilder<out TMarker>
{
    /// <summary>
    /// Gets the service collection.
    /// </summary>
    IServiceCollection Services { get; }
}
