using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using VK.Blocks.Core;

namespace VK.Blocks.AI.VectorStore.Retrieval.Internal;

/// <summary>
/// Handles the registration of the Retrieval feature.
/// </summary>
internal static class RetrievalFeatureRegistration
{
    public static IVKAIVectorStoreBuilder Register(IVKAIVectorStoreBuilder builder)
    {
        IServiceCollection services = builder.Services;

        // 1. Idempotency Check
        if (services.IsVKBlockRegistered<RetrievalFeature>())
        {
            return builder;
        }

        // 2. Options Registration
        VKRetrievalOptions options = services.AddVKBlockOptions<VKRetrievalOptions>(builder.Configuration);

        // 3. Mark-Self
        services.AddVKBlockMarker<RetrievalFeature>();

        // 4. Options Validation
        // services.TryAddEnumerableSingleton<IValidateOptions<VKRetrievalOptions>, RetrievalOptionsValidator>();

        // 5. Feature Toggle
        if (!options.Enabled)
        {
            return builder;
        }

        // 6. Core Services
        services.TryAddScoped<IVKRetrievalStore, VKVectorStoreRagEngine>();
        services.TryAddSingleton<IVKDocumentLoader, VKDocumentLoader>();

        return builder;
    }
}
