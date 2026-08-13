using System;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Psyche;

/// <summary>
/// Domain model representing a conversation session thread, lineage, and lifecycle.
/// Follows AP.01 (sealed record) and BB.01. Implements <see cref="IVKTenantScoped"/> and <see cref="IVKUserScoped"/>.
/// Order follows TenantId -> UserId -> Id hierarchy with required TenantId.
/// Symmetry with VKEchoTrace, VKPersonaAnchor, and VKDirectiveCharter.
/// </summary>
public sealed record VKSessionThread : IVKTenantScoped, IVKUserScoped
{
    /// <summary>
    /// Gets the tenant identifier for multi-tenant SaaS isolation.
    /// </summary>
    public required VKTenantId TenantId { get; init; }

    /// <summary>
    /// Gets the user identifier for user-level session security boundary. Defaults to <see cref="VKUserId.Anonymous"/>.
    /// </summary>
    public required VKUserId UserId { get; init; }

    /// <summary>
    /// Gets the unique session identifier.
    /// </summary>
    public required VKSessionId Id { get; init; }

    /// <summary>
    /// Gets the target Persona identifier bound to this session thread.
    /// </summary>
    public required VKPersonaId PersonaId { get; init; }

    /// <summary>
    /// Gets the operational mode for this session thread (Isolated, Continuous, Sandbox).
    /// </summary>
    public VKSessionMode Mode { get; init; } = VKSessionMode.Isolated;

    /// <summary>
    /// Gets the optional parent session identifier for continuous memory lineage.
    /// </summary>
    public VKSessionId? ParentSessionId { get; init; }

    /// <summary>
    /// Gets the optional fork source session identifier if this session was forked from another session.
    /// </summary>
    public VKSessionId? ForkSourceSessionId { get; init; }

    /// <summary>
    /// Gets the optional fork point reference (e.g. echo timestamp or turn index) where the fork occurred.
    /// </summary>
    public string? ForkPointRef { get; init; }

    /// <summary>
    /// Gets the current operational lifecycle status of the session thread.
    /// </summary>
    public VKSessionStatus Status { get; init; } = VKSessionStatus.Active;

    /// <summary>
    /// Gets the total turn count executed in this session thread.
    /// </summary>
    public int TurnCount { get; init; }

    /// <summary>
    /// Gets the timestamp when this session thread was created.
    /// </summary>
    public DateTimeOffset CreatedAt { get; init; }

    /// <summary>
    /// Gets the timestamp when this session thread was last updated.
    /// </summary>
    public DateTimeOffset UpdatedAt { get; init; }

    /// <summary>
    /// Gets the timestamp when this session thread was last active (updated on each turn).
    /// </summary>
    public DateTimeOffset? LastActivityAt { get; init; }

    /// <summary>
    /// Gets the session-level knowledge execution tracking state for incremental matching & sliding window lifecycle.
    /// </summary>
    public VKSessionKnowledgeState KnowledgeState { get; init; } = new();
}
