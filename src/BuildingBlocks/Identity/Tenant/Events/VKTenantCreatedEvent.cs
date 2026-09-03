using System;
using VK.Blocks.Core;

namespace VK.Blocks.Identity;

/// <summary>
/// Domain event emitted when a new tenant aggregate root is created.
/// </summary>
public sealed record VKTenantCreatedEvent(
    VKTenantId TenantId,
    string Name,
    VKUserId OwnerUserId,
    DateTimeOffset CreatedAt) : IVKDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTimeOffset OccurredOn { get; } = CreatedAt;
}
