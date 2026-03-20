using System.ComponentModel.DataAnnotations;
using VK.Blocks.Validation.Abstractions.Contracts;
using VK.Blocks.Validation.Validators;

namespace VK.Blocks.Validation.UnitTests.Validators;

public sealed class DataAnnotationsValidatorTests
{
    public sealed class TestModel
    {
        [Required(ErrorMessage = "Name is required")]
        public string? Name { get; set; }

        [Range(1, 100, ErrorMessage = "Age must be between 1 and 100")]
        public int Age { get; set; }
    }

    private readonly DataAnnotationsValidator _validator;

    public DataAnnotationsValidatorTests()
    {
        _validator = new DataAnnotationsValidator();
    }

    [Fact]
    public void CanValidate_ShouldReturnTrue_WhenModelIsNotNull()
    {
        // Act
        var result = _validator.CanValidate(new object());

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void CanValidate_ShouldReturnFalse_WhenModelIsNull()
    {
        // Act
        var result = _validator.CanValidate(null!);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task ValidateAsync_ShouldReturnSuccess_WhenModelIsValid()
    {
        // Arrange
        var model = new TestModel { Name = "John", Age = 30 };

        // Act
        var result = await _validator.ValidateAsync(model);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task ValidateAsync_ShouldReturnFailure_WhenModelIsInvalid()
    {
        // Arrange
        var model = new TestModel { Name = null, Age = 150 };

        // Act
        var result = await _validator.ValidateAsync(model);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().HaveCount(2);
        result.Errors.Should().Contain(e => e.PropertyName == "Name" && e.ErrorMessage == "Name is required");
        result.Errors.Should().Contain(e => e.PropertyName == "Age" && e.ErrorMessage == "Age must be between 1 and 100");
    }
}
