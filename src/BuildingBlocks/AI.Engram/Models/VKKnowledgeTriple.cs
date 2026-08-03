using System;
using VK.Blocks.AI.Psyche;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Engram;

/// <summary>
/// Structured entity-relation-entity triple extracted from memory blocks.
/// Implements <see cref="IVKMultiTenant"/> to satisfy tenant isolation (OR.02).
/// </summary>
public sealed record VKKnowledgeTriple : IVKMultiTenant
{
    /// <summary>
    /// Gets the unique identifier for this triple record.
    /// </summary>
    public Guid Id { get; init; } = Guid.NewGuid();

    /// <summary>
    /// Gets the tenant identifier for strict multi-tenant isolation.
    /// </summary>
    public VKTenantId? TenantId { get; init; }

    /// <summary>
    /// Gets the optional user identifier owning this memory context.
    /// </summary>
    public string? UserId { get; init; }

    /// <summary>
    /// Gets the optional session identifier during which this triple was derived.
    /// </summary>
    public VKSessionId? SessionId { get; init; }

    /// <summary>
    /// Gets the subject entity.
    /// </summary>
    public required string Subject { get; init; }

    /// <summary>
    /// Gets the relationship predicate.
    /// </summary>
    public required string Relation { get; init; }

    /// <summary>
    /// Gets the object entity.
    /// </summary>
    public required string Object { get; init; }

    /// <summary>
    /// Gets the confidence score of this triple (0.0 to 1.0).
    /// </summary>
    public float Confidence { get; init; } = 1.0f;
}
