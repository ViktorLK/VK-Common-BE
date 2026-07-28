using System;
using VK.Blocks.AI.Psyche;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Engram;

/// <summary>
/// Represents a structured deterministic fact entry.
/// Follows AP.01 (sealed record, required properties).
/// </summary>
public sealed record VKStructuredFact : IVKFragmentMetadata
{
    /// <summary>
    /// Fact key identifier.
    /// </summary>
    public required string Key { get; init; }

    /// <summary>
    /// Raw fact value.
    /// </summary>
    public required object Value { get; init; }

    /// <summary>
    /// Expected CLR type for schema validation.
    /// </summary>
    public Type? ExpectedType { get; init; }

    /// <summary>
    /// Scope/Tenant identifier for multi-tenant isolation.
    /// </summary>
    public VKTenantId? TenantId { get; init; }

    /// <summary>
    /// Timestamp when fact was created.
    /// </summary>
    public DateTimeOffset StoredAt { get; init; }

    /// <summary>
    /// Timestamp when fact was updated.
    /// </summary>
    public DateTimeOffset? UpdatedAt { get; init; }

    /// <summary>
    /// Indicates whether value contains sensitive information (PII) requiring masking in logs/telemetry.
    /// </summary>
    public bool IsSensitive { get; init; }
}
