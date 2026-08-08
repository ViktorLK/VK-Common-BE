using System.Net.Http;
using Microsoft.SemanticKernel;

namespace VK.Blocks.AI.SemanticKernel.Common.Kernel.Internal;

/// <summary>
/// Helper for registering specific AI providers to the Kernel.
/// This class is partial to allow provider-specific extensions in separate files.
/// </summary>
internal static partial class AISemanticKernelProviderRegistrar
{
    internal static void RegisterChatService(
        this IKernelBuilder builder,
        VKAISemanticKernelOptions AISemanticKernelOptions,
        IVKAIProviderOptions connectionSettings,
        HttpClient? httpClient,
        string? serviceId = null)
    {
        switch (connectionSettings.Provider)
        {
            case VKAIProviderType.AzureOpenAI:
                builder.RegisterAzureOpenAIChat(AISemanticKernelOptions, connectionSettings, httpClient, serviceId);
                break;
            case VKAIProviderType.Google:
                builder.RegisterGoogleAIChat(AISemanticKernelOptions, connectionSettings, httpClient, serviceId);
                break;
            case VKAIProviderType.Ollama:
                builder.RegisterOllamaChat(AISemanticKernelOptions, connectionSettings, serviceId);
                break;
            case VKAIProviderType.OpenAI:
            default:
                builder.RegisterOpenAIChat(AISemanticKernelOptions, connectionSettings, httpClient, serviceId);
                break;
        }
    }

    internal static void RegisterEmbeddingService(
        this IKernelBuilder builder,
        VKAISemanticKernelOptions AISemanticKernelOptions,
        IVKAIProviderOptions connectionSettings,
        HttpClient? httpClient)
    {
        switch (connectionSettings.Provider)
        {
            case VKAIProviderType.AzureOpenAI:
                builder.RegisterAzureOpenAIEmbedding(AISemanticKernelOptions, connectionSettings, httpClient);
                break;
            case VKAIProviderType.Google:
                builder.RegisterGoogleAIEmbedding(AISemanticKernelOptions, connectionSettings, httpClient);
                break;
            case VKAIProviderType.Ollama:
                builder.RegisterOllamaEmbedding(AISemanticKernelOptions, connectionSettings);
                break;
            case VKAIProviderType.OpenAI:
            default:
                builder.RegisterOpenAIEmbedding(AISemanticKernelOptions, connectionSettings, httpClient);
                break;
        }
    }
}
