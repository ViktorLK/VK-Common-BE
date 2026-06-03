using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.AI.SemanticKernel.Diagnostics.Internal;
using VK.Blocks.Core;

namespace VK.Blocks.AI.SemanticKernel.AtomicTools.Internal;

/// <summary>
/// An atomic tool that performs a web search using the Wikipedia REST API.
/// This fulfills the need for a basic informational retrieval tool for Agents.
/// </summary>
internal sealed class WebSearchTool : IVKAtomicTool
{
    private readonly HttpClient _httpClient;
    private readonly IVKJsonSerializer _jsonSerializer;

    public WebSearchTool(IHttpClientFactory httpClientFactory, IVKJsonSerializer jsonSerializer)
    {
        VKGuard.NotNull(httpClientFactory);
        _jsonSerializer = VKGuard.NotNull(jsonSerializer);

        _httpClient = httpClientFactory.CreateClient(AISKConstants.HttpClientName);
    }

    /// <inheritdoc />
    public VKAtomicToolManifest Manifest { get; } = new VKAtomicToolManifest
    {
        Metadata = new VKAtomicToolMetadata
        {
            Name = "WebSearch",
            Description = "Searches Wikipedia for current factual information. Use this to look up facts, people, history, or general knowledge.",
            Category = "Search",
            Tags = ["web", "search", "wikipedia"]
        },
        ParameterSchema = """
        {
            "type": "object",
            "properties": {
                "query": {
                    "type": "string",
                    "description": "The search query to look up on Wikipedia."
                }
            },
            "required": ["query"]
        }
        """
    };

    /// <inheritdoc />
    public async Task<VKResult<VKAtomicToolResult>> ExecuteAsync(
        IDictionary<string, object> arguments,
        VKAgentExecutionContext context,
        CancellationToken cancellationToken = default)
    {
        VKGuard.NotNull(arguments);

        if (!arguments.TryGetValue("query", out var queryObj) || queryObj is not string query)
        {
            return VKResult.Failure<VKAtomicToolResult>(VKAIErrors.InvalidRequest("Missing or invalid 'query' parameter."));
        }

        try
        {
            // Wikipedia API endpoint for search
            var url = $"https://en.wikipedia.org/w/api.php?action=query&list=search&srsearch={Uri.EscapeDataString(query)}&utf8=&format=json";

            var request = new HttpRequestMessage(HttpMethod.Get, url);
            // Wikipedia API requires a User-Agent
            request.Headers.UserAgent.ParseAdd("VK.Blocks.AI/1.0 (internal-test-bot)");

            using var response = await _httpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            var resultDto = _jsonSerializer.Deserialize<WikipediaResponse>(json);

            if (resultDto?.Query?.Search == null || resultDto.Query.Search.Count == 0)
            {
                return VKResult.Success(new VKAtomicToolResult
                {
                    Content = "No results found."
                });
            }

            // Extract the top 3 results and format them into a text blob for the AI
            var resultsText = string.Join("\n\n", resultDto.Query.Search.ConvertAll(s => $"Title: {s.Title}\nSnippet: {StripHtml(s.Snippet)}"));

            return VKResult.Success(new VKAtomicToolResult
            {
                Content = resultsText
            });
        }
        catch (OperationCanceledException)
        {
            return VKResult.Failure<VKAtomicToolResult>(VKAIErrors.Timeout);
        }
        catch (Exception ex)
        {
            return VKResult.Failure<VKAtomicToolResult>(VKAIErrors.EngineError(ex.Message));
        }
    }

    private static string StripHtml(string? input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return string.Empty;
        // Super simple HTML tag stripper for Wikipedia snippets
        return System.Text.RegularExpressions.Regex.Replace(input, "<.*?>", string.Empty);
    }

    private sealed record WikipediaResponse
    {
        [JsonPropertyName("query")]
        public WikipediaQuery? Query { get; init; }
    }

    private sealed record WikipediaQuery
    {
        [JsonPropertyName("search")]
        public List<WikipediaSearchResult>? Search { get; init; }
    }

    private sealed record WikipediaSearchResult
    {
        [JsonPropertyName("title")]
        public string? Title { get; init; }

        [JsonPropertyName("snippet")]
        public string? Snippet { get; init; }
    }
}
