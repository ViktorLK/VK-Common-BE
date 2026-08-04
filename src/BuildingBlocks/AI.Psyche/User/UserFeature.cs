using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using VK.Blocks.AI.Psyche.User.Internal;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Psyche;

/// <summary>
/// User profile feature marker and registration hub.
/// </summary>
[VKFeature(typeof(VKAIPsycheBlock), OptionsType = typeof(VKUserOptions), ArgsGenerationMode = VKArgsGenerationMode.Explicit)]
internal sealed partial class UserFeature
{
    static partial void RegisterFeatureCustom(IServiceCollection services, VKUserOptions options)
    {
        if (!options.Enabled)
            return;

        services.TryAddScoped<IVKUserStore, InMemoryUserStore>();
        services.AddScoped<IVKPsychePipelineStage, DefaultUserStage>();
    }

    // [SG Hook]
    static partial void ValidateFeatureCustom(VKUserOptions options, List<string> failures)
    {
        _ = options;
        _ = failures;
    }
}
