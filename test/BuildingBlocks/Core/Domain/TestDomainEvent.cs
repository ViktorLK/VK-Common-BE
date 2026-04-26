namespace VK.Blocks.Core.UnitTests.Domain;

public class TestDomainEvent : IVKDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTimeOffset OccurredOn { get; } = DateTimeOffset.UtcNow;
}
