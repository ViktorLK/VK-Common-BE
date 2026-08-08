using System;
using System.Net.Http;
using Microsoft.SemanticKernel;

namespace VK.Blocks.AI.SemanticKernel.Common.Kernel.Internal;

internal static partial class AISemanticKernelProviderRegistrar
{
    internal static void RegisterGoogleAIChat(
        this IKernelBuilder builder,
        VKAISemanticKernelOptions AISemanticKernelOptions,
        IVKAIProviderOptions connectionSettings,
        HttpClient? httpClient,
        string? serviceId = null)
    {
        var modelId = connectionSettings.ModelId ?? string.Empty;
        var cleanModelId = modelId.Replace("models/", "", StringComparison.OrdinalIgnoreCase);
        builder.AddGoogleAIGeminiChatCompletion(cleanModelId, connectionSettings.ApiKey?.Reveal() ?? string.Empty, serviceId: serviceId, httpClient: httpClient);
    }

    internal static void RegisterGoogleAIEmbedding(
        this IKernelBuilder builder,
        VKAISemanticKernelOptions AISemanticKernelOptions,
        IVKAIProviderOptions connectionSettings,
        HttpClient? httpClient)
    {
        var modelId = connectionSettings.ModelId ?? string.Empty;
        var cleanModelId = modelId.Replace("models/", "", StringComparison.OrdinalIgnoreCase);
        builder.AddGoogleAIEmbeddingGenerator(cleanModelId, connectionSettings.ApiKey?.Reveal() ?? string.Empty, httpClient: httpClient);
    }
}
