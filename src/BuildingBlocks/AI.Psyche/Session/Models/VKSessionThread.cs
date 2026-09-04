using System;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Psyche;

/// <summary>
/// Domain aggregate root representing a conversation session thread, lineage, and lifecycle.
/// Follows AP.01, CS.01, CS.05.
/// </summary>
public sealed class VKSessionThread : VKAggregateRoot<VKSessionId>, IVKConcurrency
{
    // =========================================================================
    // Properties
    // =========================================================================

    /// <summary>
    /// Gets the session execution mode (Isolated, Shared, Forked).
    /// </summary>
    public VKSessionMode Mode { get; private set; }

    /// <summary>
    /// Gets the optional parent session identifier.
    /// </summary>
    public VKSessionId? ParentSessionId { get; private set; }

    /// <summary>
    /// Gets the optional source session identifier from which this session was branched.
    /// </summary>
    public VKSessionId? ForkSourceSessionId { get; private set; }

    /// <summary>
    /// Gets the optional message ID or checkpoint reference where the fork occurred.
    /// </summary>
    public string? ForkPointRef { get; private set; }

    /// <summary>
    /// Gets the operational lifecycle status of the session thread.
    /// </summary>
    public VKSessionStatus Status { get; private set; }

    /// <summary>
    /// Gets the total number of dialogue turns recorded in this session.
    /// </summary>
    public int TurnCount { get; private set; }

    /// <summary>
    /// Gets the dynamic knowledge activation state and token tracking.
    /// </summary>
    public VKSessionKnowledgeState KnowledgeState { get; private set; }

    /// <summary>
    /// Gets the timestamp when the session thread was created.
    /// </summary>
    public DateTimeOffset CreatedAt { get; private set; }

    /// <summary>
    /// Gets the timestamp when the session thread was last updated.
    /// </summary>
    public DateTimeOffset UpdatedAt { get; private set; }

    /// <summary>
    /// Gets the timestamp of the latest interaction or message in this session.
    /// </summary>
    public DateTimeOffset? LastActivityAt { get; private set; }

    /// <inheritdoc />
    public byte[] RowVersion { get; set; } = [];

    // =========================================================================
    // Constructor (Private)
    // =========================================================================

    private VKSessionThread(
        VKSessionId id,
        VKSessionMode mode,
        VKSessionId? parentSessionId,
        VKSessionId? forkSourceSessionId,
        string? forkPointRef,
        VKSessionStatus status,
        int turnCount,
        VKSessionKnowledgeState? knowledgeState,
        DateTimeOffset createdAt,
        DateTimeOffset updatedAt,
        DateTimeOffset? lastActivityAt,
        byte[]? rowVersion = null) : base(id)
    {
        Mode = mode;
        ParentSessionId = parentSessionId;
        ForkSourceSessionId = forkSourceSessionId;
        ForkPointRef = forkPointRef;
        Status = status;
        TurnCount = turnCount;
        KnowledgeState = knowledgeState ?? new VKSessionKnowledgeState();
        CreatedAt = createdAt;
        UpdatedAt = updatedAt;
        LastActivityAt = lastActivityAt;
        RowVersion = rowVersion ?? [];
    }

    // =========================================================================
    // Factory Methods
    // =========================================================================

    /// <summary>
    /// Factory method to create a new session thread aggregate root.
    /// </summary>
    public static VKResult<VKSessionThread> Create(
        VKSessionId id,
        DateTimeOffset now,
        VKSessionMode mode = VKSessionMode.Isolated,
        VKSessionId? parentSessionId = null,
        VKSessionId? forkSourceSessionId = null,
        string? forkPointRef = null,
        VKSessionKnowledgeState? knowledgeState = null)
    {
        // [AP.01]
        VKGuard.NotDefault(id);

        var thread = new VKSessionThread(
            id: id,
            mode: mode,
            parentSessionId: parentSessionId,
            forkSourceSessionId: forkSourceSessionId,
            forkPointRef: forkPointRef,
            status: VKSessionStatus.Active,
            turnCount: 0,
            knowledgeState: knowledgeState ?? new VKSessionKnowledgeState(),
            createdAt: now,
            updatedAt: now,
            lastActivityAt: now);

        return VKResult.Success(thread);
    }

    /// <summary>
    /// Rehydration factory used exclusively by persistence mappers to restore persisted state without side effects.
    /// </summary>
    internal static VKSessionThread Rehydrate(
        VKSessionId id,
        VKSessionMode mode,
        VKSessionId? parentSessionId,
        VKSessionId? forkSourceSessionId,
        string? forkPointRef,
        VKSessionStatus status,
        int turnCount,
        VKSessionKnowledgeState knowledgeState,
        DateTimeOffset createdAt,
        DateTimeOffset updatedAt,
        DateTimeOffset? lastActivityAt,
        byte[]? rowVersion = null)
    {
        return new VKSessionThread(
            id,
            mode,
            parentSessionId,
            forkSourceSessionId,
            forkPointRef,
            status,
            turnCount,
            knowledgeState,
            createdAt,
            updatedAt,
            lastActivityAt,
            rowVersion);
    }

    // =========================================================================
    // Behavioral Methods
    // =========================================================================

    /// <summary>
    /// Advances dialogue turn count and refreshes last activity timestamp.
    /// </summary>
    public VKResult IncrementTurn(DateTimeOffset now)
    {
        if (Status != VKSessionStatus.Active)
        {
            return VKResult.Failure(VKSessionErrors.SessionNotActive);
        }

        TurnCount++;
        LastActivityAt = now;
        UpdatedAt = now;

        return VKResult.Success();
    }

    /// <summary>
    /// Updates the session-level knowledge execution tracking state.
    /// </summary>
    public VKResult AdvanceKnowledgeState(VKSessionKnowledgeState knowledgeState, DateTimeOffset now)
    {
        KnowledgeState = VKGuard.NotNull(knowledgeState);

        if (Status != VKSessionStatus.Active)
        {
            return VKResult.Failure(VKSessionErrors.SessionNotActive);
        }

        UpdatedAt = now;
        return VKResult.Success();
    }

    /// <summary>
    /// Changes the operational status of the session thread (e.g. Paused, Closed).
    /// </summary>
    public VKResult ChangeStatus(VKSessionStatus newStatus, DateTimeOffset now)
    {
        if (Status == VKSessionStatus.Closed && newStatus != VKSessionStatus.Closed)
        {
            return VKResult.Failure(VKSessionErrors.SessionNotActive);
        }

        Status = newStatus;
        UpdatedAt = now;

        return VKResult.Success();
    }

    /// <summary>
    /// Closes the session thread permanently.
    /// </summary>
    public VKResult Close(DateTimeOffset now) => ChangeStatus(VKSessionStatus.Closed, now);

    /// <summary>
    /// Creates a forked child session derived from this session thread at the given checkpoint reference.
    /// </summary>
    public VKResult<VKSessionThread> Fork(VKSessionId newSessionId, string forkPointRef, DateTimeOffset now)
    {
        VKGuard.NotDefault(newSessionId);
        VKGuard.NotNullOrWhiteSpace(forkPointRef);

        return Create(
            id: newSessionId,
            now: now,
            mode: Mode,
            parentSessionId: ParentSessionId,
            forkSourceSessionId: Id,
            forkPointRef: forkPointRef,
            knowledgeState: KnowledgeState);
    }
}
