using System;
using VK.Blocks.AI;
using VK.Blocks.AI.Psyche;
using VK.Blocks.Core;

namespace VK.Labs.PersonaWeavePulsar.Psyche.Echo.Entities;

/// <summary>
/// Database entity representing a record in the VK_AI_Chat_Message SQLite table.
/// </summary>
public sealed class PwpEchoEntity : IVKMultiTenantEntity
{
    public VKTenantId? TenantId { get; set; }
    public required VKEchoId Id { get; set; }
    public required VKSessionId SessionId { get; set; }
    public VKChatRole Role { get; set; } = VKChatRole.User;
    public string Content { get; set; } = string.Empty;
    public int TokenCount { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}
