using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using VK.Blocks.AI;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Engram.Compression.Internal;

/// <summary>
/// Compression strategy that performs chunked, hierarchical summarization.
/// </summary>
internal sealed class HierarchicalSummaryCompressionStrategy : IVKCompressionStrategy
{
    private readonly IVKChatEngine _chatEngine;
    private readonly IVKTokenCounter _tokenCounter;
    private readonly VKCompressionOptions _options;

    public HierarchicalSummaryCompressionStrategy(
        IVKChatEngine chatEngine,
        IVKTokenCounter tokenCounter,
        IOptions<VKCompressionOptions> options)
    {
        _chatEngine = VKGuard.NotNull(chatEngine);
        _tokenCounter = VKGuard.NotNull(tokenCounter);
        _options = VKGuard.NotNull(options?.Value);
    }

    public async Task<VKResult<string>> CompressAsync(string content, CancellationToken cancellationToken = default)
    {
        VKGuard.NotNull(content);

        if (string.IsNullOrWhiteSpace(content))
        {
            return VKResult.Success(string.Empty);
        }

        // Split input by newlines to get individual messages
        var lines = content.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
        if (lines.Length == 0)
        {
            return VKResult.Success(string.Empty);
        }

        var chunks = new List<string>();
        var currentChunk = new StringBuilder();
        int currentChunkTokens = 0;

        foreach (var line in lines)
        {
            int lineTokens = _tokenCounter.CountTokens(line);

            if (currentChunk.Length > 0 && currentChunkTokens + lineTokens > _options.MaxInputTokensPerJob)
            {
                chunks.Add(currentChunk.ToString());
                currentChunk.Clear();
                currentChunkTokens = 0;
            }

            currentChunk.AppendLine(line);
            currentChunkTokens += lineTokens;
        }

        if (currentChunk.Length > 0)
        {
            chunks.Add(currentChunk.ToString());
        }

        // Phase 1: Summarize each chunk individually
        var chunkSummaries = new List<string>();
        IVKAIArgs? chatArgs = null;
        if (!string.IsNullOrWhiteSpace(_options.ModelId))
        {
            chatArgs = new VKChatArgs { ModelId = _options.ModelId };
        }

        foreach (var chunk in chunks)
        {
            string prompt = $"Summarize this section of chat history briefly, keeping key facts:\n\n{chunk}";
            var messages = new[] { VKChatMessage.FromText(VKChatRole.User, prompt) };

            var result = await _chatEngine.SendAsync(messages, chatArgs, cancellationToken).ConfigureAwait(false);
            if (!result.IsSuccess)
            {
                return VKResult.Failure<string>(result.FirstError);
            }

            var summary = result.Value.Message.Content ?? string.Empty;
            if (!string.IsNullOrWhiteSpace(summary))
            {
                chunkSummaries.Add(summary);
            }
        }

        if (chunkSummaries.Count == 0)
        {
            return VKResult.Success(string.Empty);
        }

        // Phase 2: Recursively merge chunk summaries into a single final summary
        string combinedSummary = string.Join("\n\n", chunkSummaries);
        int combinedTokens = _tokenCounter.CountTokens(combinedSummary);

        if (chunkSummaries.Count > 1 || combinedTokens > _options.MaxInputTokensPerJob)
        {
            string finalPrompt = $"Consolidate the following section summaries into a single, cohesive, and brief summary:\n\n{combinedSummary}";
            var finalMessages = new[] { VKChatMessage.FromText(VKChatRole.User, finalPrompt) };

            var finalResult = await _chatEngine.SendAsync(finalMessages, chatArgs, cancellationToken).ConfigureAwait(false);
            if (!finalResult.IsSuccess)
            {
                return VKResult.Failure<string>(finalResult.FirstError);
            }

            combinedSummary = finalResult.Value.Message.Content ?? string.Empty;
        }

        return VKResult.Success(combinedSummary);
    }
}
