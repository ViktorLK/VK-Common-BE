using System.Globalization;

namespace VK.Blocks.Validation;

/// <summary>
/// Defines a provider for resolving localized validation error messages.
/// </summary>
public interface IVKValidationMessageProvider
{
    /// <summary>
    /// Resolves a localized message for the specified message key or template.
    /// </summary>
    /// <param name="key">The message key (e.g. "Validation.Required", "Validation.Length").</param>
    /// <param name="culture">The target culture, or null for default/ambient culture.</param>
    /// <param name="arguments">Optional format arguments for placeholder replacement.</param>
    /// <returns>The resolved localized message, or null if the key was not found.</returns>
    string? GetMessage(string key, CultureInfo? culture = null, params object?[] arguments);
}
