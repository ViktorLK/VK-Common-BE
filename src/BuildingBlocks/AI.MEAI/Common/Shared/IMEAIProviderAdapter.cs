#pragma warning disable MEAI001

using System.Net.Http;
using Microsoft.Extensions.AI;

namespace VK.Blocks.AI.MEAI.Common.Shared;

/// <summary>
/// Seam abstraction for creating provider-specific MEAI clients
/// without coupling the router/factory directly to concrete vendor SDKs (R1).
/// </summary>
internal interface IMEAIProviderAdapter
{
    VKAIProviderType Provider { get; }

    IChatClient CreateChatClient(VKAIMEAIProviderSettings options, HttpClient httpClient);

    IEmbeddingGenerator<string, Embedding<float>> CreateEmbeddingGenerator(VKAIMEAIProviderSettings options, HttpClient httpClient);

    ITextToSpeechClient CreateTextToSpeechClient(VKAIMEAIProviderSettings options, HttpClient httpClient);

    ISpeechToTextClient CreateSpeechToTextClient(VKAIMEAIProviderSettings options, HttpClient httpClient);

    IImageGenerator CreateImageGenerator(VKAIMEAIProviderSettings options, HttpClient httpClient);
}
