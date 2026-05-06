using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace VK.Blocks.Core;

/// <summary>
/// Default implementation of the <see cref="IVKBlockBuilder{TMarker}"/> interface.
/// </summary>
/// <typeparam name="TMarker">A marker type representing the specific building block (implements IVKBlockMarker).</typeparam>
/// <remarks>
/// This class is intentionally not declared as <c>sealed</c> because it serves as a base class 
/// for specialized builders in other modules (e.g., AuthenticationBlockBuilder) to share 
/// common <see cref="Services"/> and <see cref="Configuration"/> logic (Rule 12).
/// </remarks>
public class VKBlockBuilder<TMarker>(IServiceCollection services, IConfiguration configuration) : IVKBlockBuilder<TMarker>
{
    /// <inheritdoc />
    public IServiceCollection Services { get; } = services;

    /// <inheritdoc />
    public IConfiguration Configuration { get; } = configuration;
}
