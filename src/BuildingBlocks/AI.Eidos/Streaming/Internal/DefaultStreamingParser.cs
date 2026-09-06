using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.RegularExpressions;
using VK.Blocks.AI.Eidos.Common.Internal;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Eidos.Streaming.Internal;

internal sealed class DefaultStreamingParser : IVKStreamingParser // [AP.01]
{
    private static readonly Regex UnclosedThinkStartRegex = new(
        @"<think>(?:(?!</think>)[\s\S])*$",
        RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.NonBacktracking);

    private static readonly string[] SpeculativeClosingCandidates =
    [
        "}",
        "\"}",
        "]}",
        "\"]}",
        "\"}}",
        "}}",
        "\"]}}",
        "\"}]}",
        "}]}"
    ];

    public VKStreamingChunk ParseChunk(string accumulatedText, VKAIEidosSchema schema)
    {
        VKGuard.NotNull(schema);
        if (string.IsNullOrWhiteSpace(accumulatedText))
        {
            return new VKStreamingChunk();
        }

        // 1. Check if model is currently within an unclosed <think> tag
        if (UnclosedThinkStartRegex.IsMatch(accumulatedText))
        {
            return new VKStreamingChunk
            {
                ChunkType = VKStreamingChunkType.ThinkingPart,
                DeltaText = accumulatedText,
                AvailableProperties = new Dictionary<string, object?>(),
                IsComplete = false
            };
        }

        // 2. Strip completed thinking blocks using shared extractor
        var sanitizedText = JsonBlockExtractor.StripThinkingBlocks(accumulatedText);
        if (string.IsNullOrWhiteSpace(sanitizedText))
        {
            return new VKStreamingChunk
            {
                ChunkType = VKStreamingChunkType.TextPart,
                DeltaText = accumulatedText,
                AvailableProperties = new Dictionary<string, object?>(),
                IsComplete = false
            };
        }

        // 3. Locate the active JSON candidate block using shared extractor
        var candidate = JsonBlockExtractor.LocateStreamingJsonCandidate(sanitizedText);
        if (string.IsNullOrWhiteSpace(candidate))
        {
            return new VKStreamingChunk
            {
                ChunkType = VKStreamingChunkType.TextPart,
                DeltaText = accumulatedText,
                AvailableProperties = new Dictionary<string, object?>(),
                IsComplete = false
            };
        }

        var availableProps = new Dictionary<string, object?>();
        bool isComplete = false;

        // 4. Try parsing full complete JSON first
        try
        {
            using var doc = JsonDocument.Parse(candidate);
            if (doc.RootElement.ValueKind == JsonValueKind.Object)
            {
                foreach (var prop in doc.RootElement.EnumerateObject())
                {
                    availableProps[prop.Name] = prop.Value.Clone();
                }
                isComplete = true;
            }
            else if (doc.RootElement.ValueKind == JsonValueKind.Array)
            {
                availableProps["$items"] = doc.RootElement.Clone();
                isComplete = true;
            }
        }
        catch (JsonException)
        {
            // 5. Speculative synthetic closing to harvest partially formed properties
            TryExtractPartialProperties(candidate, availableProps);
        }

        // 6. Schema fingerprint validation (noise filtering)
        if (schema.RequiredProperties.Count > 0 && availableProps.Count > 0 && !availableProps.ContainsKey("$items"))
        {
            // Filter noise properties if none of the parsed properties match schema
            bool hasMatchingProperty = schema.RequiredProperties.Any(availableProps.ContainsKey);
            if (!hasMatchingProperty && availableProps.Count < schema.RequiredProperties.Count)
            {
                // Unrelated dummy or noise JSON, suppress properties
                availableProps.Clear();
            }
        }

        var chunkType = isComplete ? VKStreamingChunkType.TextPart : VKStreamingChunkType.DeltaPart;

        return new VKStreamingChunk
        {
            ChunkType = chunkType,
            DeltaText = accumulatedText,
            AvailableProperties = availableProps,
            IsComplete = isComplete
        };
    }

    private static void TryExtractPartialProperties(string candidate, Dictionary<string, object?> availableProps)
    {
        var trimmed = candidate.Trim();
        if (!trimmed.StartsWith('{'))
            return;

        foreach (var suffix in SpeculativeClosingCandidates)
        {
            try
            {
                using var partialDoc = JsonDocument.Parse(trimmed + suffix);
                if (partialDoc.RootElement.ValueKind == JsonValueKind.Object)
                {
                    foreach (var prop in partialDoc.RootElement.EnumerateObject())
                    {
                        availableProps[prop.Name] = prop.Value.Clone();
                    }
                    return;
                }
            }
            catch (JsonException)
            {
                // Continue to next closing candidate
            }
        }
    }
}
