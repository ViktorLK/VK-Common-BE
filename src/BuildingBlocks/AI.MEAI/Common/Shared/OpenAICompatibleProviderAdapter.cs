#pragma warning disable MEAI001

using System.Net.Http;
using Microsoft.Extensions.AI;

namespace VK.Blocks.AI.MEAI.Common.Shared;

/// <summary>
/// Shared adapter for OpenAI-compatible endpoints (e.g. Custom, DeepSeek, XAI, Mistral).
/// </summary>
internal sealed class OpenAICompatibleProviderAdapter(VKAIProviderType provider = VKAIProviderType.Custom) : IMEAIProviderAdapter
{
    public VKAIProviderType Provider { get; } = provider;

    public IChatClient CreateChatClient(VKAIMEAIProviderSettings options, HttpClient httpClient)
        => AIMEAIClientFactory.CreateOpenAIWireChatClient(options, httpClient);

    public IEmbeddingGenerator<string, Embedding<float>> CreateEmbeddingGenerator(VKAIMEAIProviderSettings options, HttpClient httpClient)
        => AIMEAIClientFactory.CreateOpenAIWireEmbeddingGenerator(options, httpClient);

    public ITextToSpeechClient CreateTextToSpeechClient(VKAIMEAIProviderSettings options, HttpClient httpClient)
        => AIMEAIClientFactory.CreateOpenAIWireTextToSpeechClient(options, httpClient);

    public ISpeechToTextClient CreateSpeechToTextClient(VKAIMEAIProviderSettings options, HttpClient httpClient)
        => AIMEAIClientFactory.CreateOpenAIWireSpeechToTextClient(options, httpClient);

    public IImageGenerator CreateImageGenerator(VKAIMEAIProviderSettings options, HttpClient httpClient)
        => AIMEAIClientFactory.CreateOpenAIWireImageGenerator(options, httpClient);
}
