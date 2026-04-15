using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using VK.Blocks.Authorization.Features.DynamicPolicies.Internal;

namespace VK.Blocks.Authorization.Features.DynamicPolicies;

/// <summary>
/// Provides extension methods for registering the Dynamic Policies authorization feature.
/// </summary>
internal static class DynamicPoliciesRegistration
{
    /// <summary>
    /// Adds the Dynamic Policies authorization feature to the specified <see cref="IServiceCollection"/>.
    /// </summary>
    /// <param name="services">The service collection to add services to.</param>
    /// <returns>The updated service collection.</returns>
    public static IServiceCollection AddDynamicPoliciesFeature(this IServiceCollection services)
    {
        services.TryAddScoped<IDynamicPolicyProvider, DefaultDynamicPolicyProvider>();
        services.TryAddScoped<IDynamicPolicyEvaluator, DefaultDynamicPolicyEvaluator>();

        return services;
    }
}
