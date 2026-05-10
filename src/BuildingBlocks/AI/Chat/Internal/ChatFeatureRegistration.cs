using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Chat.Internal;

/// <summary>
/// Handles the registration of the Chat feature.
/// </summary>
internal static class ChatFeatureRegistration
{
    public static IVKAIBuilder Register(IVKAIBuilder builder)
    {
        IServiceCollection services = builder.Services;

        // 1. Idempotency Check
        if (services.IsVKBlockRegistered<ChatFeature>())
        {
            return builder;
        }

        // 2. Options Registration
        // Note: Features use the parent block's configuration to resolve their sub-options.
        VKChatOptions options = services.AddVKBlockOptions<VKChatOptions>(builder.Configuration!);

        // 3. Mark-Self
        services.AddVKBlockMarker<ChatFeature>();

        // 4. Options Validation
        services.TryAddEnumerableSingleton<IValidateOptions<VKChatOptions>, ChatOptionsValidator>();

        // 5. Feature Toggle
        if (!options.Enabled)
        {
            return builder;
        }

        // 6. Core Services
        services.TryAddSingleton<IVKChatEngine, DefaultChatEngine>();

        return builder;
    }
}
