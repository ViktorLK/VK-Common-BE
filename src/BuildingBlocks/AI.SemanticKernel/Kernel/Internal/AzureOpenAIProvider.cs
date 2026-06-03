using System;
using System.Net.Http;
using Microsoft.SemanticKernel;

namespace VK.Blocks.AI.SemanticKernel.Kernel.Internal;

internal static partial class AISKProviderRegistrar
{
    internal static void RegisterAzureOpenAIChat(
        this IKernelBuilder builder,
        VKAISKOptions aiskOptions,
        IVKAIProviderOptions connectionSettings,
        HttpClient? httpClient)
    {
        if (string.IsNullOrWhiteSpace(connectionSettings.Endpoint))
            throw new InvalidOperationException("Endpoint is required for AzureOpenAI");

        var modelId = connectionSettings.ModelId ?? string.Empty;

        builder.AddAzureOpenAIChatCompletion(
            deploymentName: aiskOptions.DeploymentName ?? modelId,
            endpoint: connectionSettings.Endpoint,
            apiKey: connectionSettings.ApiKey?.Reveal() ?? string.Empty,
            httpClient: httpClient);
    }

    internal static void RegisterAzureOpenAIEmbedding(
        this IKernelBuilder builder,
        VKAISKOptions aiskOptions,
        IVKAIProviderOptions connectionSettings,
        HttpClient? httpClient)
    {
        if (string.IsNullOrWhiteSpace(connectionSettings.Endpoint))
            throw new InvalidOperationException("Endpoint is required for AzureOpenAI");

        var modelId = connectionSettings.ModelId ?? string.Empty;

        builder.AddAzureOpenAIEmbeddingGenerator(
            deploymentName: aiskOptions.DeploymentName ?? modelId,
            endpoint: connectionSettings.Endpoint,
            apiKey: connectionSettings.ApiKey?.Reveal() ?? string.Empty,
            httpClient: httpClient);
    }
}
