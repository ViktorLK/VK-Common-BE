using System.Net.Http;
using Microsoft.SemanticKernel;

namespace VK.Blocks.AI.SemanticKernel.Kernel.Internal;

/// <summary>
/// Helper for registering specific AI providers to the Kernel.
/// This class is partial to allow provider-specific extensions in separate files.
/// </summary>
internal static partial class AISKProviderRegistrar
{
    internal static void RegisterChatService(
        this IKernelBuilder builder,
        VKAISKOptions aiskOptions,
        IVKAIProviderOptions connectionSettings,
        HttpClient? httpClient)
    {
        switch (connectionSettings.Provider)
        {
            case VKAIProviderType.AzureOpenAI:
                builder.RegisterAzureOpenAIChat(aiskOptions, connectionSettings, httpClient);
                break;
            case VKAIProviderType.Google:
                builder.RegisterGoogleAIChat(aiskOptions, connectionSettings, httpClient);
                break;
            case VKAIProviderType.Ollama:
                builder.RegisterOllamaChat(aiskOptions, connectionSettings);
                break;
            case VKAIProviderType.OpenAI:
            default:
                builder.RegisterOpenAIChat(aiskOptions, connectionSettings, httpClient);
                break;
        }
    }

    internal static void RegisterEmbeddingService(
        this IKernelBuilder builder,
        VKAISKOptions aiskOptions,
        IVKAIProviderOptions connectionSettings,
        HttpClient? httpClient)
    {
        switch (connectionSettings.Provider)
        {
            case VKAIProviderType.AzureOpenAI:
                builder.RegisterAzureOpenAIEmbedding(aiskOptions, connectionSettings, httpClient);
                break;
            case VKAIProviderType.Google:
                builder.RegisterGoogleAIEmbedding(aiskOptions, connectionSettings, httpClient);
                break;
            case VKAIProviderType.Ollama:
                builder.RegisterOllamaEmbedding(aiskOptions, connectionSettings);
                break;
            case VKAIProviderType.OpenAI:
            default:
                builder.RegisterOpenAIEmbedding(aiskOptions, connectionSettings, httpClient);
                break;
        }
    }
}
