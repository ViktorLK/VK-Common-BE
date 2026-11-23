#pragma warning disable MEAI001

using System;
using System.Net.Http;
using Microsoft.Extensions.AI;

namespace VK.Blocks.AI.MEAI.Common.Shared;

internal sealed class GoogleProviderAdapter : IMEAIProviderAdapter
{
    public VKAIProviderType Provider => VKAIProviderType.Google;

    public IChatClient CreateChatClient(VKAIMEAIProviderSettings options, HttpClient httpClient)
        => AIMEAIClientFactory.CreateGoogleChatClient(options, httpClient);

    public IEmbeddingGenerator<string, Embedding<float>> CreateEmbeddingGenerator(VKAIMEAIProviderSettings options, HttpClient httpClient)
        => AIMEAIClientFactory.CreateGoogleEmbeddingGenerator(options, httpClient);

    public ITextToSpeechClient CreateTextToSpeechClient(VKAIMEAIProviderSettings options, HttpClient httpClient)
        => throw new NotSupportedException($"Provider '{Provider}' is not supported for text-to-speech.");

    public ISpeechToTextClient CreateSpeechToTextClient(VKAIMEAIProviderSettings options, HttpClient httpClient)
        => AIMEAIClientFactory.CreateGoogleSpeechToTextClient(options, httpClient);

    public IImageGenerator CreateImageGenerator(VKAIMEAIProviderSettings options, HttpClient httpClient)
        => throw new NotSupportedException("Google image generation is not yet supported via Microsoft.Extensions.AI. Use OpenAI or AzureOpenAI.");
}
