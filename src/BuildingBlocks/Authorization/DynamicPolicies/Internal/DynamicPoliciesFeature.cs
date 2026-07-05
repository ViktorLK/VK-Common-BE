using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using VK.Blocks.Core;

namespace VK.Blocks.Authorization.DynamicPolicies.Internal;

[ExcludeFromCodeCoverage]
internal sealed partial class DynamicPoliciesFeature
{
    static partial void RegisterFeatureCustom(IServiceCollection services, VKDynamicPoliciesOptions options)
    {
        services.TryAddScoped<IVKDynamicPoliciesProvider, DefaultDynamicPoliciesProvider>();
        services.TryAddScoped<IVKDynamicPoliciesEvaluator, DefaultDynamicPoliciesEvaluator>();
        services.TryAddScoped<DynamicRequirementHandler>();
        services.TryAddEnumerableScopedForwarding<IAuthorizationHandler, DynamicRequirementHandler>();
    }
}
