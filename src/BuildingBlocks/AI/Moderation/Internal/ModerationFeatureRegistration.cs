using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Moderation.Internal;

/// <summary>
/// Handles the registration of the Moderation feature.
/// </summary>
internal static class ModerationFeatureRegistration
{
    public static IVKAIBuilder Register(IVKAIBuilder builder)
    {
        IServiceCollection services = builder.Services;

        // 1. Idempotency Check
        if (services.IsVKBlockRegistered<ModerationFeature>())
        {
            return builder;
        }

        // 2. Options Registration
        VKModerationOptions options = services.AddVKBlockOptions<VKModerationOptions>(builder.Configuration!);

        // 3. Mark-Self
        services.AddVKBlockMarker<ModerationFeature>();

        // 4. Options Validation
        services.TryAddEnumerableSingleton<IValidateOptions<VKModerationOptions>, ModerationOptionsValidator>();

        // 5. Feature Toggle
        if (!options.Enabled)
        {
            return builder;
        }

        // 6. Core Services
        // Implementations would go here.

        return builder;
    }
}
