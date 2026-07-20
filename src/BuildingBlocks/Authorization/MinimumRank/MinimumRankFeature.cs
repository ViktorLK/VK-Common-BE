using VK.Blocks.Authorization.MinimumRank;
using VK.Blocks.Authorization.MinimumRank.Internal;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using VK.Blocks.Core;

namespace VK.Blocks.Authorization;

[ExcludeFromCodeCoverage]
[VKFeature(typeof(VKAuthorizationBlock), OptionsType = typeof(VKMinimumRankOptions), ArgsGenerationMode = VKArgsGenerationMode.Explicit)]
internal sealed partial class MinimumRankFeature
{
    static partial void RegisterFeatureCustom(IServiceCollection services, VKMinimumRankOptions options)
    {
        services.TryAddScoped<IVKRankProvider, DefaultRankProvider>();
        services.TryAddScoped<MinimumRankAuthorizationHandler>();
        services.TryAddEnumerableScopedForwarding<IAuthorizationHandler, MinimumRankAuthorizationHandler>();
        services.TryAddScopedForwarding<IVKMinimumRankEvaluator, MinimumRankAuthorizationHandler>();

        services.AddOptions<AuthorizationOptions>()
            .Configure((AuthorizationOptions authOptions) =>
            {
                authOptions.AddPolicy(VKAuthorizationPolicies.SeniorAndAbove, p =>
                    p.RequireAuthenticatedUser()
                     .RequireVKMinimumRank(VKEmployeeRank.Senior));
            });
    }

    static partial void ValidateFeatureCustom(VKMinimumRankOptions options, List<string> failures)
    {
        if (options.RankClaimType is not null && string.IsNullOrWhiteSpace(options.RankClaimType))
        {
            failures.Add("RankClaimType cannot be whitespace if provided.");
        }
    }
}
