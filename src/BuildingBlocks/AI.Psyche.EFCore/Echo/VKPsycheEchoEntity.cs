using System;
using System.ComponentModel.DataAnnotations;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Psyche.EFCore;

/// <summary>
/// Database entity representing a chat message echo in Psyche short-term memory.
/// Follows CS.05, CS.08.
/// </summary>
[VKPersistEntity(typeof(VKEchoTrace), TableName = "VK_AI_Psyche_Echo")]
public sealed class VKPsycheEchoEntity : IVKTenantScoped, IVKAuditable
{
    /// <inheritdoc />
    [VKPersistIndex(Group = "Tenant_Session_Timestamp", Order = 1)]
    public VKTenantId TenantId { get; set; }

    /// <summary>
    /// Gets or sets the unique strongly-typed echo message trace identifier.
    /// </summary>
    [VKPersistKey]
    public required VKEchoId Id { get; set; }

    /// <summary>
    /// Gets or sets the parent session identifier foreign key.
    /// </summary>
    [VKPersistIndex(Group = "Tenant_Session_Timestamp", Order = 2)]
    public required VKSessionId SessionId { get; set; }

    /// <summary>
    /// Gets or sets the chat message author role (User, Assistant, System).
    /// </summary>
    public VKChatRole Role { get; set; } = VKChatRole.User;

    /// <summary>
    /// Gets or sets the dialogue message body text.
    /// </summary>
    [Required]
    [MaxLength(16000)]
    public required string Content { get; set; }

    /// <summary>
    /// Gets or sets the calculated token cost for this message content.
    /// </summary>
    public int TokenCount { get; set; }

    /// <inheritdoc />
    [VKPersistIndex(Group = "Tenant_Session_Timestamp", Order = 3)]
    public DateTimeOffset CreatedAt { get; set; }

    /// <inheritdoc />
    public VKUserId? CreatedBy { get; set; }

    /// <inheritdoc />
    public DateTimeOffset? UpdatedAt { get; set; }

    /// <inheritdoc />
    public VKUserId? UpdatedBy { get; set; }
}
