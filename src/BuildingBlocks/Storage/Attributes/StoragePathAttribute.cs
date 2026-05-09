using System;
using System.ComponentModel.DataAnnotations;

namespace VK.Blocks.Storage;

/// <summary>
/// Validates that a string is a safe path for Azure Storage Storage (no traversal, no illegal chars, max 1024).
/// Suitable for both directory paths and file paths.
/// </summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter)]
public sealed class StoragePathAttribute : ValidationAttribute
{
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value is null)
            return ValidationResult.Success;

        if (value is not string path)
        {
            return new ValidationResult("StoragePathAttribute can only be applied to string properties.");
        }

        if (!VKStorageGuard.IsValidSafePath(path, out var error))
        {
            return new ValidationResult(error);
        }

        return ValidationResult.Success;
    }
}
