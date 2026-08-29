using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using VK.Blocks.AI.Psyche.Profile.Internal;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Psyche;

/// <summary>
/// Profile feature marker and registration hub.
/// </summary>
[ExcludeFromCodeCoverage(Justification = "Feature marker and DI registration hub containing no business logic.")]
[VKFeature(typeof(VKAIPsycheBlock), OptionsType = typeof(VKProfileOptions), ArgsGenerationMode = VKArgsGenerationMode.Explicit)]
internal sealed partial class ProfileFeature
{
    static partial void RegisterFeatureCustom(IServiceCollection services, VKProfileOptions options)
    {
        if (!options.Enabled)
            return;

        services.TryAddScoped<InMemoryProfileStore>();
        services.TryAddScoped<IVKPsycheProfileRepository>(sp => sp.GetRequiredService<InMemoryProfileStore>());
        services.TryAddScoped<IVKReadRepository<VKProfilePresence, VKProfileId>>(sp => sp.GetRequiredService<InMemoryProfileStore>());
        services.AddScoped<IVKPsychePipelineStage, DefaultProfileStage>();
    }

    // [SG Hook]
    static partial void ValidateFeatureCustom(VKProfileOptions options, List<string> failures)
    {
        _ = options;
        _ = failures;
    }
}
