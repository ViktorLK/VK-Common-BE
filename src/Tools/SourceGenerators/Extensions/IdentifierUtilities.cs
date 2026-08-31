using System.Text;

namespace VK.Tools.SourceGenerators.Extensions;

/// <summary>
/// Provides utility methods for transforming strings into valid C# identifiers.
/// </summary>
internal static class IdentifierUtilities
{
    /// <summary>
    /// Converts a string into a safe C# identifier by removing invalid characters 
    /// and ensuring it doesn't start with a digit.
    /// </summary>
    /// <param name="value">The source string to convert.</param>
    /// <returns>A safe C# identifier string in PascalCase.</returns>
    public static string ToSafeIdentifier(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return "_";
        }

        var sb = new StringBuilder();
        foreach (var c in value)
        {
            if (char.IsLetterOrDigit(c))
            {
                sb.Append(c);
            }
            else
            {
                sb.Append('_');
            }
        }

        var safeName = sb.ToString();

        // Ensure it doesn't start with a digit
        if (safeName.Length > 0 && char.IsDigit(safeName[0]))
        {
            safeName = "_" + safeName;
        }

        return ToPascalCase(safeName);
    }

    /// <summary>
    /// Converts a string to PascalCase.
    /// </summary>
    /// <param name="value">The source string to convert.</param>
    /// <returns>The string in PascalCase.</returns>
    public static string ToPascalCase(string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return value;
        }

        if (char.IsUpper(value[0]))
        {
            return value;
        }

        return char.ToUpperInvariant(value[0]) + value.Substring(1);
    }

    /// <summary>
    /// Converts a PascalCase or camelCase string to snake_case.
    /// </summary>
    public static string ToSnakeCase(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        var sb = new StringBuilder();
        for (int i = 0; i < value.Length; i++)
        {
            char c = value[i];
            if (char.IsUpper(c))
            {
                if (i > 0 && (char.IsLower(value[i - 1]) || (i + 1 < value.Length && char.IsLower(value[i + 1]))))
                {
                    sb.Append('_');
                }
                sb.Append(char.ToLowerInvariant(c));
            }
            else
            {
                sb.Append(c);
            }
        }

        return sb.ToString();
    }

    /// <summary>
    /// Extracts the lowercase logical module name from a namespace (e.g. VK.Blocks.Identity.EFCore -> identity, VK.Blocks.AI.Psyche.EFCore -> psyche).
    /// </summary>
    public static string ExtractModuleName(string ns)
    {
        if (string.IsNullOrWhiteSpace(ns))
        {
            return "persist";
        }

        var parts = ns.Split('.');
        for (int i = 0; i < parts.Length; i++)
        {
            var part = parts[i];
            if (part.Equals("Blocks", System.StringComparison.OrdinalIgnoreCase) ||
                part.Equals("Labs", System.StringComparison.OrdinalIgnoreCase))
            {
                if (i + 1 < parts.Length)
                {
                    var next = parts[i + 1];
                    if (next.Equals("AI", System.StringComparison.OrdinalIgnoreCase) && i + 2 < parts.Length)
                    {
                        return parts[i + 2].ToLowerInvariant();
                    }
                    return next.ToLowerInvariant();
                }
            }
        }

        return parts[0].ToLowerInvariant();
    }
}
