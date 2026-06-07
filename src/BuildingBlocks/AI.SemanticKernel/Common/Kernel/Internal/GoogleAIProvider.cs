using System;
using System.Net.Http;
using Microsoft.SemanticKernel;

using VK.Blocks.AI.SemanticKernel.Common.DependencyInjection;

namespace VK.Blocks.AI.SemanticKernel.Common.Kernel.Internal;

internal static partial class AISKProviderRegistrar
{
    internal static void RegisterGoogleAIChat(
        this IKernelBuilder builder,
        VKAISKDefaultsOptions aiskOptions,
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
        VKAISKDefaultsOptions aiskOptions,
        IVKAIProviderOptions connectionSettings,
        HttpClient? httpClient)
    {
        var modelId = connectionSettings.ModelId ?? string.Empty;
        var cleanModelId = modelId.Replace("models/", "", StringComparison.OrdinalIgnoreCase);
        builder.AddGoogleAIEmbeddingGenerator(cleanModelId, connectionSettings.ApiKey?.Reveal() ?? string.Empty, httpClient: httpClient);
    }
}


