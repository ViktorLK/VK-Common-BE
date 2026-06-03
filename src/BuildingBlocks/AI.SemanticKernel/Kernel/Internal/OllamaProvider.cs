using System;
using Microsoft.SemanticKernel;

namespace VK.Blocks.AI.SemanticKernel.Kernel.Internal;

internal static partial class AISKProviderRegistrar
{
    internal static void RegisterOllamaChat(
        this IKernelBuilder builder,
        VKAISKOptions aiskOptions,
        IVKAIProviderOptions connectionSettings)
    {
        var endpoint = new Uri(connectionSettings.Endpoint ?? "http://localhost:11434");
        var modelId = connectionSettings.ModelId ?? string.Empty;
        builder.AddOllamaChatCompletion(modelId: modelId, endpoint: endpoint);
    }

    internal static void RegisterOllamaEmbedding(
        this IKernelBuilder builder,
        VKAISKOptions aiskOptions,
        IVKAIProviderOptions connectionSettings)
    {
        var endpoint = new Uri(connectionSettings.Endpoint ?? "http://localhost:11434");
        var modelId = connectionSettings.ModelId ?? string.Empty;
        builder.AddOllamaEmbeddingGenerator(modelId: modelId, endpoint: endpoint);
    }
}
