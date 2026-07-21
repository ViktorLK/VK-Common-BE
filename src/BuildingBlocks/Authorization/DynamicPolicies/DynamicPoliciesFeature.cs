using VK.Blocks.Authorization.DynamicPolicies;
using VK.Blocks.Authorization.DynamicPolicies.Internal;
using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using VK.Blocks.Core;

namespace VK.Blocks.Authorization;

[ExcludeFromCodeCoverage]
[VKFeature(typeof(VKAuthorizationBlock), OptionsType = typeof(VKDynamicPoliciesOptions), ArgsGenerationMode = VKArgsGenerationMode.Explicit)]
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
