using System.Linq.Expressions;
using VK.Blocks.Core.Utilities.Internal;

namespace VK.Blocks.Core.UnitTests.Utilities.Internal;

public class ExpressionEqualityComparerTests
{
    private readonly VKExpressionEqualityComparer _comparer = VKExpressionEqualityComparer.Instance;

    [Fact]
    public void Equals_SameReference_ReturnsTrue()
    {
        Expression<Func<int, int>> expr = x => x;
        _comparer.Equals(expr, expr).Should().BeTrue();
    }

    [Fact]
    public void Equals_NullValues_ReturnsCorrectResult()
    {
        _comparer.Equals(null, null).Should().BeTrue();
        _comparer.Equals(Expression.Constant(1), null).Should().BeFalse();
        _comparer.Equals(null, Expression.Constant(1)).Should().BeFalse();
    }

    [Fact]
    public void Equals_DifferentNodeTypes_ReturnsFalse()
    {
        Expression expr1 = Expression.Constant(1);
        Expression expr2 = Expression.Parameter(typeof(int), "x");
        _comparer.Equals(expr1, expr2).Should().BeFalse();
    }

    [Fact]
    public void Equals_ConstantExpressions_EvaluatesValue()
    {
        var expr1 = Expression.Constant(1);
        var expr2 = Expression.Constant(1);
        var expr3 = Expression.Constant(2);

        _comparer.Equals(expr1, expr2).Should().BeTrue();
        _comparer.Equals(expr1, expr3).Should().BeFalse();
    }

    [Fact]
    public void Equals_ParameterExpressions_EvaluatesNameAndType()
    {
        var p1 = Expression.Parameter(typeof(int), "x");
        var p2 = Expression.Parameter(typeof(int), "x");
        var p3 = Expression.Parameter(typeof(int), "y");
        var p4 = Expression.Parameter(typeof(long), "x");

        _comparer.Equals(p1, p2).Should().BeTrue();
        _comparer.Equals(p1, p3).Should().BeFalse();
        _comparer.Equals(p1, p4).Should().BeFalse();
    }

    [Fact]
    public void Equals_BinaryExpressions_EvaluatesOperands()
    {
        Expression<Func<int, int, int>> expr1 = (a, b) => a + b;
        Expression<Func<int, int, int>> expr2 = (a, b) => a + b;
        Expression<Func<int, int, int>> expr3 = (a, b) => a - b;

        _comparer.Equals(expr1, expr2).Should().BeTrue();
        _comparer.Equals(expr1, expr3).Should().BeFalse();
    }

    [Fact]
    public void Equals_MemberExpressions_EvaluatesMember()
    {
        Expression<Func<string, int>> expr1 = s => s.Length;
        Expression<Func<string, int>> expr2 = s => s.Length;

        _comparer.Equals(expr1.Body, expr2.Body).Should().BeTrue();
    }

    [Fact]
    public void Equals_MethodCallExpressions_EvaluatesMethodAndArguments()
    {
        Expression<Func<string, bool>> expr1 = s => s.Contains("a");
        Expression<Func<string, bool>> expr2 = s => s.Contains("a");
        Expression<Func<string, bool>> expr3 = s => s.Contains("b");

        _comparer.Equals(expr1.Body, expr2.Body).Should().BeTrue();
        _comparer.Equals(expr1.Body, expr3.Body).Should().BeFalse();
    }

    [Fact]
    public void Equals_UnaryExpressions_EvaluatesOperand()
    {
        Expression<Func<int, int>> expr1 = x => -x;
        Expression<Func<int, int>> expr2 = x => -x;

        _comparer.Equals(expr1.Body, expr2.Body).Should().BeTrue();
    }

    [Fact]
    public void Equals_NewExpressions_EvaluatesConstructorAndArguments()
    {
        Expression<Func<DateTime>> expr1 = () => new DateTime(2020, 1, 1);
        Expression<Func<DateTime>> expr2 = () => new DateTime(2020, 1, 1);
        Expression<Func<DateTime>> expr3 = () => new DateTime(2021, 1, 1);

        _comparer.Equals(expr1.Body, expr2.Body).Should().BeTrue();
        _comparer.Equals(expr1.Body, expr3.Body).Should().BeFalse();
    }

    [Fact]
    public void GetHashCode_DifferentExpressions_ReturnsDifferentHashes()
    {
        var expr1 = Expression.Constant(1);
        var expr2 = Expression.Constant(2);
        var expr3 = Expression.Parameter(typeof(int), "x");

        _comparer.GetHashCode(expr1).Should().NotBe(_comparer.GetHashCode(expr2));
        _comparer.GetHashCode(expr1).Should().NotBe(_comparer.GetHashCode(expr3));
    }
}
