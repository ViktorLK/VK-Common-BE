using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Vectorics.Embeddings.Internal;

/// <summary>
/// A lightweight HTTP client implementation of <see cref="IVKEmbeddingsEngine"/> direct connecting to OpenAI/Azure/Ollama.
/// </summary>
internal sealed class DefaultEmbeddingsEngine : IVKEmbeddingsEngine
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IVKJsonSerializer _jsonSerializer;
    private readonly VKEmbeddingsOptions _options;

    public DefaultEmbeddingsEngine(
        IHttpClientFactory httpClientFactory,
        IVKJsonSerializer jsonSerializer,
        IOptions<VKEmbeddingsOptions> options)
    {
        _httpClientFactory = VKGuard.NotNull(httpClientFactory);
        _jsonSerializer = VKGuard.NotNull(jsonSerializer);
        _options = options?.Value ?? new VKEmbeddingsOptions();
    }

    public async Task<VKResult<VKEmbeddingsResponse>> GetEmbeddingsAsync(
        IEnumerable<string> inputs,
        VKEmbeddingsArgs? args = null,
        CancellationToken cancellationToken = default)
    {
        VKGuard.NotNull(inputs);

        var inputList = inputs.ToList();
        if (inputList.Count == 0)
        {
            return VKResult.Success(new VKEmbeddingsResponse
            {
                Vectors = Array.Empty<VKEmbeddingsVector>()
            });
        }

        // Merge runtime options using standard merge utility
        var mergedOptions = args.Merge(_options);
        var provider = mergedOptions.Provider ?? VKAIProviderType.OpenAI;
        var apiKey = mergedOptions.ApiKey?.Reveal();
        var modelId = mergedOptions.ModelId ?? "text-embedding-3-small";
        var endpoint = mergedOptions.Endpoint;

        using var client = _httpClientFactory.CreateClient();
        if (mergedOptions.Timeout.HasValue)
        {
            client.Timeout = mergedOptions.Timeout.Value;
        }

        HttpRequestMessage request;

        if (provider == VKAIProviderType.AzureOpenAI)
        {
            if (string.IsNullOrWhiteSpace(endpoint))
            {
                return VKResult.Failure<VKEmbeddingsResponse>(VKEmbeddingsErrors.EndpointRequired);
            }
            if (string.IsNullOrWhiteSpace(apiKey))
            {
                return VKResult.Failure<VKEmbeddingsResponse>(VKEmbeddingsErrors.ApiKeyRequired);
            }

            var requestUri = new Uri(new Uri(endpoint), $"openai/deployments/{modelId}/embeddings?api-version=2023-05-15");
            request = new HttpRequestMessage(HttpMethod.Post, requestUri);
            request.Headers.Add("api-key", apiKey);

            var requestBody = new AzureOpenAIEmbeddingsRequest { Input = inputList };
            var json = _jsonSerializer.Serialize(requestBody);
            request.Content = new StringContent(json, Encoding.UTF8, "application/json");
        }
        else if (provider == VKAIProviderType.Ollama)
        {
            var uri = string.IsNullOrWhiteSpace(endpoint) ? "http://localhost:11434/api/embeddings" : endpoint;
            request = new HttpRequestMessage(HttpMethod.Post, uri);

            var requestBody = new OllamaEmbeddingsRequest { Model = modelId, Prompt = inputList.Count == 1 ? inputList[0] : null, Prompts = inputList.Count > 1 ? inputList : null };
            var json = _jsonSerializer.Serialize(requestBody);
            request.Content = new StringContent(json, Encoding.UTF8, "application/json");
        }
        else // Default to OpenAI
        {
            var uri = string.IsNullOrWhiteSpace(endpoint) ? "https://api.openai.com/v1/embeddings" : endpoint;
            request = new HttpRequestMessage(HttpMethod.Post, uri);
            if (!string.IsNullOrWhiteSpace(apiKey))
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
            }

            var requestBody = new OpenAIEmbeddingsRequest { Model = modelId, Input = inputList };
            var json = _jsonSerializer.Serialize(requestBody);
            request.Content = new StringContent(json, Encoding.UTF8, "application/json");
        }

        try
        {
            var response = await client.SendAsync(request, cancellationToken).ConfigureAwait(false); // [CS.03]
            if (!response.IsSuccessStatusCode)
            {
                var errorMsg = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false); // [CS.03]
                return VKResult.Failure<VKEmbeddingsResponse>(VKEmbeddingsErrors.EngineError(errorMsg));
            }

            var responseJson = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false); // [CS.03]

            if (provider == VKAIProviderType.Ollama)
            {
                var ollamaResponse = _jsonSerializer.Deserialize<OllamaEmbeddingsResponse>(responseJson);
                if (ollamaResponse?.Embedding != null)
                {
                    var vector = new VKEmbeddingsVector { Values = ollamaResponse.Embedding.ToArray() };
                    return VKResult.Success(new VKEmbeddingsResponse
                    {
                        Vectors = new[] { vector },
                        ModelId = modelId
                    });
                }
                else if (ollamaResponse?.Embeddings != null)
                {
                    var vectors = ollamaResponse.Embeddings.Select(e => new VKEmbeddingsVector { Values = e.ToArray() }).ToList();
                    return VKResult.Success(new VKEmbeddingsResponse
                    {
                        Vectors = vectors,
                        ModelId = modelId
                    });
                }
                return VKResult.Failure<VKEmbeddingsResponse>(VKEmbeddingsErrors.InvalidResponse);
            }
            else
            {
                var openAiResponse = _jsonSerializer.Deserialize<OpenAIEmbeddingsResponse>(responseJson);
                if (openAiResponse?.Data == null)
                {
                    return VKResult.Failure<VKEmbeddingsResponse>(VKEmbeddingsErrors.InvalidResponse);
                }

                var vectors = openAiResponse.Data
                    .OrderBy(d => d.Index)
                    .Select(d => new VKEmbeddingsVector { Values = d.Embedding?.ToArray() ?? Array.Empty<float>() })
                    .ToList();

                var usage = openAiResponse.Usage != null ? new VKAITokenUsage
                {
                    InputTokens = openAiResponse.Usage.Prompt_Tokens,
                    OutputTokens = openAiResponse.Usage.Total_Tokens - openAiResponse.Usage.Prompt_Tokens
                } : null;

                return VKResult.Success(new VKEmbeddingsResponse
                {
                    Vectors = vectors,
                    ModelId = modelId,
                    Usage = usage
                });
            }
        }
        catch (Exception ex)
        {
            return VKResult.Failure<VKEmbeddingsResponse>(VKEmbeddingsErrors.EngineError(ex.Message));
        }
    }

    private sealed class OpenAIEmbeddingsRequest
    {
        public string? Model { get; set; }
        public List<string>? Input { get; set; }
    }

    private sealed class OpenAIEmbeddingsResponse
    {
        public List<OpenAIEmbeddingData>? Data { get; set; }
        public OpenAIUsageData? Usage { get; set; }
    }

    private sealed class OpenAIEmbeddingData
    {
        public int Index { get; set; }
        public List<float>? Embedding { get; set; }
    }

    private sealed class OpenAIUsageData
    {
        public int Prompt_Tokens { get; set; }
        public int Total_Tokens { get; set; }
    }

    private sealed class AzureOpenAIEmbeddingsRequest
    {
        public List<string>? Input { get; set; }
    }

    private sealed class OllamaEmbeddingsRequest
    {
        public string? Model { get; set; }
        public string? Prompt { get; set; }
        public List<string>? Prompts { get; set; }
    }

    private sealed class OllamaEmbeddingsResponse
    {
        public List<float>? Embedding { get; set; }
        public List<List<float>>? Embeddings { get; set; }
    }
}
