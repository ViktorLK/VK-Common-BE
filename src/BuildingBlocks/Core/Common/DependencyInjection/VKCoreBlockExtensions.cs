using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using VK.Blocks.Core.DependencyInjection.Internal;

namespace VK.Blocks.Core;

/// <summary>
/// Extension methods for registering the core building block.
/// </summary>
public static class VKCoreBlockExtensions
{
    /// <summary>
    /// Adds the core building block services and marker.
    /// This should be called before other building blocks are registered.
    /// </summary>
    public static IServiceCollection AddVKCoreBlock(this IServiceCollection services, IConfiguration _)
        => CoreBlockRegistration.Register(services);
}
