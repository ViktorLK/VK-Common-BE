using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Tokenics.Internal;

/// <summary>
/// Handles the registration of the Tokenics feature.
/// </summary>
internal static class TokenicsFeatureRegistration
{
    public static IVKAIBuilder Register(IVKAIBuilder builder)
    {
        IServiceCollection services = builder.Services;

        // 1. Idempotency Check
        if (services.IsVKBlockRegistered<TokenicsFeature>())
        {
            return builder;
        }

        // 2. Options Registration
        VKTokenicsOptions options = services.AddVKBlockOptions<VKTokenicsOptions>(builder.Configuration!);

        // 3. Mark-Self
        services.AddVKBlockMarker<TokenicsFeature>();

        // 4. Options Validation
        services.TryAddEnumerableSingleton<IValidateOptions<VKTokenicsOptions>, TokenicsOptionsValidator>();

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
