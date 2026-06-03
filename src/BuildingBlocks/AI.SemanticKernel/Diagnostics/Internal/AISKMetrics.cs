using System.Diagnostics;
using System.Diagnostics.Metrics;
using VK.Blocks.Core;

namespace VK.Blocks.AI.SemanticKernel.Diagnostics.Internal;

/// <summary>
/// Metrics for the AISK building block.
/// </summary>
[VKBlockDiagnostics<VKAISKBlock>]
internal static partial class AISKMetrics
{
    private static readonly Histogram<double> ChatRequestDuration;
    private static readonly Counter<long> ChatTokenUsage;
    private static readonly Histogram<double> EmbeddingGenerationDuration;
    private static readonly Counter<long> EmbeddingItemsCount;
    private static readonly Counter<long> AutoToolCallsTotal;

    static AISKMetrics()
    {
        ChatRequestDuration = Meter.CreateHistogram<double>(
            VKAISKDiagnosticsConstants.Metrics.ChatRequestDuration,
            unit: "s",
            description: "Duration of AI chat requests.");

        ChatTokenUsage = Meter.CreateCounter<long>(
            VKAISKDiagnosticsConstants.Metrics.ChatTokenUsage,
            unit: "{tokens}",
            description: "Total number of tokens consumed by AI chat requests.");

        EmbeddingGenerationDuration = Meter.CreateHistogram<double>(
            VKAISKDiagnosticsConstants.Metrics.EmbeddingGenerationDuration,
            unit: "s",
            description: "Duration of embedding generation requests.");

        EmbeddingItemsCount = Meter.CreateCounter<long>(
            VKAISKDiagnosticsConstants.Metrics.EmbeddingItemsCount,
            unit: "{items}",
            description: "Total number of items processed for embeddings.");

        AutoToolCallsTotal = Meter.CreateCounter<long>(
            VKAISKDiagnosticsConstants.Metrics.AutoToolCallsTotal,
            unit: "{calls}",
            description: "Total number of automatic tool/function invocations triggered by the LLM.");
    }

    /// <summary>
    /// Records the duration of a chat request.
    /// </summary>
    public static void RecordChatDuration(double durationSeconds, string? modelId)
    {
        ChatRequestDuration.Record(durationSeconds, new TagList
        {
            { VKAISKDiagnosticsConstants.Tags.ModelId, modelId ?? "unknown" }
        });
    }

    /// <summary>
    /// Records the duration of an embedding generation request.
    /// </summary>
    public static void RecordEmbeddingDuration(double durationSeconds, string? modelId)
    {
        EmbeddingGenerationDuration.Record(durationSeconds, new TagList
        {
            { VKAISKDiagnosticsConstants.Tags.ModelId, modelId ?? "unknown" }
        });
    }

    /// <summary>
    /// Records the number of items processed for embeddings.
    /// </summary>
    public static void RecordEmbeddingItems(int count, string? modelId)
    {
        EmbeddingItemsCount.Add(count, new TagList
        {
            { VKAISKDiagnosticsConstants.Tags.ModelId, modelId ?? "unknown" }
        });
    }

    /// <summary>
    /// Records the token usage of a chat request.
    /// </summary>
    public static void RecordTokenUsage(string? modelId, int promptTokens, int completionTokens)
    {
        var tags = new TagList { { VKAISKDiagnosticsConstants.Tags.ModelId, modelId ?? "unknown" } };

        if (promptTokens > 0)
        {
            var pTags = tags;
            pTags.Add(VKAISKDiagnosticsConstants.Tags.TokenType, "prompt");
            ChatTokenUsage.Add(promptTokens, pTags);
        }

        if (completionTokens > 0)
        {
            var cTags = tags;
            cTags.Add(VKAISKDiagnosticsConstants.Tags.TokenType, "completion");
            ChatTokenUsage.Add(completionTokens, cTags);
        }
    }

    /// <summary>
    /// Records a single automatic function/tool call triggered by the LLM.
    /// </summary>
    /// <param name="pluginName">The plugin (tool category) name.</param>
    /// <param name="functionName">The specific function name invoked.</param>
    public static void RecordAutoToolCall(string? pluginName, string? functionName)
    {
        AutoToolCallsTotal.Add(1, new TagList
        {
            { "vk.ai.tool.plugin", pluginName ?? "unknown" },
            { "vk.ai.tool.function", functionName ?? "unknown" }
        });
    }
}
