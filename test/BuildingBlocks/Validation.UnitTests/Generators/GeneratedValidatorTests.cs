using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using VK.Blocks.Testing.Core.Assertions;
using VK.Blocks.Validation;
using Xunit;

namespace VK.Blocks.Validation.UnitTests.Generators;

[VKValidate]
public sealed class TestUserCommand
{
    [Required]
    [StringLength(20, MinimumLength = 3)]
    public string Username { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Range(18, 100)]
    public int Age { get; set; }

    [RegularExpression(@"^[0-9]+$")]
    public string? Code { get; set; }
}

[VKValidate(CascadeMode = VKCascadeMode.Stop)]
public sealed class TestLoginCommand
{
    [Required]
    public string Username { get; set; } = string.Empty;

    [Required]
    [StringLength(50, MinimumLength = 8)]
    [VKSensitiveData]
    public string Password { get; set; } = string.Empty;
}

[VKValidate]
public sealed class TestAddress
{
    [Required]
    public string City { get; set; } = string.Empty;
}

[VKValidate]
public sealed class TestCustomerCommand
{
    [Required]
    public string Name { get; set; } = string.Empty;

    [VKValidate]
    public TestAddress? Address { get; set; }
}

public sealed class GeneratedValidatorTests
{
    [Fact]
    public async Task ValidateAsync_WhenModelIsValid_ReturnsSuccess()
    {
        // Arrange
        var validator = new TestUserCommandValidator();
        var model = new TestUserCommand
        {
            Username = "johndoe",
            Email = "john@example.com",
            Age = 25,
            Code = "12345"
        };

        // Act
        var result = await validator.ValidateAsync(model);

        // Assert
        result.ShouldBeValid();
    }

    [Fact]
    public async Task ValidateAsync_WhenRequiredAndRangeViolated_CollectsAllErrors()
    {
        // Arrange
        var validator = new TestUserCommandValidator();
        var model = new TestUserCommand
        {
            Username = "", // Required + Length
            Email = "invalid-email", // Email
            Age = 15, // Range
            Code = "abc" // Pattern
        };

        // Act
        var result = await validator.ValidateAsync(model);

        // Assert
        result.ShouldBeInvalid();
        result.ShouldHaveErrorFor(nameof(TestUserCommand.Username));
        result.ShouldHaveErrorFor(nameof(TestUserCommand.Email));
        result.ShouldHaveErrorFor(nameof(TestUserCommand.Age));
        result.ShouldHaveErrorFor(nameof(TestUserCommand.Code));
        result.ShouldHaveErrorCode(VKValidationCodes.Required);
        result.ShouldHaveErrorCode(VKValidationCodes.Email);
        result.ShouldHaveErrorCode(VKValidationCodes.Range);
        result.ShouldHaveErrorCode(VKValidationCodes.Pattern);
    }

    [Fact]
    public async Task ValidateAsync_WhenSensitiveData_MasksAttemptedValue()
    {
        // Arrange
        var validator = new TestLoginCommandValidator();
        var model = new TestLoginCommand
        {
            Username = "admin",
            Password = "" // Required
        };

        // Act
        var result = await validator.ValidateAsync(model);

        // Assert
        result.ShouldBeInvalid();
        var pwdError = result.Errors.First(e => e.PropertyName == nameof(TestLoginCommand.Password));
        pwdError.AttemptedValue.Should().Be("******");
    }

    [Fact]
    public async Task ValidateAsync_WhenCascadeModeStop_StopsAtFirstError()
    {
        // Arrange
        var validator = new TestLoginCommandValidator();
        var model = new TestLoginCommand
        {
            Username = "", // First error
            Password = ""  // Should not be evaluated due to cascade stop
        };

        // Act
        var result = await validator.ValidateAsync(model);

        // Assert
        result.ShouldBeInvalid();
        result.Errors.Should().HaveCount(1);
        result.Errors[0].PropertyName.Should().Be(nameof(TestLoginCommand.Username));
    }

    [Fact]
    public async Task ValidateAsync_WhenNestedModelInvalid_ReportsNestedPath()
    {
        // Arrange
        var validator = new TestCustomerCommandValidator();
        var model = new TestCustomerCommand
        {
            Name = "Alice",
            Address = new TestAddress { City = "" } // Nested city is empty
        };

        // Act
        var result = await validator.ValidateAsync(model);

        // Assert
        result.ShouldBeInvalid();
        result.ShouldHaveErrorFor("Address.City");
    }

    [Fact]
    public void AddVKGeneratedValidators_RegistersAllGeneratedValidatorsInDI()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddVKGeneratedValidators();
        var provider = services.BuildServiceProvider();

        // Assert
        var userValidator = provider.GetService<IVKValidator<TestUserCommand>>();
        userValidator.Should().NotBeNull();
        userValidator.Should().BeOfType<TestUserCommandValidator>();

        var allValidators = provider.GetServices<IVKValidator>();
        allValidators.Should().Contain(v => v is TestUserCommandValidator);
        allValidators.Should().Contain(v => v is TestLoginCommandValidator);
        allValidators.Should().Contain(v => v is TestCustomerCommandValidator);
    }
}
