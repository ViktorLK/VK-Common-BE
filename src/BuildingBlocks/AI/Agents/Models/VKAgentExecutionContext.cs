using System.Collections.Generic;

namespace VK.Blocks.AI;

/// <summary>
/// Context for agent execution, composing <see cref="VKAtomicToolContext"/> along with
/// boundary-propagated thread, tenant, trace, and conversation history metadata (ARCH-09).
/// </summary>
public sealed record VKAgentExecutionContext
{
    private const string ThreadIdVariableKey = "ThreadId";
    private const string HistoryVariableKey = "history";

    private readonly string? _traceId;

    /// <summary>
    /// Gets the underlying atomic tool execution context.
    /// </summary>
    public VKAtomicToolContext ToolContext { get; init; } = new();

    /// <summary>
    /// Gets the variables/state available to the agent.
    /// </summary>
    public Dictionary<string, object> Variables => ToolContext.Variables;

    /// <summary>
    /// Gets the history of tool calls and results.
    /// </summary>
    public List<VKAtomicToolResult> ToolCallHistory => ToolContext.ToolCallHistory;

    /// <summary>
    /// Gets or initializes the thread/session identifier for restoring and saving transient thread state.
    /// Synchronized with <see cref="Variables"/>[<c>"ThreadId"</c>] for backward compatibility.
    /// </summary>
    public string? ThreadId
    {
        get => Variables.TryGetValue(ThreadIdVariableKey, out var val) ? val as string ?? val?.ToString() : null;
        init
        {
            if (!string.IsNullOrWhiteSpace(value))
            {
                Variables[ThreadIdVariableKey] = value;
            }
            else
            {
                Variables.Remove(ThreadIdVariableKey);
            }
        }
    }

    /// <summary>
    /// Gets the tenant identifier propagated from the execution boundary for observability and scoping.
    /// </summary>
    public string? TenantId { get; init; }

    /// <summary>
    /// Gets the trace/correlation identifier propagated from the execution boundary.
    /// Falls back to <see cref="VKAtomicToolContext.CorrelationId"/> if unset.
    /// </summary>
    public string? TraceId
    {
        get => _traceId ?? ToolContext.CorrelationId;
        init => _traceId = value;
    }

    /// <summary>
    /// Gets or initializes the typed multi-turn conversation history passed into the run.
    /// Synchronized with <see cref="Variables"/>[<c>"history"</c>] for backward compatibility.
    /// </summary>
    public IReadOnlyList<VKChatMessage>? History
    {
        get => Variables.TryGetValue(HistoryVariableKey, out var val) && val is IEnumerable<VKChatMessage> list
            ? (list as IReadOnlyList<VKChatMessage> ?? [.. list])
            : null;
        init
        {
            if (value is not null)
            {
                Variables[HistoryVariableKey] = value;
            }
            else
            {
                Variables.Remove(HistoryVariableKey);
            }
        }
    }

    /// <summary>
    /// Gets the optional per-call maximum number of history messages retained before trimming.
    /// </summary>
    public int? MaxHistoryMessages { get; init; }
}
