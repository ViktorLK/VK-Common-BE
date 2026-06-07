using System;
using System.Net.Http;
using Microsoft.SemanticKernel;

using VK.Blocks.AI.SemanticKernel.Common.DependencyInjection;

namespace VK.Blocks.AI.SemanticKernel.Common.Kernel.Internal;

internal static partial class AISKProviderRegistrar
{
    internal static void RegisterAzureOpenAIChat(
        this IKernelBuilder builder,
        VKAISKDefaultsOptions aiskOptions,
        IVKAIProviderOptions connectionSettings,
        HttpClient? httpClient,
        string? serviceId = null)
    {
        if (string.IsNullOrWhiteSpace(connectionSettings.Endpoint))
            throw new InvalidOperationException("Endpoint is required for AzureOpenAI");

        var modelId = connectionSettings.ModelId ?? string.Empty;

        builder.AddAzureOpenAIChatCompletion(
            deploymentName: aiskOptions.DeploymentName ?? modelId,
            endpoint: connectionSettings.Endpoint,
            apiKey: connectionSettings.ApiKey?.Reveal() ?? string.Empty,
            serviceId: serviceId,
            httpClient: httpClient);
    }

    internal static void RegisterAzureOpenAIEmbedding(
        this IKernelBuilder builder,
        VKAISKDefaultsOptions aiskOptions,
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


