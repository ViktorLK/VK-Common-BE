using System.Text.RegularExpressions;
using FluentValidation;

namespace VK.Blocks.Validation.Extensions;

/// <summary>
/// Extension methods for <see cref="IRuleBuilder{T, TProperty}"/> to provide common validation rules.
/// </summary>
public static partial class RuleBuilderExtensions
{
    private const string PasswordPattern = @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{8,}$";
    private const string PhonePattern = @"^\+?[\d\s-]{7,20}$";

    [GeneratedRegex(PasswordPattern, RegexOptions.Compiled, 100)]
    private static partial Regex PasswordRegex();

    [GeneratedRegex(PhonePattern, RegexOptions.Compiled, 100)]
    private static partial Regex PhoneRegex();

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
            .Must(password => !string.IsNullOrEmpty(password) && PasswordRegex().IsMatch(password))
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
            .Must(phone => !string.IsNullOrEmpty(phone) && PhoneRegex().IsMatch(phone))
            .WithMessage("Phone number format is invalid.");
    }
}
