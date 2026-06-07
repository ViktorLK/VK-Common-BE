using System;
using Microsoft.SemanticKernel;

using VK.Blocks.AI.SemanticKernel.Common.DependencyInjection;

namespace VK.Blocks.AI.SemanticKernel.Common.Kernel.Internal;

internal static partial class AISKProviderRegistrar
{
    internal static void RegisterOllamaChat(
        this IKernelBuilder builder,
        VKAISKDefaultsOptions aiskOptions,
        IVKAIProviderOptions connectionSettings,
        string? serviceId = null)
    {
        var endpoint = new Uri(connectionSettings.Endpoint ?? "http://localhost:11434");
        var modelId = connectionSettings.ModelId ?? string.Empty;
        builder.AddOllamaChatCompletion(modelId: modelId, endpoint: endpoint, serviceId: serviceId);
    }

    internal static void RegisterOllamaEmbedding(
        this IKernelBuilder builder,
        VKAISKDefaultsOptions aiskOptions,
        IVKAIProviderOptions connectionSettings)
    {
        var endpoint = new Uri(connectionSettings.Endpoint ?? "http://localhost:11434");
        var modelId = connectionSettings.ModelId ?? string.Empty;
        builder.AddOllamaEmbeddingGenerator(modelId: modelId, endpoint: endpoint);
    }
}


