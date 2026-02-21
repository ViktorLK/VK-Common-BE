using FluentAssertions;
using Microsoft.EntityFrameworkCore.Query;
using System.Linq.Expressions;
using VK.Blocks.Persistence.EFCore.Repositories;
using Xunit;

namespace VK.Blocks.Persistence.EFCore.IntegrationTests.Repositories;

public class EfCorePropertySetterTests
{
    public class TestEntity
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int Value { get; set; }
    }

    [Fact]
    public void BuildSetPropertyExpression_ShouldChainSetPropertyCalls()
    {
        // Arrange
        var setter = new EfCorePropertySetter<TestEntity>();

        // Act
        setter.SetProperty(e => e.Name, "New Name");
        setter.SetProperty(e => e.Value, 100);

        var expression = setter.BuildSetPropertyExpression();

        // Assert
        expression.Should().NotBeNull();

        // Structure verification:
        // Expected structure is roughly: calls => calls.SetProperty(e => e.Name, "New Name").SetProperty(e => e.Value, 100)

        // We can inspect the Body of the expression
        expression.Body.Should().BeAssignableTo<MethodCallExpression>();
        var outerCall = (MethodCallExpression)expression.Body;

        // The outer call should be the SECOND SetProperty (Value)
        outerCall.Method.Name.Should().Be("SetProperty");
        outerCall.Arguments.Should().HaveCount(2); // propertyExpression, valueExpression

        // Inspect the value argument for the second call
        var valueArg = outerCall.Arguments[1];
        valueArg.Should().BeAssignableTo<ConstantExpression>();
        ((ConstantExpression)valueArg).Value.Should().Be(100);

        // The 'instance' (first arg of static extension method or object of instance method)
        // Wait, SetProperty is an extension method on SetPropertyCalls<T>.
        // BUT EfCoreMethodInfoCache says it's likely using the method info found on SetPropertyCalls<T>.
        // Let's assume standard EF Core usage: SetPropertyCalls<T>.SetProperty(...)

        // Wait, EfCorePropertySetter uses:
        // _currentExpressionChain = Expression.Call(_currentExpressionChain, method, ...)
        // The calls are chained on the result of the previous call.

        // So `outerCall` object (instance) is the result of the previous SetProperty.
        outerCall.Object.Should().NotBeNull();
        outerCall.Object.Should().BeAssignableTo<MethodCallExpression>();

        var innerCall = (MethodCallExpression)outerCall.Object!;
        innerCall.Method.Name.Should().Be("SetProperty");

        // Inspect valid argument for first call
        var innerValueArg = innerCall.Arguments[1];
        innerValueArg.Should().BeAssignableTo<ConstantExpression>();
        ((ConstantExpression)innerValueArg).Value.Should().Be("New Name");
    }

    [Fact]
    public void BuildSetPropertyExpression_WithEmptySetters_ShouldReturnParameter()
    {
        // Arrange
        var setter = new EfCorePropertySetter<TestEntity>();

        // Act
        var expression = setter.BuildSetPropertyExpression();

        // Assert
        expression.Body.Should().BeAssignableTo<ParameterExpression>();
    }
    [Fact]
    public void BuildSetPropertyExpression_WithExpressionValues_ShouldChainSetPropertyCalls()
    {
        // Arrange
        var setter = new EfCorePropertySetter<TestEntity>();

        // Act
        // Set Value = Value + 1
        setter.SetProperty(e => e.Value, e => e.Value + 1);

        var expression = setter.BuildSetPropertyExpression();

        // Assert
        expression.Should().NotBeNull();
        expression.Body.Should().BeAssignableTo<MethodCallExpression>();
        var outerCall = (MethodCallExpression)expression.Body;

        outerCall.Method.Name.Should().Be("SetProperty");
        outerCall.Arguments.Should().HaveCount(2);

        // Value expression
        var valueArg = outerCall.Arguments[1];
        valueArg.Should().BeAssignableTo<LambdaExpression>(); // It's an Expression<Func<..>>

        // We could verify the content of lambda "e => e.Value + 1" but determining structure is enough for coverage
    }
}
