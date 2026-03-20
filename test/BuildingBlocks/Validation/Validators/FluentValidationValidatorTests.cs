using FluentValidation;
using FluentValidation.Results;
using Moq;
using VK.Blocks.Validation.Abstractions.Contracts;
using VK.Blocks.Validation.Validators;

namespace VK.Blocks.Validation.UnitTests.Validators;

public sealed class FluentValidationValidatorTests
{
    public sealed class TestModel
    {
        public string? Name { get; set; }
    }

    private readonly Mock<IServiceProvider> _serviceProviderMock;
    private readonly FluentValidationValidator _validator;

    public FluentValidationValidatorTests()
    {
        _serviceProviderMock = new Mock<IServiceProvider>();
        _validator = new FluentValidationValidator(_serviceProviderMock.Object);
    }

    [Fact]
    public void CanValidate_ShouldReturnTrue_WhenValidatorIsRegistered()
    {
        // Arrange
        var model = new TestModel();
        var validatorType = typeof(IValidator<TestModel>);
        var mockValidator = new Mock<IValidator<TestModel>>();
        _serviceProviderMock.Setup(s => s.GetService(validatorType)).Returns(mockValidator.Object);

        // Act
        var result = _validator.CanValidate(model);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void CanValidate_ShouldReturnFalse_WhenValidatorIsNotRegistered()
    {
        // Arrange
        var model = new TestModel();
        var validatorType = typeof(IValidator<TestModel>);
        _serviceProviderMock.Setup(s => s.GetService(validatorType)).Returns((object?)null);

        // Act
        var result = _validator.CanValidate(model);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task ValidateAsync_ShouldReturnSuccess_WhenValidatorPasses()
    {
        // Arrange
        var model = new TestModel();
        var validatorType = typeof(IValidator<TestModel>);
        var mockValidator = new Mock<IValidator<TestModel>>();
        mockValidator.Setup(v => v.ValidateAsync(It.IsAny<ValidationContext<object>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new FluentValidation.Results.ValidationResult());
        _serviceProviderMock.Setup(s => s.GetService(validatorType)).Returns(mockValidator.Object);

        // Act
        var result = await _validator.ValidateAsync(model);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task ValidateAsync_ShouldReturnFailure_WhenValidatorFails()
    {
        // Arrange
        var model = new TestModel();
        var validatorType = typeof(IValidator<TestModel>);
        var mockValidator = new Mock<IValidator<TestModel>>();
        var failures = new List<ValidationFailure> { new ValidationFailure("Name", "Name is required") };
        mockValidator.Setup(v => v.ValidateAsync(It.IsAny<ValidationContext<object>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new FluentValidation.Results.ValidationResult(failures));
        _serviceProviderMock.Setup(s => s.GetService(validatorType)).Returns(mockValidator.Object);

        // Act
        var result = await _validator.ValidateAsync(model);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle().Which.PropertyName.Should().Be("Name");
    }

    [Fact]
    public async Task ValidateAsync_ShouldReturnSuccess_WhenNoValidatorIsFound()
    {
        // Arrange
        var model = new TestModel();
        var validatorType = typeof(IValidator<TestModel>);
        _serviceProviderMock.Setup(s => s.GetService(validatorType)).Returns((object?)null);

        // Act
        var result = await _validator.ValidateAsync(model);

        // Assert
        result.IsValid.Should().BeTrue();
    }
}
