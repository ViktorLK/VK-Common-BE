using System;
using Microsoft.SemanticKernel;

namespace VK.Blocks.AI.SemanticKernel.Kernel.Internal;

internal static partial class AISKProviderRegistrar
{
    internal static void RegisterOllama(
        this IKernelBuilder builder,
        VKAISKOptions options,
        string modelId)
    {
        var endpoint = new Uri(options.Endpoint ?? "http://localhost:11434");
        builder.AddOllamaChatCompletion(modelId: modelId, endpoint: endpoint);
        builder.AddOllamaEmbeddingGenerator(modelId: modelId, endpoint: endpoint);
    }
}
