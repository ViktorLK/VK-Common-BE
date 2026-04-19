using Microsoft.Extensions.DependencyInjection;

namespace VK.Blocks.Core.DependencyInjection;

/// <summary>
/// Default implementation of the <see cref="IVKBlockBuilder{TMarker}"/> interface.
/// </summary>
/// <typeparam name="TMarker">A marker type representing the specific building block (implements IVKBlockMarker).</typeparam>
public sealed class VKBlockBuilder<TMarker>(IServiceCollection services) : IVKBlockBuilder<TMarker>
{
    /// <inheritdoc />
    public IServiceCollection Services { get; } = services;
}




