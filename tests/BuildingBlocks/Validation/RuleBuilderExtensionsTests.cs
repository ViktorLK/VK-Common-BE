using FluentValidation;
using VK.Blocks.Validation.Extensions;

namespace VK.Blocks.Validation.UnitTests;

public class RuleBuilderExtensionsTests
{
    private class TestModel
    {
        public string? Value { get; set; }
    }

    private class PasswordValidator : AbstractValidator<TestModel>
    {
        public PasswordValidator()
        {
            RuleFor(x => x.Value).MustBeValidPassword();
        }
    }

    private class PhoneValidator : AbstractValidator<TestModel>
    {
        public PhoneValidator()
        {
            RuleFor(x => x.Value).MustBeValidPhone();
        }
    }

    [Theory]
    [InlineData("P@ssw0rd123")]
    [InlineData("Strong!123")]
    public void MustBeValidPassword_ShouldPass_ForValidPasswords(string password)
    {
        var validator = new PasswordValidator();
        var result = validator.Validate(new TestModel { Value = password });
        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData("short")]
    [InlineData("nouppercase1!")]
    [InlineData("NOLOWERCASE1!")]
    [InlineData("NoDigit!")]
    [InlineData("NoSpecialChar1")]
    public void MustBeValidPassword_ShouldFail_ForInvalidPasswords(string password)
    {
        var validator = new PasswordValidator();
        var result = validator.Validate(new TestModel { Value = password });
        result.IsValid.Should().BeFalse();
    }

    [Theory]
    [InlineData("+819012345678")]
    [InlineData("09012345678")]
    [InlineData("+1 234 567 8900")]
    public void MustBeValidPhone_ShouldPass_ForValidPhones(string phone)
    {
        var validator = new PhoneValidator();
        var result = validator.Validate(new TestModel { Value = phone });
        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData("123")] // Too short
    [InlineData("abcde-fghi-jkl")] // Not numeric
    public void MustBeValidPhone_ShouldFail_ForInvalidPhones(string phone)
    {
        var validator = new PhoneValidator();
        var result = validator.Validate(new TestModel { Value = phone });
        result.IsValid.Should().BeFalse();
    }
}
