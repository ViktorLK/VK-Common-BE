using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using VK.Blocks.Authorization.Features.InternalNetwork.Internal;

namespace VK.Blocks.Authorization.Features.InternalNetwork;

/// <summary>
/// Provides extension methods for registering the Internal Network authorization feature.
/// </summary>
internal static class InternalNetworkRegistration
{
    /// <summary>
    /// Adds the Internal Network authorization feature to the specified <see cref="IServiceCollection"/>.
    /// </summary>
    /// <param name="services">The service collection to add services to.</param>
    /// <returns>The updated service collection.</returns>
    public static IServiceCollection AddInternalNetworkFeature(this IServiceCollection services)
    {
        services.TryAddScoped<IIpAddressProvider, DefaultIpAddressProvider>();
        services.TryAddScoped<InternalNetworkAuthorizationHandler>();
        services.TryAddScoped<IInternalNetworkEvaluator>(sp => sp.GetRequiredService<InternalNetworkAuthorizationHandler>());

        return services;
    }
}
