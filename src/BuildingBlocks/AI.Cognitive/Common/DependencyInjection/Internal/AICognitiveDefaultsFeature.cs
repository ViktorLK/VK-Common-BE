using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;

namespace VK.Blocks.AI.Cognitive.Common.DependencyInjection.Internal;

/// <summary>
/// Partial implementation for AI Cognitive Defaults feature hooks.
/// </summary>
internal sealed partial class AICognitiveDefaultsFeature
{
    // [SG Hook]
    static partial void RegisterCustom(IServiceCollection services, VKAICognitiveDefaultsOptions options)
    {
        // Add shared cognitive defaults infrastructure here if needed
    }

    // [SG Hook]
    static partial void ValidateCustom(VKAICognitiveDefaultsOptions options, List<string> failures)
    {
        if (options.DefaultMinScore is < 0f or > 1f)
        {
            failures.Add("CognitiveDefaults.DefaultMinScore must be between 0.0 and 1.0.");
        }

        if (options.ConfidenceThreshold is < 0.0 or > 1.0)
        {
            failures.Add("CognitiveDefaults.ConfidenceThreshold must be between 0.0 and 1.0.");
        }

        if (options.MaxDepth <= 0)
        {
            failures.Add("CognitiveDefaults.MaxDepth must be a positive integer.");
        }
    }
}
