using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using VK.Blocks.Authentication.Tracking.Internal;
using VK.Blocks.Authentication.Tracking.Protocols;
using VK.Blocks.Core;

namespace VK.Blocks.Authentication;

/// <summary>
/// Feature registration hook for login state tracking and device fingerprinting.
/// </summary>
[VKFeature(typeof(VKAuthenticationBlock), OptionsType = typeof(VKTrackingOptions))]
internal sealed partial class TrackingFeature
{
    static partial void RegisterFeatureCustom(IServiceCollection services, VKTrackingOptions options)
    {
        // 1. Register Default Auditor implementation
        services.TryAddSingleton<IVKLoginTracker, DefaultLoginTracker>();

        // 2. Register Scoped helper for endpoint tracking
        services.TryAddScoped<LoginTrackingHelper>();

        // 3. Register HttpContext Accessor
        services.AddHttpContextAccessor();
    }
}
