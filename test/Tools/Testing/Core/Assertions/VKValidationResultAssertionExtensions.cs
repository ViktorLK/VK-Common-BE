using System;
using System.Linq;
using FluentAssertions;
using VK.Blocks.Validation;

namespace VK.Blocks.Testing.Core.Assertions;

/// <summary>
/// Fluent assertion extensions for <see cref="VKValidationResult"/>.
/// </summary>
public static class VKValidationResultAssertionExtensions
{
    /// <summary>
    /// Asserts that the validation result is valid (no errors).
    /// </summary>
    public static void ShouldBeValid(this VKValidationResult result, string because = "", params object[] becauseArgs)
    {
        result.Should().NotBeNull();
        result.IsValid.Should().BeTrue(because, becauseArgs);
        result.Errors.Should().BeEmpty(because, becauseArgs);
    }

    /// <summary>
    /// Asserts that the validation result is invalid (has errors).
    /// </summary>
    public static void ShouldBeInvalid(this VKValidationResult result, string because = "", params object[] becauseArgs)
    {
        result.Should().NotBeNull();
        result.IsValid.Should().BeFalse(because, becauseArgs);
        result.Errors.Should().NotBeEmpty(because, becauseArgs);
    }

    /// <summary>
    /// Asserts that the validation result contains an error for the specified property name.
    /// </summary>
    public static void ShouldHaveErrorFor(this VKValidationResult result, string propertyName, string because = "", params object[] becauseArgs)
    {
        result.Should().NotBeNull();
        result.Errors.Should().Contain(e => string.Equals(e.PropertyName, propertyName, StringComparison.OrdinalIgnoreCase), because, becauseArgs);
    }

    /// <summary>
    /// Asserts that the validation result contains a specific error code.
    /// </summary>
    public static void ShouldHaveErrorCode(this VKValidationResult result, string errorCode, string because = "", params object[] becauseArgs)
    {
        result.Should().NotBeNull();
        result.Errors.Should().Contain(e => string.Equals(e.ErrorCode, errorCode, StringComparison.OrdinalIgnoreCase), because, becauseArgs);
    }

    /// <summary>
    /// Asserts that the validation result contains an error with the specified severity.
    /// </summary>
    public static void ShouldHaveSeverity(this VKValidationResult result, VKValidationSeverity severity, string because = "", params object[] becauseArgs)
    {
        result.Should().NotBeNull();
        result.Errors.Should().Contain(e => e.Severity == severity, because, becauseArgs);
    }
}
