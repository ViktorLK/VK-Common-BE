using System;
using System.Net.Http;
using Microsoft.SemanticKernel;

namespace VK.Blocks.AI.SemanticKernel.Kernel.Internal;

internal static partial class AISKProviderRegistrar
{
    internal static void RegisterAzureOpenAI(
        this IKernelBuilder builder,
        VKAISKOptions options,
        string modelId,
        bool isChat,
        HttpClient? httpClient)
    {
        if (string.IsNullOrWhiteSpace(options.Endpoint))
            throw new InvalidOperationException("Endpoint is required for AzureOpenAI");

        if (isChat)
        {
            builder.AddAzureOpenAIChatCompletion(
                deploymentName: options.DeploymentName ?? modelId,
                endpoint: options.Endpoint,
                apiKey: options.ApiKey!,
                httpClient: httpClient);
        }
        else
        {
            builder.AddAzureOpenAIEmbeddingGenerator(
                deploymentName: options.DeploymentName ?? modelId,
                endpoint: options.Endpoint,
                apiKey: options.ApiKey!,
                httpClient: httpClient);
        }
    }
}
