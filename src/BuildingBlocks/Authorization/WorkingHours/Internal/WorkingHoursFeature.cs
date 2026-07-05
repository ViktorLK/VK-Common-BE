using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using VK.Blocks.Core;

namespace VK.Blocks.Authorization.WorkingHours.Internal;

[ExcludeFromCodeCoverage]
internal sealed partial class WorkingHoursFeature
{
    static partial void RegisterFeatureCustom(IServiceCollection services, VKWorkingHoursOptions options)
    {
        services.TryAddScoped<IVKWorkingHoursProvider, DefaultWorkingHoursProvider>();
        services.TryAddScoped<WorkingHoursAuthorizationHandler>();
        services.TryAddEnumerableScopedForwarding<IAuthorizationHandler, WorkingHoursAuthorizationHandler>();
        services.TryAddScopedForwarding<IVKWorkingHoursEvaluator, WorkingHoursAuthorizationHandler>();

        services.AddOptions<AuthorizationOptions>()
            .Configure((AuthorizationOptions authOptions) =>
            {
                authOptions.AddPolicy(VKAuthorizationPolicies.WorkingHoursOnly, p =>
                    p.RequireVKWorkingHours(options.WorkStart, options.WorkEnd));
            });
    }

    static partial void ValidateFeatureCustom(VKWorkingHoursOptions options, List<string> failures)
    {
        if (options.WorkStart >= options.WorkEnd)
        {
            failures.Add($"WorkStart ({options.WorkStart}) must be earlier than WorkEnd ({options.WorkEnd}).");
        }
    }
}
