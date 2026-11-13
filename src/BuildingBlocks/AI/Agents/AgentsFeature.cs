using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using VK.Blocks.AI.Agents.Internal;
using VK.Blocks.Core;

namespace VK.Blocks.AI;

/// <summary>
/// Agents feature marker and registration hub.
/// </summary>
[VKFeature(typeof(VKAIBlock), OptionsType = typeof(VKAgentsOptions))]
internal sealed partial class AgentsFeature
{
    // [SG Hook]
    static partial void RegisterFeatureCustom(IServiceCollection services, VKAgentsOptions options)
    {
        services.TryAddSingleton<IVKAgentFactory, NoOpAgentsFactory>();
    }

    // [SG Hook]
    static partial void ValidateFeatureCustom(VKAgentsOptions options, List<string> failures)
    {
        _ = options;
        _ = failures;
    }
}
