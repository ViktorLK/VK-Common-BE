using System;
using System.Net.Http;
using Microsoft.SemanticKernel;

namespace VK.Blocks.AI.SemanticKernel.Kernel.Internal;

internal static partial class AISKProviderRegistrar
{
    internal static void RegisterGoogleAI(
        this IKernelBuilder builder,
        VKAISKOptions options,
        string modelId,
        bool isChat,
        HttpClient? httpClient)
    {
        var cleanModelId = modelId.Replace("models/", "", StringComparison.OrdinalIgnoreCase);
        if (isChat)
            builder.AddGoogleAIGeminiChatCompletion(cleanModelId, options.ApiKey!, httpClient: httpClient);
        else
            builder.AddGoogleAIEmbeddingGenerator(cleanModelId, options.ApiKey!, httpClient: httpClient);
    }
}
