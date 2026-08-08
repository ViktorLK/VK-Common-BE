using System;
using System.Net.Http;
using Microsoft.SemanticKernel;

namespace VK.Blocks.AI.SemanticKernel.Common.Kernel.Internal;

internal static partial class AISemanticKernelProviderRegistrar
{
    internal static void RegisterAzureOpenAIChat(
        this IKernelBuilder builder,
        VKAISemanticKernelOptions AISemanticKernelOptions,
        IVKAIProviderOptions connectionSettings,
        HttpClient? httpClient,
        string? serviceId = null)
    {
        if (string.IsNullOrWhiteSpace(connectionSettings.Endpoint))
            throw new InvalidOperationException("Endpoint is required for AzureOpenAI");

        var modelId = connectionSettings.ModelId ?? string.Empty;

        builder.AddAzureOpenAIChatCompletion(
            deploymentName: AISemanticKernelOptions.DeploymentName ?? modelId,
            endpoint: connectionSettings.Endpoint,
            apiKey: connectionSettings.ApiKey?.Reveal() ?? string.Empty,
            serviceId: serviceId,
            httpClient: httpClient);
    }

    internal static void RegisterAzureOpenAIEmbedding(
        this IKernelBuilder builder,
        VKAISemanticKernelOptions AISemanticKernelOptions,
        IVKAIProviderOptions connectionSettings,
        HttpClient? httpClient)
    {
        if (string.IsNullOrWhiteSpace(connectionSettings.Endpoint))
            throw new InvalidOperationException("Endpoint is required for AzureOpenAI");

        var modelId = connectionSettings.ModelId ?? string.Empty;

        builder.AddAzureOpenAIEmbeddingGenerator(
            deploymentName: AISemanticKernelOptions.DeploymentName ?? modelId,
            endpoint: connectionSettings.Endpoint,
            apiKey: connectionSettings.ApiKey?.Reveal() ?? string.Empty,
            httpClient: httpClient);
    }
}
