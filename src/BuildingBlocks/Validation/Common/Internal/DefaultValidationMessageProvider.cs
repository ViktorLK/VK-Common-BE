using System;
using System.Collections.Concurrent;
using System.Globalization;
using VK.Blocks.Core;

namespace VK.Blocks.Validation.Common.Internal;

/// <summary>
/// Default in-memory implementation of <see cref="IVKValidationMessageProvider"/> supporting standard validation error keys.
/// </summary>
// [AP.01] Sealed by default
// [AP.03] Internal scoping without VK prefix
internal sealed class DefaultValidationMessageProvider : IVKValidationMessageProvider
{
    private static readonly ConcurrentDictionary<string, string> Templates = new(StringComparer.OrdinalIgnoreCase)
    {
        [VKValidationCodes.Required] = "{0} is required.",
        [VKValidationCodes.Length] = "{0} must be between {1} and {2} characters.",
        [VKValidationCodes.Range] = "{0} must be between {1} and {2}.",
        [VKValidationCodes.Pattern] = "{0} format is invalid.",
        [VKValidationCodes.Email] = "{0} must be a valid email address.",
        [VKValidationCodes.Enum] = "{0} is not a valid enum value.",
        [VKValidationCodes.Unique] = "{0} must be unique.",
        [VKValidationCodes.Exists] = "Referenced {0} does not exist.",
        [VKValidationCodes.State] = "Invalid state transition for {0}.",
        [VKValidationCodes.Unauthorized] = "Authorization validation failed for {0}.",
        [VKValidationCodes.Custom] = "{0} is invalid."
    };

    public string? GetMessage(string key, CultureInfo? culture = null, params object?[] arguments)
    {
        // [AP.01] Boundary check with VKGuard
        if (string.IsNullOrWhiteSpace(key))
        {
            return null;
        }

        if (!Templates.TryGetValue(key, out var template))
        {
            return null;
        }

        if (arguments.Length == 0)
        {
            return template;
        }

        try
        {
            var targetCulture = culture ?? CultureInfo.CurrentUICulture;
            return string.Format(targetCulture, template, arguments);
        }
        catch (FormatException)
        {
            return template;
        }
    }

    /// <summary>
    /// Registers or overrides a message template for a specific key.
    /// </summary>
    public static void SetTemplate(string key, string template)
    {
        VKGuard.NotNullOrWhiteSpace(key);
        VKGuard.NotNullOrWhiteSpace(template);
        Templates[key] = template;
    }
}
