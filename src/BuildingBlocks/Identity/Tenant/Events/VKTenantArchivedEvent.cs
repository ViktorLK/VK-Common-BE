using System;
using VK.Blocks.Core;

namespace VK.Blocks.Identity;

/// <summary>
/// Domain event dispatched when a tenant account is permanently archived.
/// </summary>
public sealed record VKTenantArchivedEvent(
    VKTenantId TenantId,
    DateTimeOffset ArchivedAt) : IVKDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTimeOffset OccurredOn { get; } = ArchivedAt;
}
