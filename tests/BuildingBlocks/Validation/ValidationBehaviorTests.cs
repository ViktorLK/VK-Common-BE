using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Moq;
using VK.Blocks.Validation.Behaviors;
using VK.Blocks.Validation.Abstractions;
using VKValidationException = VK.Blocks.Validation.Exceptions.ValidationException;

namespace VK.Blocks.Validation.UnitTests;

public class ValidationBehaviorTests
{
    public class TestRequest : IRequest<string> { }

    [Fact]
    public async Task Handle_ShouldCallNext_WhenNoValidatorsExist()
    {
        // Arrange
        var validators = Enumerable.Empty<IValidator<TestRequest>>();
        var behavior = new ValidationBehavior<TestRequest, string>(validators);
        var nextCalled = false;
        RequestHandlerDelegate<string> next = () => { nextCalled = true; return Task.FromResult("Success"); };

        // Act
        var result = await behavior.Handle(new TestRequest(), next, CancellationToken.None);

        // Assert
        result.Should().Be("Success");
        nextCalled.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_ShouldCallNext_WhenValidationSucceeds()
    {
        // Arrange
        var mockValidator = new Mock<IValidator<TestRequest>>();
        mockValidator.Setup(v => v.ValidateAsync(It.IsAny<ValidationContext<TestRequest>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        var behavior = new ValidationBehavior<TestRequest, string>([mockValidator.Object]);
        var nextCalled = false;
        RequestHandlerDelegate<string> next = () => { nextCalled = true; return Task.FromResult("Success"); };

        // Act
        var result = await behavior.Handle(new TestRequest(), next, CancellationToken.None);

        // Assert
        result.Should().Be("Success");
        nextCalled.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_ShouldThrowValidationException_WhenValidationFails()
    {
        // Arrange
        var mockValidator = new Mock<IValidator<TestRequest>>();
        var failures = new List<ValidationFailure> { new("Property", "Error Message") { ErrorCode = "ErrorCode" } };
        mockValidator.Setup(v => v.ValidateAsync(It.IsAny<ValidationContext<TestRequest>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult(failures));

        var behavior = new ValidationBehavior<TestRequest, string>([mockValidator.Object]);
        RequestHandlerDelegate<string> next = () => Task.FromResult("Success");

        // Act
        Func<Task> act = async () => await behavior.Handle(new TestRequest(), next, CancellationToken.None);

        // Assert
        var exception = await act.Should().ThrowAsync<VKValidationException>();
        exception.Which.Errors.Should().HaveCount(1);
        exception.Which.Errors[0].PropertyName.Should().Be("Property");
        exception.Which.Errors[0].ErrorMessage.Should().Be("Error Message");
        exception.Which.Errors[0].ErrorCode.Should().Be("ErrorCode");
    }

    [Fact]
    public async Task Handle_ShouldCollectFailuresFromMultipleValidators()
    {
        // Arrange
        var mockValidator1 = new Mock<IValidator<TestRequest>>();
        mockValidator1.Setup(v => v.ValidateAsync(It.IsAny<ValidationContext<TestRequest>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult([new ValidationFailure("P1", "E1")]));

        var mockValidator2 = new Mock<IValidator<TestRequest>>();
        mockValidator2.Setup(v => v.ValidateAsync(It.IsAny<ValidationContext<TestRequest>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult([new ValidationFailure("P2", "E2")]));

        var behavior = new ValidationBehavior<TestRequest, string>([mockValidator1.Object, mockValidator2.Object]);
        RequestHandlerDelegate<string> next = () => Task.FromResult("Success");

        // Act
        Func<Task> act = async () => await behavior.Handle(new TestRequest(), next, CancellationToken.None);

        // Assert
        var exception = await act.Should().ThrowAsync<VKValidationException>();
        exception.Which.Errors.Should().HaveCount(2);
        exception.Which.Errors.Should().Contain(e => e.PropertyName == "P1");
        exception.Which.Errors.Should().Contain(e => e.PropertyName == "P2");
    }
}
