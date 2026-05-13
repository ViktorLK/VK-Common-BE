using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Embeddings.Internal;

/// <summary>
/// Handles the registration of the Embeddings feature.
/// </summary>
internal static class EmbeddingsFeatureRegistration
{
    public static IVKAIBuilder Register(
        IVKAIBuilder builder,
        Func<VKEmbeddingOptions, VKEmbeddingOptions>? transform = null)
    {
        IServiceCollection services = builder.Services;

        // 1. Idempotency Check
        if (services.IsVKBlockRegistered<EmbeddingsFeature>())
        {
            return builder;
        }

        // 2. Options Registration
        VKEmbeddingOptions options = services.AddVKBlockOptions<VKEmbeddingOptions>(builder.Configuration!, transform);

        // 3. Mark-Self
        services.AddVKBlockMarker<EmbeddingsFeature>();

        // 4. Options Validation
        services.TryAddEnumerableSingleton<IValidateOptions<VKEmbeddingOptions>, EmbeddingsOptionsValidator>();

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
