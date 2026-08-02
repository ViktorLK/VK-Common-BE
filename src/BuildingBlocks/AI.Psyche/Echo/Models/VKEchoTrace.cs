using System;
using VK.Blocks.Core;

namespace VK.Blocks.AI.Psyche;

/// <summary>
/// A single pulse of conversation history representing an echo in short term memory.
/// Follows AP.01 (sealed record for immutability). Implements <see cref="IVKTenantScoped"/>.
/// </summary>
public sealed record VKEchoTrace : IVKFragmentMetadata, IVKTenantScoped
{
    /// <summary>
    /// Gets the tenant identifier for multi-tenant SaaS isolation. Defaults to <see cref="VKTenantId.Default"/>.
    /// </summary>
    public VKTenantId TenantId { get; init; } = VKTenantId.Default;

    public required VKChatRole Role { get; init; }
    public required string Content { get; init; }
    public DateTimeOffset Timestamp { get; init; }
}
