using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using VK.Blocks.AI.Engram.Compression.Diagnostics.Internal;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Engram.Compression.Internal;

/// <summary>
/// Compression strategy that identifies topic boundaries and summarizes by topic segments.
/// </summary>
internal sealed class TopicSegmentationCompressionStrategy : IVKCompressionStrategy
{
    private readonly IVKChatEngine _chatEngine;
    private readonly VKCompressionOptions _options;
    private readonly ILogger<TopicSegmentationCompressionStrategy> _logger;

    private static readonly Regex RangeRegex = new(@"^\[(\d+)-(\d+)\]\s*(.*)$", RegexOptions.Compiled);

    public TopicSegmentationCompressionStrategy(
        IVKChatEngine chatEngine,
        IOptions<VKCompressionOptions> options,
        ILogger<TopicSegmentationCompressionStrategy> logger)
    {
        _chatEngine = VKGuard.NotNull(chatEngine);
        _options = VKGuard.NotNull(options?.Value);
        _logger = VKGuard.NotNull(logger);
    }

    public async Task<VKResult<string>> CompressAsync(VKCompressionContext context, CancellationToken cancellationToken = default)
    {
        VKGuard.NotNull(context);

        if (string.IsNullOrWhiteSpace(context.Content))
        {
            return VKResult.Success(string.Empty);
        }

        var lines = VKTextTokenizer.SplitLines(context.Content);
        if (lines.Count == 0)
        {
            return VKResult.Success(string.Empty);
        }

        // 1. Build indexed history for LLM segmenter
        var indexedHistory = new StringBuilder();
        for (int i = 0; i < lines.Count; i++)
        {
            indexedHistory.AppendLine($"[{i + 1}] {lines[i]}");
        }

        string segmentationPrompt = "Analyze the following indexed conversation history and identify semantic topic boundaries.\n" +
                                     "Output topic segments with their line ranges and topic titles in this exact format:\n" +
                                     "[StartLine-EndLine] Topic Title\n\n" +
                                     "Example:\n" +
                                     "[1-6] Technology Selection\n" +
                                     "[7-11] Budget Discussion\n\n" +
                                     "Output ONLY the segment list, one per line. Do not write any intro, explanations, or outro.\n\n" +
                                     $"CONVERSATION HISTORY:\n{indexedHistory}";

        var messages = new[] { VKChatMessage.FromText(VKChatRole.User, segmentationPrompt) };
        IVKAIArgs? chatArgs = null;
        string? targetModel = _options.SegmentationModelId ?? _options.ModelId;
        if (!string.IsNullOrWhiteSpace(targetModel))
        {
            chatArgs = new VKChatArgs { ModelId = targetModel };
        }

        // 2. Query LLM to detect boundaries
        var segmentationResult = await _chatEngine.SendAsync(messages, chatArgs, cancellationToken).ConfigureAwait(false);
        if (!segmentationResult.IsSuccess)
        {
            _logger.TopicSegmentationFailed(segmentationResult.FirstError.Description);
            return await FallbackSummarizeAsync(context.Content, chatArgs, cancellationToken).ConfigureAwait(false);
        }

        string segmentsText = segmentationResult.Value.Message.Content ?? string.Empty;
        var segments = ParseSegments(segmentsText, lines.Count);

        if (segments.Count == 0)
        {
            _logger.NoValidSegmentsParsed();
            return await FallbackSummarizeAsync(context.Content, chatArgs, cancellationToken).ConfigureAwait(false);
        }

        // 3. Summarize each segment
        var finalSummary = new StringBuilder();
        foreach (var segment in segments)
        {
            var segmentLines = new List<string>();
            int startIdx = Math.Max(0, segment.StartLine - 1);
            int endIdx = Math.Min(lines.Count - 1, segment.EndLine - 1);

            for (int i = startIdx; i <= endIdx; i++)
            {
                segmentLines.Add(lines[i]);
            }

            if (segmentLines.Count == 0)
                continue;

            string segmentContent = string.Join("\n", segmentLines);
            string summarizationPrompt = $"Summarize this section of conversation history related to the topic '{segment.Title}':\n\n{segmentContent}";
            var sumMessages = new[] { VKChatMessage.FromText(VKChatRole.User, summarizationPrompt) };

            var summaryResult = await _chatEngine.SendAsync(sumMessages, chatArgs, cancellationToken).ConfigureAwait(false);
            if (summaryResult.IsSuccess)
            {
                var summary = summaryResult.Value.Message.Content ?? string.Empty;
                if (!string.IsNullOrWhiteSpace(summary))
                {
                    finalSummary.AppendLine($"### [Topic: {segment.Title}]");
                    finalSummary.AppendLine(summary);
                    finalSummary.AppendLine();
                }
            }
        }

        return VKResult.Success(finalSummary.ToString().Trim());
    }

    private static List<TopicSegment> ParseSegments(string segmentsText, int maxLines)
    {
        var result = new List<TopicSegment>();
        var lines = VKTextTokenizer.SplitLines(segmentsText);

        foreach (var line in lines)
        {
            var match = RangeRegex.Match(line.Trim());
            if (match.Success)
            {
                if (int.TryParse(match.Groups[1].Value, out int start) &&
                    int.TryParse(match.Groups[2].Value, out int end))
                {
                    string title = match.Groups[3].Value.Trim();
                    if (string.IsNullOrWhiteSpace(title))
                    {
                        title = "General Conversation";
                    }

                    // Constrain indexes safely
                    start = Math.Clamp(start, 1, maxLines);
                    end = Math.Clamp(end, start, maxLines);

                    result.Add(new TopicSegment
                    {
                        StartLine = start,
                        EndLine = end,
                        Title = title
                    });
                }
            }
        }

        return result;
    }

    private async Task<VKResult<string>> FallbackSummarizeAsync(string content, IVKAIArgs? chatArgs, CancellationToken cancellationToken)
    {
        string prompt = $"Summarize the following chat history briefly, consolidating key points, facts, and preferences:\n\n{content}";
        var messages = new[] { VKChatMessage.FromText(VKChatRole.User, prompt) };

        var result = await _chatEngine.SendAsync(messages, chatArgs, cancellationToken).ConfigureAwait(false);
        if (!result.IsSuccess)
        {
            return VKResult.Failure<string>(result.FirstError);
        }

        return VKResult.Success(result.Value.Message.Content ?? string.Empty);
    }

    private sealed class TopicSegment
    {
        public int StartLine { get; set; }
        public int EndLine { get; set; }
        public string Title { get; set; } = string.Empty;
    }
}
