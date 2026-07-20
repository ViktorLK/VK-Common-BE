using VK.Blocks.Authorization.Entitlements;
using VK.Blocks.Authorization.Entitlements.Internal;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;
using VK.Blocks.Core;

namespace VK.Blocks.Authorization;

[ExcludeFromCodeCoverage]
[VKFeature(typeof(VKAuthorizationBlock), OptionsType = typeof(VKEntitlementsOptions))]
internal sealed partial class EntitlementsFeature
{
    static partial void RegisterFeatureCustom(IServiceCollection services, VKEntitlementsOptions options)
    {
        // No custom services to register (handled by MultiTenancy integration block)
    }
}
