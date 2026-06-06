using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using VK.Blocks.AI.SemanticKernel.Common.Diagnostics.Internal;
using VK.Blocks.Core;

namespace VK.Blocks.AI.SemanticKernel.Vectorics.ReRanking.Internal;

/// <summary>
/// Cohere-compatible ReRanker engine implementation.
/// </summary>
internal sealed class AISKReRankerEngine : IVKReRanker
{
    private readonly HttpClient _httpClient;
    private readonly VKReRankingOptions _options;
    private readonly IVKJsonSerializer _jsonSerializer;

    public AISKReRankerEngine(
        IHttpClientFactory httpClientFactory,
        IOptions<VKReRankingOptions> options,
        IVKJsonSerializer jsonSerializer)
    {
        VKGuard.NotNull(httpClientFactory);
        _options = VKGuard.NotNull(options?.Value);
        _jsonSerializer = VKGuard.NotNull(jsonSerializer);

        _httpClient = httpClientFactory.CreateClient(AISKConstants.HttpClientName);
    }

    /// <inheritdoc />
    public async Task<VKResult<IReadOnlyList<VKReRankingResult>>> ReRankAsync(
        string query,
        IEnumerable<string> candidates,
        VKReRankingArgs? args = null,
        CancellationToken cancellationToken = default)
    {
        VKGuard.NotNullOrWhiteSpace(query);
        VKGuard.NotNull(candidates);

        var candidateList = candidates.ToList();
        if (candidateList.Count == 0)
        {
            return VKResult.Success<IReadOnlyList<VKReRankingResult>>([]);
        }

        var endpoint = _options.Endpoint;
        var apiKey = _options.ApiKey?.Reveal();
        var modelId = _options.ModelId ?? "rerank-english-v2.0";

        if (string.IsNullOrWhiteSpace(endpoint))
        {
            return VKResult.Failure<IReadOnlyList<VKReRankingResult>>(VKAIErrors.EngineError("ConfigurationMissing"));
        }

        var requestDto = new CohereRerankRequest
        {
            Model = modelId,
            Query = query,
            Documents = candidateList,
            TopN = candidateList.Count // Return all, let the caller limit
        };

        var requestContent = new StringContent(
            _jsonSerializer.Serialize(requestDto),
            Encoding.UTF8,
            "application/json");

        using var request = new HttpRequestMessage(HttpMethod.Post, endpoint)
        {
            Content = requestContent
        };

        if (!string.IsNullOrWhiteSpace(apiKey))
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
        }

        try
        {
            using var response = await _httpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);
            response.EnsureSuccessStatusCode();

            var responseJson = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            var responseDto = _jsonSerializer.Deserialize<CohereRerankResponse>(responseJson);

            if (responseDto?.Results == null)
            {
                return VKResult.Failure<IReadOnlyList<VKReRankingResult>>(VKAIErrors.ProviderError);
            }

            var finalResults = new List<VKReRankingResult>(responseDto.Results.Count);
            foreach (var result in responseDto.Results)
            {
                if (result.Index >= 0 && result.Index < candidateList.Count)
                {
                    finalResults.Add(new VKReRankingResult(
                        candidateList[result.Index],
                        result.RelevanceScore,
                        result.Index));
                }
            }

            return VKResult.Success<IReadOnlyList<VKReRankingResult>>(finalResults);
        }
        catch (OperationCanceledException)
        {
            return VKResult.Failure<IReadOnlyList<VKReRankingResult>>(VKAIErrors.Timeout);
        }
        catch (Exception)
        {
            // Log if ILogger is injected, but return standard failure
            return VKResult.Failure<IReadOnlyList<VKReRankingResult>>(VKAIErrors.ExecutionError);
        }
    }

    private sealed record CohereRerankRequest
    {
        [JsonPropertyName("model")]
        public required string Model { get; init; }

        [JsonPropertyName("query")]
        public required string Query { get; init; }

        [JsonPropertyName("documents")]
        public required IReadOnlyList<string> Documents { get; init; }

        [JsonPropertyName("top_n")]
        public int TopN { get; init; }
    }

    private sealed record CohereRerankResponse
    {
        [JsonPropertyName("id")]
        public string? Id { get; init; }

        [JsonPropertyName("results")]
        public IReadOnlyList<CohereRerankResult>? Results { get; init; }
    }

    private sealed record CohereRerankResult
    {
        [JsonPropertyName("index")]
        public int Index { get; init; }

        [JsonPropertyName("relevance_score")]
        public float RelevanceScore { get; init; }
    }
}
