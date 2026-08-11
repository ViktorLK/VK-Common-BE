using System.Diagnostics.CodeAnalysis;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using VK.Blocks.AI.Psyche.Session.Internal;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Psyche;

/// <summary>
/// Session thread feature marker and registration hub for AI.Psyche.
/// Follows BB.06 and AP.02.
/// </summary>
[ExcludeFromCodeCoverage(Justification = "Feature marker and DI registration hub containing no business logic.")]
[VKFeature(typeof(VKAIPsycheBlock), OptionsType = typeof(VKSessionOptions), ArgsGenerationMode = VKArgsGenerationMode.Explicit)]
internal sealed partial class SessionFeature
{
    // [SG Hook]
    static partial void RegisterFeatureCustom(IServiceCollection services, VKSessionOptions options)
    {
        if (!options.Enabled)
            return;

        services.TryAddScoped<IVKSessionStore, InMemorySessionStore>();
        services.AddScoped<IVKPsychePipelineStage, DefaultSessionResolveStage>();
        services.AddScoped<IVKPsychePipelineStage, DefaultSessionUpdateStage>();
    }

    // [SG Hook]
    static partial void ValidateFeatureCustom(VKSessionOptions options, List<string> failures)
    {
        _ = options;
        _ = failures;
    }
}
