using System;
using FluentAssertions;
using VK.Blocks.Core.Primitives;

namespace VK.Blocks.Core.UnitTests.Primitives;

public class TestEntity : Entity<int>
{
    public TestEntity() { }
    public TestEntity(int id) : base(id) { }
}

public class TestGuidEntity : Entity
{
    public TestGuidEntity() { }
    public TestGuidEntity(Guid id) : base(id) { }
}

public class EntityTests
{
    [Fact]
    public void Entity_Equality_SameIdAndType_ReturnsTrue()
    {
        // Arrange
        var entity1 = new TestEntity(1);
        var entity2 = new TestEntity(1);

        // Act & Assert
        entity1.Equals(entity2).Should().BeTrue();
        (entity1 == entity2).Should().BeTrue();
        entity1.GetHashCode().Should().Be(entity2.GetHashCode());
    }

    [Fact]
    public void Entity_Equality_DifferentId_ReturnsFalse()
    {
        // Arrange
        var entity1 = new TestEntity(1);
        var entity2 = new TestEntity(2);

        // Act & Assert
        entity1.Equals(entity2).Should().BeFalse();
        (entity1 != entity2).Should().BeTrue();
        entity1.GetHashCode().Should().NotBe(entity2.GetHashCode());
    }

    [Fact]
    public void Entity_Equality_DifferentTypeSameId_ReturnsFalse()
    {
        // Arrange
        var entity1 = new TestEntity(1);
        var entity2 = new OtherTestEntity(1);

        // Act & Assert
        entity1.Equals(entity2).Should().BeFalse();
        (entity1 == entity2).Should().BeFalse();
    }

    [Fact]
    public void Entity_Equality_Null_ReturnsFalse()
    {
        // Arrange
        var entity1 = new TestEntity(1);
        TestEntity? entity2 = null;

        // Act & Assert
        entity1.Equals(entity2).Should().BeFalse();
        (entity1 == entity2).Should().BeFalse();
        (entity1 != entity2).Should().BeTrue();
    }

    [Fact]
    public void Entity_Equality_SameReference_ReturnsTrue()
    {
        var entity = new TestEntity(1);
        entity.Equals(entity).Should().BeTrue();
    }

    [Fact]
    public void ParameterlessConstructor_CanBeInvoked()
    {
        var entity = new TestEntity();
        entity.Id.Should().Be(0);
    }

    [Fact]
    public void GuidEntity_ParameterlessConstructor_CanBeInvoked()
    {
        var entity = new TestGuidEntity();
        entity.Id.Should().Be(Guid.Empty);
    }

    [Fact]
    public void GuidEntity_InheritsProperly_AndFunctions()
    {
        // Arrange
        var id = Guid.NewGuid();
        var entity = new TestGuidEntity(id);

        // Act & Assert
        entity.Id.Should().Be(id);
    }

    private class OtherTestEntity : Entity<int>
    {
        public OtherTestEntity(int id) : base(id) { }
    }
}
