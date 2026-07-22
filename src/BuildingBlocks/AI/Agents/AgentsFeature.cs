using VK.Blocks.AI.Agents.Internal;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using VK.Blocks.Core;

namespace VK.Blocks.AI;

/// <summary>
/// Agents feature marker and registration hub.
/// </summary>
[VKFeature(typeof(global::VK.Blocks.AI.VKAIBlock), OptionsType = typeof(VKAgentsOptions), ArgsGenerationMode = VKArgsGenerationMode.Explicit)]
internal sealed partial class AgentsFeature
{
    // [SG Hook]
    static partial void RegisterFeatureCustom(IServiceCollection services, VKAgentsOptions options) =>
        services.TryAddSingleton<IVKAgentFactory, AgentsFactory>();

    /// <summary>Add agent-specific validation logic here</summary>
    // [SG Hook]
    static partial void ValidateFeatureCustom(VKAgentsOptions options, List<string> failures)
    {
        _ = options;
        _ = failures;
    }
}
