using VK.Blocks.Testing;
using VK.Blocks.Validation;
using Xunit;

namespace VK.Blocks.Validation.UnitTests.Common;

public sealed class VKValidationResultExtensionsTests : VKUnitTestBase<VKValidationResult>
{
    [Fact]
    public void ThrowIfInvalid_WhenInvalid_ShouldThrowVKValidationException()
    {
        var result = VKValidationResult.Failure("Field", "Error message", VKValidationCodes.Required);

        var ex = Assert.Throws<VKValidationException>(() => result.ThrowIfInvalid());
        Assert.Single(ex.Errors);
    }

    [Fact]
    public void ThrowIfInvalid_WhenValid_ShouldNotThrow()
    {
        var result = VKValidationResult.Success();
        result.ThrowIfInvalid(); // should not throw
    }

    [Fact]
    public void ToResult_WhenValid_ShouldReturnSuccessVKResult()
    {
        var result = VKValidationResult.Success();
        var vkResult = result.ToResult();

        Assert.True(vkResult.IsSuccess);
    }

    [Fact]
    public void ToResult_WhenInvalid_ShouldReturnFailureVKResult()
    {
        var result = VKValidationResult.Failure("Name", "Name is required", VKValidationCodes.Required);
        var vkResult = result.ToResult();

        Assert.True(vkResult.IsFailure);
        Assert.Equal(VKValidationCodes.Required, vkResult.FirstError.Code);
    }
}
