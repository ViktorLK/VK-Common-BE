using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace VK.Blocks.AI.Chat.Internal;

/// <summary>
/// Chat feature marker and registration hub.
/// </summary>
internal sealed partial class ChatFeature
{
    // [SG Hook]
    static partial void RegisterCustom(IServiceCollection services, VKChatOptions options)
    {
        _ = options;
        services.TryAddScoped<IVKChatEngine, NoOpVKChatEngine>();
        services.TryAddScoped<IVKChat, BasicChat>();
        services.TryAddScoped<VK.Blocks.AI.IVKChatOptionsProvider, ChatDefaultOptionsProvider>();
    }

    // [SG Hook] Optional validation hook
    static partial void ValidateCustom(VKChatOptions options, List<string> failures)
    {
        if (string.IsNullOrWhiteSpace(options.ModelId))
        {
            failures.Add("ModelId is required when Chat is enabled.");
        }

        if (options.Timeout.HasValue && options.Timeout.Value <= TimeSpan.Zero)
        {
            failures.Add("Timeout must be greater than zero.");
        }

        if (options.RetryCount.HasValue && options.RetryCount.Value < 0)
        {
            failures.Add("RetryCount must be greater than or equal to 0.");
        }
    }
}
