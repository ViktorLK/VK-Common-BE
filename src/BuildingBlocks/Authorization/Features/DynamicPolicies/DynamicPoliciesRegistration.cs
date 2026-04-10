using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using VK.Blocks.Authorization.Features.DynamicPolicies.Internal;

namespace VK.Blocks.Authorization.Features.DynamicPolicies;

internal static class DynamicPoliciesRegistration
{
    public static IServiceCollection AddDynamicPoliciesFeature(this IServiceCollection services)
    {
        services.TryAddScoped<IDynamicPolicyProvider, DefaultDynamicPolicyProvider>();
        services.TryAddScoped<IDynamicPolicyEvaluator, DefaultDynamicPolicyEvaluator>();
        
        return services;
    }
}
