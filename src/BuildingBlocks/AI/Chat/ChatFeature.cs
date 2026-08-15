using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using VK.Blocks.AI.Chat.Internal;
using VK.Blocks.Core;

namespace VK.Blocks.AI;

/// <summary>
/// Chat feature marker and registration hub.
/// </summary>
[VKFeature(typeof(VKAIBlock), OptionsType = typeof(VKChatOptions), ArgsGenerationMode = VKArgsGenerationMode.Implicit, ArgsBaseType = typeof(IVKAIArgs))]
internal sealed partial class ChatFeature
{
    // [SG Hook]
    static partial void RegisterFeatureCustom(IServiceCollection services, VKChatOptions options)
    {
        _ = options;
        services.TryAddScoped<IVKChatEngine, NoOpVKChatEngine>();
        services.TryAddScoped<IVKChat, BasicChat>();
    }

    // [SG Hook] Optional validation hook
    static partial void ValidateFeatureCustom(VKChatOptions options, List<string> failures)
    {
        if (options.MaxAutoToolRounds < 1)
        {
            failures.Add("MaxAutoToolRounds must be at least 1.");
        }

        if (options.Temperature is < 0.0f or > 2.0f)
        {
            failures.Add("Temperature must be between 0.0 and 2.0.");
        }

        if (options.TopP is < 0.0f or > 1.0f)
        {
            failures.Add("TopP must be between 0.0 and 1.0.");
        }
    }
}
