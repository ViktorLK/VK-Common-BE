using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace VK.Blocks.AI.Agents.Internal;

/// <summary>
/// Agents feature marker and registration hub.
/// </summary>
internal sealed partial class AgentsFeature
{
    // [SG Hook]
    static partial void RegisterCustom(IServiceCollection services, VKAgentsOptions options) =>
        services.TryAddSingleton<IVKAgentFactory, AgentsFactory>();

    /// <summary>Add agent-specific validation logic here</summary>
    // [SG Hook]
    static partial void ValidateCustom(VKAgentsOptions options, List<string> failures)
    {
        _ = options;
        _ = failures;
    }
}
