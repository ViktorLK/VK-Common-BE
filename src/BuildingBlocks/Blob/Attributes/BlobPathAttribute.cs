using System.ComponentModel.DataAnnotations;
using VK.Blocks.Blob.Guards;

namespace VK.Blocks.Blob.Attributes;

/// <summary>
/// Validates that a string is a safe path for Azure Blob Storage (no traversal, no illegal chars, max 1024).
/// Suitable for both directory paths and file paths.
/// </summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter)]
public sealed class BlobPathAttribute : ValidationAttribute
{
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value is null) return ValidationResult.Success;

        if (value is not string path)
        {
            return new ValidationResult("BlobPathAttribute can only be applied to string properties.");
        }

        if (!BlobGuard.IsValidSafePath(path, out var error))
        {
            return new ValidationResult(error);
        }

        return ValidationResult.Success;
    }
}
