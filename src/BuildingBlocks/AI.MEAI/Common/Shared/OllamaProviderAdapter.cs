#pragma warning disable MEAI001

using System;
using System.Net.Http;
using Microsoft.Extensions.AI;

namespace VK.Blocks.AI.MEAI.Common.Shared;

internal sealed class OllamaProviderAdapter : IMEAIProviderAdapter
{
    public VKAIProviderType Provider => VKAIProviderType.Ollama;

    public IChatClient CreateChatClient(VKAIMEAIProviderSettings options, HttpClient httpClient)
        => AIMEAIClientFactory.CreateOllamaChatClient(options, httpClient);

    public IEmbeddingGenerator<string, Embedding<float>> CreateEmbeddingGenerator(VKAIMEAIProviderSettings options, HttpClient httpClient)
        => AIMEAIClientFactory.CreateOllamaEmbeddingGenerator(options, httpClient);

    public ITextToSpeechClient CreateTextToSpeechClient(VKAIMEAIProviderSettings options, HttpClient httpClient)
        => throw new NotSupportedException($"Provider '{Provider}' is not supported for text-to-speech.");

    public ISpeechToTextClient CreateSpeechToTextClient(VKAIMEAIProviderSettings options, HttpClient httpClient)
        => throw new NotSupportedException($"Provider '{Provider}' is not supported for speech-to-text.");

    public IImageGenerator CreateImageGenerator(VKAIMEAIProviderSettings options, HttpClient httpClient)
        => throw new NotSupportedException($"Provider '{Provider}' is not supported for image generation.");
}
