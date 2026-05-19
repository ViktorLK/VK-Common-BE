using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;

namespace VK.Blocks.AI.Cognitive.Reasoning.Internal;

/// <summary>
/// Feature marker and registration hook for the Reasoning feature.
/// Following BB.02 and BB.06.
/// </summary>
internal sealed partial class ReasoningFeature
{
    static partial void RegisterCustom(IServiceCollection services, VKReasoningOptions options) // [SG Hook]
    {
        // Placeholder for registering custom reasoning planners
    }

    static partial void ValidateCustom(VKReasoningOptions options, List<string> failures) // [SG Hook]
    {
        if (options.Enabled && options.MaxDepth <= 0)
        {
            failures.Add("Reasoning.MaxDepth must be a positive integer.");
        }
    }
}
