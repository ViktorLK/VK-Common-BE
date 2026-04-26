namespace VK.Blocks.Core.UnitTests.Domain;

public class TestAggregateRoot : VKAggregateRoot<int>
{
    public TestAggregateRoot() { }
    public TestAggregateRoot(int id) : base(id) { }

    public void DoSomethingThatRaisesEvent()
    {
        RaiseDomainEvent(new TestDomainEvent());
    }
}
