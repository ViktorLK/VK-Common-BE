using FluentAssertions;
using VK.Blocks.Core.Primitives;
using VK.Blocks.Core.Results;
using VK.Blocks.Persistence.EFCore.Caches;
using VK.Blocks.Persistence.EFCore.Tests;
using Xunit;

namespace VK.Blocks.Persistence.EFCore.IntegrationTests.Caches;

public class EfCoreCacheTests
{
    [Fact]
    public void EfCoreTypeCache_DetectsInterfacesCorrectly()
    {
        // TestProduct implements IAuditable and ISoftDelete
        EfCoreTypeCache<TestProduct>.IsAuditable.Should().BeTrue();
        EfCoreTypeCache<TestProduct>.IsSoftDelete.Should().BeTrue();

        // Object implements neither
        EfCoreTypeCache<object>.IsAuditable.Should().BeFalse();
        EfCoreTypeCache<object>.IsSoftDelete.Should().BeFalse();

        // Custom class implementing only one
        EfCoreTypeCache<OnlyAuditable>.IsAuditable.Should().BeTrue();
        EfCoreTypeCache<OnlyAuditable>.IsSoftDelete.Should().BeFalse();
    }

    [Fact]
    public void EfCoreMethodInfoCache_ResolvesMethods()
    {
        // Act
        var setValMethod = EfCoreMethodInfoCache<TestProduct>.SetPropertyValueMethod;
        var setExprMethod = EfCoreMethodInfoCache<TestProduct>.SetPropertyExpressionMethod;

        // Assert
        setValMethod.Should().NotBeNull();
        setValMethod.Name.Should().Be("SetProperty");

        setExprMethod.Should().NotBeNull();
        setExprMethod.Name.Should().Be("SetProperty");
    }

    [Fact]
    public void EfCoreExpressionCache_GetOrCompile_CachesDelegate()
    {
        // Arrange
        EfCoreExpressionCache<TestProduct, Guid>.Clear();
        System.Linq.Expressions.Expression<Func<TestProduct, Guid>> expr = x => x.Id;

        // Act
        var compiled1 = EfCoreExpressionCache<TestProduct, Guid>.GetOrCompile(expr);
        var compiled2 = EfCoreExpressionCache<TestProduct, Guid>.GetOrCompile(expr);

        // Assert
        compiled1.Should().NotBeNull();
        compiled2.Should().NotBeNull();
        compiled1.Should().BeSameAs(compiled2);
        EfCoreExpressionCache<TestProduct, Guid>.CachedCount.Should().Be(1);
    }

    [Fact]
    public void EfCoreExpressionCache_Clear_ResetsCache()
    {
        // Arrange
        EfCoreExpressionCache<TestProduct, Guid>.Clear();
        System.Linq.Expressions.Expression<Func<TestProduct, Guid>> expr = x => x.Id;
        EfCoreExpressionCache<TestProduct, Guid>.GetOrCompile(expr);

        // Act
        EfCoreExpressionCache<TestProduct, Guid>.Clear();

        // Assert
        EfCoreExpressionCache<TestProduct, Guid>.CachedCount.Should().Be(0);
    }

    public class OnlyAuditable : IAuditable
    {
        public DateTimeOffset CreatedAt { get; set; }
        public string? CreatedBy { get; set; }
        public DateTimeOffset? UpdatedAt { get; set; }
        public string? UpdatedBy { get; set; }
    }
}
