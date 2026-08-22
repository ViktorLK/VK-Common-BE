using VK.Blocks.Core;

namespace VK.Blocks.Workflow;

/// <summary>
/// Options for Workflow idempotency and deduplication.
/// </summary>
public sealed partial record VKIdempotencyOptions : IVKBlockOptions
{
    /// <summary>
    /// Gets or sets whether automatic SHA256 payload hashing should be used when no explicit TraceId is provided.
    /// Defaults to true.
    /// </summary>
    public bool AutoHashPayloadOnMissingKey { get; init; } = true;
}
