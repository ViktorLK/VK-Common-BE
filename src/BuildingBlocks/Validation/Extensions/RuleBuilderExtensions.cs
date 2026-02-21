using System.Text.RegularExpressions;
using FluentValidation;

namespace VK.Blocks.Validation.Extensions;

/// <summary>
/// Extension methods for <see cref="IRuleBuilder{T, TProperty}"/> to provide common validation rules.
/// </summary>
public static class RuleBuilderExtensions
{
    private static readonly Regex PasswordRegex = new(
        @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{8,}$",
        RegexOptions.Compiled,
        TimeSpan.FromMilliseconds(100));

    // Simple regex for international phone numbers (e.g. +1234567890) or local (09012345678)
    // Allows optional +, digits, spaces, hyphens. Min 7, Max 15 digits roughly.
    private static readonly Regex PhoneRegex = new(
        @"^\+?[\d\s-]{7,20}$",
        RegexOptions.Compiled,
        TimeSpan.FromMilliseconds(100));

    /// <summary>
    /// Validates that the property is a valid password.
    /// Requirements:
    /// - At least 8 characters
    /// - At least one uppercase letter
    /// - At least one lowercase letter
    /// - At least one number
    /// - At least one special character (@$!%*?&)
    /// </summary>
    /// <typeparam name="T">Type of object being validated</typeparam>
    /// <param name="ruleBuilder">The rule builder</param>
    /// <returns>The rule builder</returns>
    public static IRuleBuilderOptions<T, string> MustBeValidPassword<T>(this IRuleBuilder<T, string> ruleBuilder)
    {
        return ruleBuilder
            .Must(password => !string.IsNullOrEmpty(password) && PasswordRegex.IsMatch(password))
            .WithMessage("Password must be at least 8 characters long and contain at least one uppercase letter, one lowercase letter, one number, and one special character.");
    }

    /// <summary>
    /// Validates that the property is a valid phone number format.
    /// </summary>
    /// <typeparam name="T">Type of object being validated</typeparam>
    /// <param name="ruleBuilder">The rule builder</param>
    /// <returns>The rule builder</returns>
    public static IRuleBuilderOptions<T, string> MustBeValidPhone<T>(this IRuleBuilder<T, string> ruleBuilder)
    {
        return ruleBuilder
            .Must(phone => !string.IsNullOrEmpty(phone) && PhoneRegex.IsMatch(phone))
            .WithMessage("Phone number format is invalid.");
    }
}
