using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;

namespace VK.Blocks.Authorization.Entitlements.Internal;

[ExcludeFromCodeCoverage]
internal sealed partial class EntitlementsFeature
{
    static partial void RegisterFeatureCustom(IServiceCollection services, VKEntitlementsOptions options)
    {
        // No custom services to register (handled by MultiTenancy integration block)
    }
}
