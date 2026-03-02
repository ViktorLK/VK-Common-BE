using System;
using FluentAssertions;
using VK.Blocks.Core.Events;
using VK.Blocks.Core.Primitives;

namespace VK.Blocks.Core.UnitTests.Primitives;

public class TestDomainEvent : IDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTimeOffset OccurredOn { get; } = DateTimeOffset.UtcNow;
}

public class TestAggregateRoot : AggregateRoot<int>
{
    public TestAggregateRoot() { }
    public TestAggregateRoot(int id) : base(id) { }

    public void DoSomethingThatRaisesEvent()
    {
        RaiseDomainEvent(new TestDomainEvent());
    }
}

public class TestGuidAggregateRoot : AggregateRoot
{
    public TestGuidAggregateRoot() { }
    public TestGuidAggregateRoot(Guid id) : base(id) { }
}

public class AggregateRootTests
{
    [Fact]
    public void AggregateRoot_RaiseDomainEvent_AddsEventToList()
    {
        // Arrange
        var aggregate = new TestAggregateRoot(1);

        // Act
        aggregate.DoSomethingThatRaisesEvent();

        // Assert
        aggregate.DomainEvents.Should().HaveCount(1);
        aggregate.DomainEvents.First().Should().BeOfType<TestDomainEvent>();
    }

    [Fact]
    public void AggregateRoot_ClearDomainEvents_EmptiesList()
    {
        // Arrange
        var aggregate = new TestAggregateRoot(1);
        aggregate.DoSomethingThatRaisesEvent();

        // Act
        aggregate.ClearDomainEvents();

        // Assert
        aggregate.DomainEvents.Should().BeEmpty();
    }

    [Fact]
    public void AggregateRoot_ParameterlessConstructor_CanBeInvoked()
    {
        var root = new TestAggregateRoot();
        root.Id.Should().Be(0);
        root.DomainEvents.Should().BeEmpty();
    }

    [Fact]
    public void GuidAggregateRoot_Constructors_FunctionCorrectly()
    {
        var id = Guid.NewGuid();
        var root = new TestGuidAggregateRoot(id);
        root.Id.Should().Be(id);

        var defaultRoot = new TestGuidAggregateRoot();
        defaultRoot.Id.Should().Be(Guid.Empty);
    }
}
