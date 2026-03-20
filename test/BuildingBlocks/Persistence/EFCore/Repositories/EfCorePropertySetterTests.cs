#if NET8_0
using System;
using System.Linq.Expressions;
using FluentAssertions;
using Microsoft.EntityFrameworkCore.Query;
using VK.Blocks.Persistence.EFCore.Repositories;
using Xunit;

namespace VK.Blocks.Persistence.EFCore.IntegrationTests.Repositories;

/// <summary>
/// Unit tests for <see cref="EfCorePropertySetter{T}"/>.
/// </summary>
public class EfCorePropertySetterTests
{
    /// <summary>
    /// A test entity for expression building tests.
    /// </summary>
    public class TestEntity
    {
        /// <summary>
        /// Gets or sets the entity identifier.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the value.
        /// </summary>
        public int Value { get; set; }
    }

    /// <summary>
    /// Verifies that <see cref="EfCorePropertySetter{T}.BuildSetPropertyExpression"/> correctly chains multiple <c>SetProperty</c> calls.
    /// </summary>
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

        // Rationale: Structure verification. Expected structure is roughly: calls => calls.SetProperty(e => e.Name, "New Name").SetProperty(e => e.Value, 100)
        expression.Body.Should().BeAssignableTo<MethodCallExpression>();
        var outerCall = (MethodCallExpression)expression.Body;

        // Rationale: The outer call should be the SECOND SetProperty (Value) since it was applied last.
        outerCall.Method.Name.Should().Be("SetProperty");
        outerCall.Arguments.Should().HaveCount(2); // propertyExpression, valueExpression

        // Inspect the value argument for the second call
        var valueArg = outerCall.Arguments[1];
        valueArg.Should().BeAssignableTo<ConstantExpression>();
        ((ConstantExpression)valueArg).Value.Should().Be(100);

        // Rationale: In EF Core, SetProperty is an extension method. The 'instance' in our expression tree
        // is the result of the previous call in the chain.
        outerCall.Object.Should().NotBeNull();
        outerCall.Object.Should().BeAssignableTo<MethodCallExpression>();

        var innerCall = (MethodCallExpression)outerCall.Object!;
        innerCall.Method.Name.Should().Be("SetProperty");

        // Inspect valid argument for first call
        var innerValueArg = innerCall.Arguments[1];
        innerValueArg.Should().BeAssignableTo<ConstantExpression>();
        ((ConstantExpression)innerValueArg).Value.Should().Be("New Name");
    }

    /// <summary>
    /// Verifies that <see cref="EfCorePropertySetter{T}.BuildSetPropertyExpression"/> returns the parameter itself when no setters are added.
    /// </summary>
    [Fact]
    public void BuildSetPropertyExpression_WithEmptySetters_ShouldReturnParameter()
    {
        // Arrange
        var setter = new EfCorePropertySetter<TestEntity>();

        // Act
        var expression = setter.BuildSetPropertyExpression();

        // Assert
        // Rationale: If no properties are set, the identity expression (x => x) should be returned as the body.
        expression.Body.Should().BeAssignableTo<ParameterExpression>();
    }

    /// <summary>
    /// Verifies that <see cref="EfCorePropertySetter{T}.BuildSetPropertyExpression"/> correctly chains calls using lambda expressions for values.
    /// </summary>
    [Fact]
    public void BuildSetPropertyExpression_WithExpressionValues_ShouldChainSetPropertyCalls()
    {
        // Arrange
        var setter = new EfCorePropertySetter<TestEntity>();

        // Act
        // Rationale: Set Value = Value + 1 (using an expression instead of a constant).
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
        valueArg.Should().BeAssignableTo<LambdaExpression>(); // Rationale: It should be a LambdaExpression<Func<T, TProperty>>.
    }
}
#endif
