using System.Collections.Generic;
using VK.Blocks.Testing;
using VK.Blocks.Testing.Core.Assertions;
using VK.Blocks.Validation;
using Xunit;

namespace VK.Blocks.Validation.UnitTests.Common;

public sealed class VKValidationResultTests : VKUnitTestBase<VKValidationResult>
{
    [Fact]
    public void Success_ShouldCreateValidResult()
    {
        var result = VKValidationResult.Success();

        result.ShouldBeValid();
        Assert.Empty(result.Errors);
    }

    [Fact]
    public void Failure_WithSingleError_ShouldContainError()
    {
        var result = VKValidationResult.Failure("Email", "Email is required.", VKValidationCodes.Required);

        result.ShouldBeInvalid();
        result.ShouldHaveErrorFor("Email");
        result.ShouldHaveErrorCode(VKValidationCodes.Required);
        result.ShouldHaveSeverity(VKValidationSeverity.Error);
    }

    [Fact]
    public void Failure_WithMultipleErrors_ShouldContainAllErrors()
    {
        var errors = new List<VKValidationError>
        {
            new("Username", "Username is required.", VKValidationCodes.Required),
            new("Age", "Age must be positive.", VKValidationCodes.Range, VKValidationSeverity.Warning)
        };

        var result = VKValidationResult.Failure(errors);

        result.ShouldBeInvalid();
        Assert.Equal(2, result.Errors.Count);
        result.ShouldHaveErrorFor("Username");
        result.ShouldHaveErrorFor("Age");
        result.ShouldHaveSeverity(VKValidationSeverity.Warning);
    }
}
