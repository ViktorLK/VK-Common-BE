using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using VK.Blocks.AI.MEAI.Common.Shared;
using VK.Blocks.AI.MEAI.Embeddings.Internal;
using VK.Blocks.Core;

namespace VK.Blocks.AI.MEAI;

/// <summary>
/// Embeddings feature marker and registration hub for VK.Blocks.AI.MEAI.
/// </summary>
// [SG Marker]
[VKFeature(typeof(VKAIMEAIBlock), OptionsType = typeof(VKAIMEAIEmbeddingsOptions))]
internal sealed partial class EmbeddingsFeature
{
    // [SG Hook]
    static partial void RegisterFeatureCustom(IServiceCollection services, VKAIMEAIEmbeddingsOptions options)
    {
        _ = options;
        VKAIMEAIBlock.TryAddDefaultAndKeyedClient<IEmbeddingGenerator<string, Embedding<float>>>(
            services,
            static (providerOptions, rootOptions, sp) =>
                AIMEAIClientFactory.CreateEmbeddingGenerator(providerOptions, rootOptions.ChainSettings, sp));

        services.Replace(ServiceDescriptor.Scoped<IVKEmbeddingsEngine, DefaultEmbeddingsEngine>());
    }
}
