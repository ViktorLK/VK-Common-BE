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
    /// Gets the session execution mode (Isolated, Continuous).
    /// </summary>
    public VKSessionMode Mode { get; private set; }

    /// <summary>
    /// Gets a value indicating whether this session operates under Sandbox trial mode (bypasses L2/L3 memory persistence).
    /// </summary>
    public bool IsSandbox { get; private set; }

    /// <summary>
    /// Gets the optional parent session identifier for continuous dialogue chaining.
    /// </summary>
    public VKSessionId? ParentSessionId { get; private set; }

    /// <summary>
    /// Gets the optional source session identifier from which this session was branched.
    /// </summary>
    public VKSessionId? ForkSourceSessionId { get; private set; }

    /// <summary>
    /// Gets the optional strongly-typed echo checkpoint identifier where the fork occurred.
    /// </summary>
    public VKEchoId? ForkPointEchoId { get; private set; }

    /// <summary>
    /// Gets the operational lifecycle status of the session thread.
    /// </summary>
    public VKSessionStatus Status { get; private set; }

    /// <summary>
    /// Gets the total number of dialogue turns recorded in this session.
    /// </summary>
    public int TurnCount { get; private set; }

    /// <summary>
    /// Gets the baseline turn offset inherited from parent session in Continuous mode.
    /// </summary>
    public int BaseTurnOffset { get; private set; }

    /// <summary>
    /// Gets the absolute turn count across the session lineage (BaseTurnOffset + TurnCount).
    /// </summary>
    public int AbsoluteTurnCount => BaseTurnOffset + TurnCount;

    /// <summary>
    /// Gets the timestamp when the session thread was created.
    /// </summary>
    public DateTimeOffset CreatedAt { get; private set; }

    /// <summary>
    /// Gets the timestamp when the session thread was last updated.
    /// </summary>
    public DateTimeOffset? UpdatedAt { get; private set; }

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
        VKEchoId? forkPointEchoId,
        VKSessionStatus status,
        int turnCount,
        DateTimeOffset createdAt,
        DateTimeOffset? updatedAt,
        DateTimeOffset? lastActivityAt,
        byte[]? rowVersion = null,
        int baseTurnOffset = 0,
        bool isSandbox = false) : base(id)
    {
        Mode = mode;
        ParentSessionId = parentSessionId;
        ForkSourceSessionId = forkSourceSessionId;
        ForkPointEchoId = forkPointEchoId;
        Status = status;
        TurnCount = turnCount;
        CreatedAt = createdAt;
        UpdatedAt = updatedAt;
        LastActivityAt = lastActivityAt;
        RowVersion = rowVersion ?? [];
        BaseTurnOffset = baseTurnOffset;
        IsSandbox = isSandbox;
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
        VKEchoId? forkPointEchoId = null,
        int baseTurnOffset = 0,
        bool isSandbox = false)
    {
        // [AP.01]
        VKGuard.NotDefault(id);

        if (baseTurnOffset < 0)
        {
            return VKResult.Failure<VKSessionThread>(VKSessionErrors.InvalidTurnCount);
        }

        if (forkSourceSessionId.HasValue && !forkPointEchoId.HasValue)
        {
            return VKResult.Failure<VKSessionThread>(VKSessionErrors.MissingForkPoint);
        }

        // [CS.08] UpdatedAt is null on creation, updated only on actual modification.
        var thread = new VKSessionThread(
            id: id,
            mode: mode,
            parentSessionId: parentSessionId,
            forkSourceSessionId: forkSourceSessionId,
            forkPointEchoId: forkPointEchoId,
            status: VKSessionStatus.Active,
            turnCount: 0,
            createdAt: now,
            updatedAt: null,
            lastActivityAt: now,
            baseTurnOffset: baseTurnOffset,
            isSandbox: isSandbox);

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
        VKEchoId? forkPointEchoId,
        VKSessionStatus status,
        int turnCount,
        DateTimeOffset createdAt,
        DateTimeOffset? updatedAt,
        DateTimeOffset? lastActivityAt,
        byte[]? rowVersion = null,
        int baseTurnOffset = 0,
        bool isSandbox = false)
    {
        return new VKSessionThread(
            id,
            mode,
            parentSessionId,
            forkSourceSessionId,
            forkPointEchoId,
            status,
            turnCount,
            createdAt,
            updatedAt,
            lastActivityAt,
            rowVersion,
            baseTurnOffset,
            isSandbox);
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
    /// Sets the baseline turn offset inherited from parent session in continuous lineage.
    /// </summary>
    public VKResult SetBaseTurnOffset(int offset)
    {
        if (offset < 0)
        {
            return VKResult.Failure(VKSessionErrors.InvalidTurnCount);
        }

        BaseTurnOffset = offset;
        return VKResult.Success();
    }

    /// <summary>
    /// Creates a continuous child session derived from this session thread, inheriting turn offset and lineage.
    /// </summary>
    public VKResult<VKSessionThread> CreateContinuousChild(
        VKSessionId childSessionId,
        DateTimeOffset now,
        bool? isSandbox = null)
    {
        VKGuard.NotDefault(childSessionId);

        return Create(
            id: childSessionId,
            now: now,
            mode: VKSessionMode.Continuous,
            parentSessionId: Id,
            forkSourceSessionId: null,
            forkPointEchoId: null,
            baseTurnOffset: AbsoluteTurnCount,
            isSandbox: isSandbox ?? IsSandbox);
    }

    /// <summary>
    /// Creates a forked child session derived from this session thread at the given echo checkpoint.
    /// Forked sessions branch into a new dialogue thread inheriting history and mode up to the checkpoint.
    /// </summary>
    public VKResult<VKSessionThread> Fork(
        VKSessionId newSessionId,
        VKEchoId forkPointEchoId,
        DateTimeOffset now,
        int baseTurnOffset = 0,
        bool? isSandbox = null)
    {
        // [AP.01]
        VKGuard.NotDefault(newSessionId);
        VKGuard.NotDefault(forkPointEchoId);

        return Create(
            id: newSessionId,
            now: now,
            mode: Mode,
            parentSessionId: Id,
            forkSourceSessionId: Id,
            forkPointEchoId: forkPointEchoId,
            baseTurnOffset: baseTurnOffset,
            isSandbox: isSandbox ?? IsSandbox);
    }
}
