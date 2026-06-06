using System;
using System.Net.Http;
using Microsoft.SemanticKernel;

namespace VK.Blocks.AI.SemanticKernel.Common.Kernel.Internal;

internal static partial class AISKProviderRegistrar
{
    internal static void RegisterOpenAIChat(
        this IKernelBuilder builder,
        VKAISKOptions aiskOptions,
        IVKAIProviderOptions connectionSettings,
        HttpClient? httpClient)
    {
        var modelId = connectionSettings.ModelId ?? string.Empty;

        builder.AddOpenAIChatCompletion(
            modelId: modelId,
            apiKey: connectionSettings.ApiKey?.Reveal() ?? string.Empty,
            orgId: aiskOptions.OrgId,
            httpClient: httpClient);
    }

    internal static void RegisterOpenAIEmbedding(
        this IKernelBuilder builder,
        VKAISKOptions aiskOptions,
        IVKAIProviderOptions connectionSettings,
        HttpClient? httpClient)
    {
        var modelId = connectionSettings.ModelId ?? string.Empty;

        builder.AddOpenAIEmbeddingGenerator(
            modelId,
            connectionSettings.ApiKey?.Reveal() ?? throw new InvalidOperationException("ApiKey is required for OpenAI"),
            orgId: aiskOptions.OrgId,
            httpClient: httpClient);
    }
}
