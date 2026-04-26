namespace VK.Blocks.Core.UnitTests.Domain;

public class TestGuidAggregateRoot : VKAggregateRoot
{
    public TestGuidAggregateRoot() { }
    public TestGuidAggregateRoot(Guid id) : base(id) { }
}
