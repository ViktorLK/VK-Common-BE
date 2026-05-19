using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Cognitive.Agents.Internal;

/// <summary>
/// Marker and registration hook for the Cognitive Agents feature.
/// Following BB.02 and BB.06.
/// </summary>
internal sealed partial class CognitiveAgentsFeature
{
    static partial void RegisterCustom(IServiceCollection services, VKCognitiveAgentsOptions options) // [SG Hook]
    {
        services.TryAddSingleton<IVKCognitiveAgentFactory, CognitiveAgentFactory>();
    }

    static partial void ValidateCustom(VKCognitiveAgentsOptions options, List<string> failures) // [SG Hook]
    {
        if (options.Enabled && options.DefaultPersonaId is not null)
        {
            VKGuard.NotNullOrWhiteSpace(options.DefaultPersonaId);
        }
    }
}
