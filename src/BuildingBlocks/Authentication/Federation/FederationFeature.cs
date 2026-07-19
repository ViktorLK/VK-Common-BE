using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using VK.Blocks.Authentication.Federation.Internal;
using VK.Blocks.Authentication.Federation.Protocols;
using VK.Blocks.Core;

namespace VK.Blocks.Authentication;

/// <summary>
/// Feature registration hook for Identity Federation and Account Linking.
/// </summary>
[VKFeature(typeof(VKAuthenticationBlock), OptionsType = typeof(VKFederationOptions))]
internal sealed partial class FederationFeature
{
    static partial void RegisterFeatureCustom(IServiceCollection services, VKFederationOptions options)
    {
        // 1. Register default in-memory mapping fallback
        services.TryAddSingleton<IVKAccountLinkService, InMemoryAccountLinkService>();
    }
}
