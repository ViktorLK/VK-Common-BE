using System;
using Microsoft.SemanticKernel;

namespace VK.Blocks.AI.SemanticKernel.Common.Kernel.Internal;

internal static partial class AISemanticKernelProviderRegistrar
{
    internal static void RegisterOllamaChat(
        this IKernelBuilder builder,
        VKAISemanticKernelOptions AISemanticKernelOptions,
        IVKAIProviderOptions connectionSettings,
        string? serviceId = null)
    {
        var endpoint = new Uri(connectionSettings.Endpoint ?? "http://localhost:11434");
        var modelId = connectionSettings.ModelId ?? string.Empty;
        builder.AddOllamaChatCompletion(modelId: modelId, endpoint: endpoint, serviceId: serviceId);
    }

    internal static void RegisterOllamaEmbedding(
        this IKernelBuilder builder,
        VKAISemanticKernelOptions AISemanticKernelOptions,
        IVKAIProviderOptions connectionSettings)
    {
        var endpoint = new Uri(connectionSettings.Endpoint ?? "http://localhost:11434");
        var modelId = connectionSettings.ModelId ?? string.Empty;
        builder.AddOllamaEmbeddingGenerator(modelId: modelId, endpoint: endpoint);
    }
}
