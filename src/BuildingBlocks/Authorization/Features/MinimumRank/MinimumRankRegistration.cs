using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using VK.Blocks.Authorization.Features.MinimumRank.Internal;

namespace VK.Blocks.Authorization.Features.MinimumRank;

internal static class MinimumRankRegistration
{
    public static IServiceCollection AddMinimumRankFeature(this IServiceCollection services)
    {
        services.TryAddScoped<IRankProvider, DefaultRankProvider>();
        services.TryAddScoped<MinimumRankAuthorizationHandler>();
        services.TryAddScoped<IMinimumRankEvaluator>(sp => sp.GetRequiredService<MinimumRankAuthorizationHandler>());
        
        return services;
    }
}
