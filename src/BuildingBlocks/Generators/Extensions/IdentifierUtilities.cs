using System.Text;

namespace VK.Blocks.Generators.Extensions;

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
}
