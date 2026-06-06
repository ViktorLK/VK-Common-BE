using System;
using System.Net.Http;
using Microsoft.SemanticKernel;

namespace VK.Blocks.AI.SemanticKernel.Common.Kernel.Internal;

internal static partial class AISKProviderRegistrar
{
    internal static void RegisterGoogleAIChat(
        this IKernelBuilder builder,
        VKAISKOptions aiskOptions,
        IVKAIProviderOptions connectionSettings,
        HttpClient? httpClient)
    {
        var modelId = connectionSettings.ModelId ?? string.Empty;
        var cleanModelId = modelId.Replace("models/", "", StringComparison.OrdinalIgnoreCase);
        builder.AddGoogleAIGeminiChatCompletion(cleanModelId, connectionSettings.ApiKey?.Reveal() ?? string.Empty, httpClient: httpClient);
    }

    internal static void RegisterGoogleAIEmbedding(
        this IKernelBuilder builder,
        VKAISKOptions aiskOptions,
        IVKAIProviderOptions connectionSettings,
        HttpClient? httpClient)
    {
        var modelId = connectionSettings.ModelId ?? string.Empty;
        var cleanModelId = modelId.Replace("models/", "", StringComparison.OrdinalIgnoreCase);
        builder.AddGoogleAIEmbeddingGenerator(cleanModelId, connectionSettings.ApiKey?.Reveal() ?? string.Empty, httpClient: httpClient);
    }
}
