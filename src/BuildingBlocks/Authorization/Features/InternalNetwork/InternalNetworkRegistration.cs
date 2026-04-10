using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using VK.Blocks.Authorization.Features.InternalNetwork.Internal;

namespace VK.Blocks.Authorization.Features.InternalNetwork;

internal static class InternalNetworkRegistration
{
    public static IServiceCollection AddInternalNetworkFeature(this IServiceCollection services)
    {
        services.TryAddScoped<IIpAddressProvider, DefaultIpAddressProvider>();
        services.TryAddScoped<InternalNetworkAuthorizationHandler>();
        services.TryAddScoped<IInternalNetworkEvaluator>(sp => sp.GetRequiredService<InternalNetworkAuthorizationHandler>());
        
        return services;
    }
}
