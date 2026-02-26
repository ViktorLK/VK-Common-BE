using System;
using FluentAssertions;
using VK.Blocks.Core.Primitives;
using VK.Blocks.Persistence.EFCore.Caches;
using VK.Blocks.Persistence.EFCore.Tests;
using Xunit;

namespace VK.Blocks.Persistence.EFCore.IntegrationTests.Caches;

/// <summary>
/// Unit tests for the EF Core caching mechanisms.
/// </summary>
public class EfCoreCacheTests
{
    /// <summary>
    /// Verifies that <see cref="EfCoreTypeCache{T}"/> correctly detects interface implementation on types.
    /// </summary>
    [Fact]
    public void EfCoreTypeCache_DetectsInterfacesCorrectly()
    {
        // Rationale: TestProduct implements IAuditable and ISoftDelete.
        EfCoreTypeCache<TestProduct>.IsAuditable.Should().BeTrue();
        EfCoreTypeCache<TestProduct>.IsSoftDelete.Should().BeTrue();

        // Rationale: Object implements neither of the target interfaces.
        EfCoreTypeCache<object>.IsAuditable.Should().BeFalse();
        EfCoreTypeCache<object>.IsSoftDelete.Should().BeFalse();

        // Rationale: Custom class implementing only one of the interfaces.
        EfCoreTypeCache<OnlyAuditable>.IsAuditable.Should().BeTrue();
        EfCoreTypeCache<OnlyAuditable>.IsSoftDelete.Should().BeFalse();
    }
#if NET8_0
    /// <summary>
    /// Verifies that <see cref="EfCoreMethodInfoCache{T}"/> correctly resolves method information.
    /// </summary>
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
#endif
    /// <summary>
    /// Verifies that <see cref="EfCoreExpressionCache{TEntity, TProperty}"/> correctly caches compiled delegates.
    /// </summary>
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

    /// <summary>
    /// Verifies that <see cref="EfCoreExpressionCache{TEntity, TProperty}.Clear"/> correctly resets the cache.
    /// </summary>
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

    /// <summary>
    /// A test class that only implements <see cref="IAuditable"/>.
    /// </summary>
    public class OnlyAuditable : IAuditable
    {
        public DateTimeOffset CreatedAt { get; set; }
        public string? CreatedBy { get; set; }
        public DateTimeOffset? UpdatedAt { get; set; }
        public string? UpdatedBy { get; set; }
    }
}
