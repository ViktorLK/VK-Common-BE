using System;
using System.ComponentModel.DataAnnotations;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Psyche.EFCore;

/// <summary>
/// Database entity representing a chat session thread.
/// Pure persistence model for Psyche IVKSessionStore. [CS.05] [CS.08]
/// </summary>
[VKPersistEntity(typeof(VKSessionThread), TableName = "VK_AI_Psyche_Session")]
public sealed class VKPsycheSessionEntity : IVKTenantScoped, IVKAuditable, IVKConcurrency
{
    /// <inheritdoc />
    [VKPersistIndex]
    public VKTenantId TenantId { get; set; }

    /// <summary>
    /// Gets or sets the unique strongly-typed session identifier.
    /// </summary>
    [VKPersistKey]
    public required VKSessionId Id { get; set; }

    /// <summary>
    /// Gets or sets the session execution mode (Isolated, Shared, Forked).
    /// </summary>
    public VKSessionMode Mode { get; set; } = VKSessionMode.Isolated;

    /// <summary>
    /// Gets or sets the parent session identifier for hierarchy or tree tracking.
    /// </summary>
    [VKPersistIndex]
    public VKSessionId? ParentSessionId { get; set; }

    /// <summary>
    /// Gets or sets the source session identifier if this session was branched/forked.
    /// </summary>
    public VKSessionId? ForkSourceSessionId { get; set; }

    /// <summary>
    /// Gets or sets the optional message ID or checkpoint reference where the fork occurred.
    /// </summary>
    [MaxLength(256)]
    public string? ForkPointRef { get; set; }

    /// <summary>
    /// Gets or sets the current lifecycle status of the session.
    /// </summary>
    public VKSessionStatus Status { get; set; } = VKSessionStatus.Active;

    /// <summary>
    /// Gets or sets the total number of dialogue turns recorded in this session.
    /// </summary>
    public int TurnCount { get; set; }

    /// <summary>
    /// Gets or sets the dynamic knowledge activation state and token tracking.
    /// </summary>
    [VKPersistJson(MaxLength = 8000)]
    public VKSessionKnowledgeState KnowledgeState { get; set; } = new();

    /// <inheritdoc />
    public DateTimeOffset CreatedAt { get; set; }

    /// <inheritdoc />
    public VKUserId? CreatedBy { get; set; }

    /// <inheritdoc />
    public DateTimeOffset? UpdatedAt { get; set; }

    /// <inheritdoc />
    public VKUserId? UpdatedBy { get; set; }

    /// <summary>
    /// Gets or sets the timestamp of the latest interaction or message in this session.
    /// </summary>
    public DateTimeOffset? LastActivityAt { get; set; }

    /// <inheritdoc />
    public byte[] RowVersion { get; set; } = [];
}
