using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace VK.Blocks.Core;

/// <summary>
/// Default implementation of the <see cref="IVKBlockBuilder{TMarker}"/> interface.
/// </summary>
/// <typeparam name="TMarker">A marker type representing the specific building block (implements IVKBlockMarker).</typeparam>
public class VKBlockBuilder<TMarker>(IServiceCollection services, IConfiguration configuration) : IVKBlockBuilder<TMarker>
{
    /// <inheritdoc />
    public IServiceCollection Services { get; } = services;

    /// <inheritdoc />
    public IConfiguration Configuration { get; } = configuration;
}
