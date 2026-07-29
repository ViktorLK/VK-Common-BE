using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using VK.Blocks.AI.Engram.Scoring.Internal;
using VK.Blocks.AI.Engram.Scoring.Internal.Rules;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Engram;

/// <summary>
/// Scoring feature marker and registration hub.
/// </summary>
[VKFeature(typeof(VKAIEngramBlock), OptionsType = typeof(VKScoringOptions), ArgsGenerationMode = VKArgsGenerationMode.Explicit)]
internal sealed partial class ScoringFeature
{
    // [SG Hook]
    static partial void RegisterFeatureCustom(IServiceCollection services, VKScoringOptions options)
    {
        // Pluggable rules for RuleBasedScoringStrategy
        services.TryAddEnumerable(ServiceDescriptor.Scoped<IVKScoringRule, SensitiveCredentialRule>());

        // Registered scoring tasks for job pipeline execution
        services.TryAddEnumerable(ServiceDescriptor.Scoped<IVKScoringTask, RuleBasedScoringTask>());
        services.TryAddEnumerable(ServiceDescriptor.Scoped<IVKScoringTask, EmotionalImpactScoringTask>());
        services.TryAddEnumerable(ServiceDescriptor.Scoped<IVKScoringTask, LlmHeuristicScoringTask>());
        services.TryAddEnumerable(ServiceDescriptor.Scoped<IVKScoringTask, NullScoringTask>());

        // Primary scoring job entrypoint resolves to DefaultScoringJob
        services.TryAddScoped<IVKScoringJob, DefaultScoringJob>();

        services.TryAddScoped<IVKScoreOverrideService, DefaultScoreOverrideService>();
    }

    // [SG Hook]
    static partial void ValidateFeatureCustom(VKScoringOptions options, List<string> failures)
    {
        VKGuard.NotNull(options);
        VKGuard.NotNull(failures);

        if (options.L1DefaultImportance is < 0.0 or > 1.0)
        {
            failures.Add("VKScoringOptions.L1DefaultImportance must be between 0.0 and 1.0.");
        }

        if (options.L2DefaultImportance is < 0.0 or > 1.0)
        {
            failures.Add("VKScoringOptions.L2DefaultImportance must be between 0.0 and 1.0.");
        }

        if (options.L3DefaultImportance is < 0.0 or > 1.0)
        {
            failures.Add("VKScoringOptions.L3DefaultImportance must be between 0.0 and 1.0.");
        }
    }
}
