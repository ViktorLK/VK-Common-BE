using System;
using VK.Blocks.Core;

namespace VK.Blocks.Identity;

/// <summary>
/// Domain event emitted when a tenant's ownership is transferred to another user.
/// </summary>
public sealed record VKTenantOwnerTransferredEvent(
    VKTenantId TenantId,
    VKUserId PreviousOwnerUserId,
    VKUserId NewOwnerUserId,
    DateTimeOffset TransferredAt) : IVKDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTimeOffset OccurredOn { get; } = TransferredAt;
}
