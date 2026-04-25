using VK.Blocks.Core.Utilities.Internal;
namespace VK.Blocks.Core.UnitTests.Utilities.Internal;

public class VKTypeMetadataCacheTests
{
    private interface IOther;

    [AttributeUsage(AttributeTargets.Class)]
    private sealed class TestAttribute(string name) : Attribute
    {
        public string Name { get; } = name;
    }

    [Test("Metadata")]
    private sealed class AuditableEntity : IVKAuditable
    {
        public DateTimeOffset CreatedAt { get; set; }
        public string? CreatedBy { get; set; }
        public DateTimeOffset? UpdatedAt { get; set; }
        public string? UpdatedBy { get; set; }
    }

    private sealed class PlainEntity;

    [Fact]
    public void IsSoftDelete_ReturnsCorrectValue()
    {
        VKTypeMetadataCache.IsSoftDelete<AuditableEntity>().Should().BeFalse();
    }

    [Fact]
    public void IsMultiTenant_ReturnsCorrectValue()
    {
        VKTypeMetadataCache.IsMultiTenant<AuditableEntity>().Should().BeFalse();
    }

    [Fact]
    public void IsAuditable_ReturnsCorrectValue()
    {
        VKTypeMetadataCache.IsAuditable<AuditableEntity>().Should().BeTrue();
        VKTypeMetadataCache.IsAuditable<PlainEntity>().Should().BeFalse();
    }

    [Fact]
    public void Name_ReturnsCorrectValue()
    {
        VKTypeMetadataCache.GetName<AuditableEntity>().Should().Be(nameof(AuditableEntity));
        VKTypeMetadataCache.GetFullName<AuditableEntity>().Should().Contain(nameof(AuditableEntity));
    }

    [Fact]
    public void IsAssignableTo_ReturnsCorrectValue()
    {
        VKTypeMetadataCache.IsAssignableTo<AuditableEntity, IVKAuditable>().Should().BeTrue();
        VKTypeMetadataCache.IsAssignableTo<AuditableEntity, IOther>().Should().BeFalse();
    }

    [Fact]
    public void GetAttribute_ReturnsCorrectValue()
    {
        var attr = VKTypeMetadataCache.GetAttribute<AuditableEntity, TestAttribute>();
        attr.Should().NotBeNull();
        attr!.Name.Should().Be("Metadata");

        VKTypeMetadataCache.GetAttribute<PlainEntity, TestAttribute>().Should().BeNull();
    }

    [Fact]
    public void IsAssignableTo_PrimitiveTypes_ShouldReturnFalse()
    {
        // Act & Assert
        VKTypeMetadataCache.IsAssignableTo<int, string>().Should().BeFalse();
        VKTypeMetadataCache.IsAssignableTo<string, int>().Should().BeFalse();
    }

    [Fact]
    public void GetAttribute_TypeWithoutAttribute_ShouldReturnNull()
    {
        // Act
        var attr = VKTypeMetadataCache.GetAttribute<string, VKBlockDiagnosticsAttribute<VKCoreBlock>>();

        // Assert
        attr.Should().BeNull();
    }
}
