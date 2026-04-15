using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using VK.Blocks.Authorization.Features.MinimumRank.Internal;

namespace VK.Blocks.Authorization.Features.MinimumRank;

/// <summary>
/// Provides extension methods for registering the Minimum Rank authorization feature.
/// </summary>
internal static class MinimumRankRegistration
{
    /// <summary>
    /// Adds the Minimum Rank authorization feature to the specified <see cref="IServiceCollection"/>.
    /// </summary>
    /// <param name="services">The service collection to add services to.</param>
    /// <returns>The updated service collection.</returns>
    public static IServiceCollection AddMinimumRankFeature(this IServiceCollection services)
    {
        services.TryAddScoped<IRankProvider, DefaultRankProvider>();
        services.TryAddScoped<MinimumRankAuthorizationHandler>();
        services.TryAddScoped<IMinimumRankEvaluator>(sp => sp.GetRequiredService<MinimumRankAuthorizationHandler>());

        return services;
    }
}
