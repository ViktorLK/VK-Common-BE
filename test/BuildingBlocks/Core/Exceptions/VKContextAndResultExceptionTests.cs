using System;
using VK.Blocks.Core;

namespace VK.Blocks.Core.UnitTests.Exceptions;

public sealed class VKContextAndResultExceptionTests
{
    [Fact]
    public void VKContextException_MissingTenantCoordinate_ShouldSetCorrectProperties()
    {
        // Act
        var ex = VKContextException.MissingTenantCoordinate();

        // Assert
        ex.Code.Should().Be("Core.ContextError");
        ex.StatusCode.Should().Be(500);
        ex.IsPublic.Should().BeFalse();
        ex.Message.Should().Contain("active ambient tenant coordinate");
    }

    [Fact]
    public void VKContextException_MissingUserCoordinate_ShouldSetCorrectProperties()
    {
        // Act
        var ex = VKContextException.MissingUserCoordinate();

        // Assert
        ex.Code.Should().Be("Core.ContextError");
        ex.StatusCode.Should().Be(500);
        ex.IsPublic.Should().BeFalse();
        ex.Message.Should().Contain("active ambient user coordinate");
    }

    [Fact]
    public void VKResultException_FactoryMethods_ShouldSetCorrectProperties()
    {
        // Act & Assert
        var failAccess = VKResultException.FailureValueAccess();
        failAccess.Code.Should().Be("Core.ResultError");
        failAccess.StatusCode.Should().Be(500);
        failAccess.IsPublic.Should().BeFalse();

        var nullSuccess = VKResultException.NullSuccessValue();
        nullSuccess.Code.Should().Be("Core.ResultError");
        nullSuccess.Message.Should().Contain("null value");

        var invalidSuccess = VKResultException.InvalidSuccessState("ERR01");
        invalidSuccess.Message.Should().Contain("ERR01");

        var invalidFailure = VKResultException.InvalidFailureState();
        invalidFailure.Message.Should().Contain("at least one error");
    }
}
