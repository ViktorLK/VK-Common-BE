using System.ComponentModel.DataAnnotations;
using VK.Blocks.Blob.Constants;
using VK.Blocks.Blob.Guards;

namespace VK.Blocks.Blob.Attributes;

/// <summary>
/// Validates that a string is a valid blob file name.
/// Includes SafePath checks, reserved name checks, and optional extension checks.
/// </summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter)]
public sealed class BlobFileAttribute : ValidationAttribute
{
    private readonly string[]? _allowedExtensions;

    public BlobFileAttribute(params string[] allowedExtensions)
    {
        _allowedExtensions = allowedExtensions.Length > 0 ? allowedExtensions : null;
    }

    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value is null) return ValidationResult.Success;

        if (value is not string fileName)
        {
            return new ValidationResult("BlobFileAttribute can only be applied to string properties.");
        }

        // 1. Common Safe Path Rules
        if (!BlobGuard.IsValidSafePath(fileName, out var error))
        {
            return new ValidationResult(error);
        }

        // 2. Reserved Names
        if (fileName.EndsWith(BlobConstants.DirectoryMarker, StringComparison.OrdinalIgnoreCase))
        {
            return new ValidationResult("The file name is reserved for directory markers.");
        }

        // 3. Extension Check (Optional at Attribute level)
        if (_allowedExtensions != null)
        {
            var extension = Path.GetExtension(fileName).ToLowerInvariant();
            if (!_allowedExtensions.Contains(extension, StringComparer.OrdinalIgnoreCase))
            {
                return new ValidationResult($"File extension {extension} is not allowed. Allowed: {string.Join(", ", _allowedExtensions)}");
            }
        }

        return ValidationResult.Success;
    }
}
