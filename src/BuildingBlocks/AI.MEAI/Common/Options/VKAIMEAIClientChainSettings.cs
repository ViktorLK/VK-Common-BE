using System.Collections.Generic;

namespace VK.Blocks.AI.MEAI;

/// <summary>
/// Configuration options governing the MEAI DelegatingChatClient pipeline.
/// Adheres to ARCH-05 and AP.06: explicit toggle settings with deterministic order.
/// </summary>
public sealed record VKAIMEAIClientChainSettings
{
    /// <summary>
    /// Gets or sets whether OpenTelemetry tracing and metrics instrumentation is enabled.
    /// Default is true.
    /// </summary>
    public bool EnableOpenTelemetry { get; init; } = true;

    /// <summary>
    /// Gets or sets whether structured request/response auditing via [LoggerMessage] is enabled.
    /// Default is true.
    /// </summary>
    public bool EnableAudit { get; init; } = true;

    /// <summary>
    /// Gets or sets whether prompt injection detection is enabled.
    /// Default is true.
    /// </summary>
    public bool EnableInjectionGuard { get; init; } = true;

    /// <summary>
    /// Gets or sets whether sensitive PII masking is enabled.
    /// Default is true.
    /// </summary>
    public bool EnablePrivacy { get; init; } = true;

    /// <summary>
    /// Gets or sets whether token governance and context pruning is enabled.
    /// Default is true.
    /// </summary>
    public bool EnableTokenGovernance { get; init; } = true;

    /// <summary>
    /// Gets or sets the maximum allowed tokens before triggering pruning or rejection.
    /// Default is 8192.
    /// </summary>
    public int MaxContextTokens { get; init; } = 8192;

    /// <summary>
    /// Gets or sets whether response caching and idempotency handling is enabled.
    /// Default is false.
    /// </summary>
    public bool EnableCaching { get; init; } = false;

    /// <summary>
    /// Gets or sets whether function invocation middleware is enabled.
    /// Default is true.
    /// </summary>
    public bool EnableFunctionInvocation { get; init; } = true;

    /// <summary>
    /// Gets or sets the list of enabled tool names for function invocation (AP.06).
    /// Empty list means all registered tools are enabled.
    /// </summary>
    public IReadOnlyList<string> EnabledTools { get; init; } = [];
}

