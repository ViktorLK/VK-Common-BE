using System;
using System.Net.Http;
using Microsoft.SemanticKernel;

namespace VK.Blocks.AI.SemanticKernel.Kernel.Internal;

internal static partial class AISKProviderRegistrar
{
    internal static void RegisterOpenAI(
        this IKernelBuilder builder,
        VKAISKOptions options,
        string modelId,
        HttpClient? httpClient)
    {
        builder.AddOpenAIChatCompletion(
            modelId,
            options.ApiKey ?? throw new InvalidOperationException("ApiKey is required for OpenAI"),
            orgId: options.OrgId,
            httpClient: httpClient);
    }
}
