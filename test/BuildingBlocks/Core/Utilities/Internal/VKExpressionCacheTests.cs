using System.Linq.Expressions;
using VK.Blocks.Core.Utilities.Internal;

namespace VK.Blocks.Core.UnitTests.Utilities.Internal;

public class VKExpressionCacheTests
{
    [Fact]
    public void GetOrCompile_ShouldCompileAndCacheExpression()
    {
        // Arrange
        Expression<Func<int, int>> expr = x => x * 2;

        // Act
        var func1 = VKExpressionCache.GetOrCompile(expr);
        var func2 = VKExpressionCache.GetOrCompile(expr);

        // Assert
        func1.Should().NotBeNull();
        func1(5).Should().Be(10);
        func2.Should().BeSameAs(func1);
    }

    [Fact]
    public void GetCount_ShouldReturnNumberOfCachedItems()
    {
        // Arrange
        VKExpressionCache.Clear<string, int>();
        Expression<Func<string, int>> expr1 = s => s.Length;
        Expression<Func<string, int>> expr2 = s => s.GetHashCode();

        // Act
        VKExpressionCache.GetOrCompile(expr1);
        VKExpressionCache.GetOrCompile(expr2);

        // Assert
        VKExpressionCache.GetCount<string, int>().Should().Be(2);
    }

    [Fact]
    public void Clear_ShouldRemoveAllCachedItems()
    {
        // Arrange
        Expression<Func<double, double>> expr = x => x + 1;
        VKExpressionCache.GetOrCompile(expr);
        VKExpressionCache.GetCount<double, double>().Should().BeGreaterThan(0);

        // Act
        VKExpressionCache.Clear<double, double>();

        // Assert
        VKExpressionCache.GetCount<double, double>().Should().Be(0);
    }
}
