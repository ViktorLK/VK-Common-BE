#pragma warning disable MEAI001

using System.Net.Http;
using Microsoft.Extensions.AI;

namespace VK.Blocks.AI.MEAI.Common.Shared;

internal sealed class OpenAIProviderAdapter : IMEAIProviderAdapter
{
    public VKAIProviderType Provider => VKAIProviderType.OpenAI;

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
