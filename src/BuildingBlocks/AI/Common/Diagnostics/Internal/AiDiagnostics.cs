using System.Diagnostics;
using System.Diagnostics.Metrics;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Common.Diagnostics.Internal;

/// <summary>
/// Metrics and Tracing for the AI building block.
/// </summary>
// [SG Diagnostics] - This attribute triggers the Source Generator to generate ActivitySource and Meter.
[VKBlockDiagnostics<VKAIBlock>]
internal static partial class AiDiagnostics
{
    private static readonly Counter<long> AgentRunCount;
    private static readonly Histogram<double> AgentRunDuration;
    private static readonly Counter<long> ToolCallCount;
    private static readonly Counter<long> ChatRequestCount;
    private static readonly Histogram<double> ChatRequestDuration;
    private static readonly Counter<long> TokenUsage;
    private static readonly Counter<double> TokenCost;
    private static readonly Counter<long> VectorOperationCount;
    private static readonly Counter<long> AudioOperationCount;

    static AiDiagnostics()
    {
        // Instruments are initialized using the generated Meter property
        AgentRunCount = Meter.CreateCounter<long>(VKAIDiagnosticsConstants.Metrics.AgentRunCount);
        AgentRunDuration = Meter.CreateHistogram<double>(VKAIDiagnosticsConstants.Metrics.AgentRunDuration, "ms");
        ToolCallCount = Meter.CreateCounter<long>(VKAIDiagnosticsConstants.Metrics.ToolCallCount);

        ChatRequestCount = Meter.CreateCounter<long>(VKAIDiagnosticsConstants.Metrics.ChatRequestCount);
        ChatRequestDuration = Meter.CreateHistogram<double>(VKAIDiagnosticsConstants.Metrics.ChatRequestDuration, "ms");

        TokenUsage = Meter.CreateCounter<long>(VKAIDiagnosticsConstants.Metrics.TokenUsage);
        TokenCost = Meter.CreateCounter<double>(VKAIDiagnosticsConstants.Metrics.TokenCost);

        VectorOperationCount = Meter.CreateCounter<long>(VKAIDiagnosticsConstants.Metrics.VectorOperationCount);
        AudioOperationCount = Meter.CreateCounter<long>(VKAIDiagnosticsConstants.Metrics.AudioOperationCount);
    }

    public static void RecordAgentRun(string agentName, bool success, double durationMs, string? model = null, string? tenantId = null)
    {
        var tags = new TagList
        {
            { VKAIDiagnosticsConstants.Tags.AgentName, agentName },
            { VKAIDiagnosticsConstants.Tags.Success, success }
        };

        if (model is not null)
            tags.Add(VKAIDiagnosticsConstants.Tags.ModelName, model);
        if (tenantId is not null)
            tags.Add(VKAIDiagnosticsConstants.Tags.TenantId, tenantId);

        AgentRunCount.Add(1, tags);
        AgentRunDuration.Record(durationMs, tags);
    }

    public static void RecordToolCall(string agentName, string toolName, bool success, string? tenantId = null)
    {
        var tags = new TagList
        {
            { VKAIDiagnosticsConstants.Tags.AgentName, agentName },
            { VKAIDiagnosticsConstants.Tags.ToolName, toolName },
            { VKAIDiagnosticsConstants.Tags.Success, success }
        };

        if (tenantId is not null)
            tags.Add(VKAIDiagnosticsConstants.Tags.TenantId, tenantId);

        ToolCallCount.Add(1, tags);
    }

    public static void RecordChatRequest(string provider, string model, bool success, double durationMs, string? tenantId = null)
    {
        var tags = new TagList
        {
            { VKAIDiagnosticsConstants.Tags.ProviderName, provider },
            { VKAIDiagnosticsConstants.Tags.ModelName, model },
            { VKAIDiagnosticsConstants.Tags.Success, success }
        };

        if (tenantId is not null)
            tags.Add(VKAIDiagnosticsConstants.Tags.TenantId, tenantId);

        ChatRequestCount.Add(1, tags);
        ChatRequestDuration.Record(durationMs, tags);
    }

    public static void RecordTokenUsage(string provider, string model, long tokens, double? cost = null, string? tenantId = null)
    {
        var tags = new TagList
        {
            { VKAIDiagnosticsConstants.Tags.ProviderName, provider },
            { VKAIDiagnosticsConstants.Tags.ModelName, model }
        };

        if (tenantId is not null)
            tags.Add(VKAIDiagnosticsConstants.Tags.TenantId, tenantId);

        TokenUsage.Add(tokens, tags);
        if (cost.HasValue)
        {
            TokenCost.Add(cost.Value, tags);
        }
    }

    public static void RecordVectorOperation(string operation, bool success, string? provider = null, string? tenantId = null)
    {
        var tags = new TagList
        {
            { VKAIDiagnosticsConstants.Tags.OperationName, operation },
            { VKAIDiagnosticsConstants.Tags.Success, success }
        };

        if (provider is not null)
            tags.Add(VKAIDiagnosticsConstants.Tags.ProviderName, provider);
        if (tenantId is not null)
            tags.Add(VKAIDiagnosticsConstants.Tags.TenantId, tenantId);

        VectorOperationCount.Add(1, tags);
    }

    public static void RecordAudioOperation(string operation, bool success, string? provider = null, string? tenantId = null)
    {
        var tags = new TagList
        {
            { VKAIDiagnosticsConstants.Tags.OperationName, operation },
            { VKAIDiagnosticsConstants.Tags.Success, success }
        };

        if (provider is not null)
            tags.Add(VKAIDiagnosticsConstants.Tags.ProviderName, provider);
        if (tenantId is not null)
            tags.Add(VKAIDiagnosticsConstants.Tags.TenantId, tenantId);

        AudioOperationCount.Add(1, tags);
    }
}
