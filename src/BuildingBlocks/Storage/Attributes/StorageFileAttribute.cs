using System;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;

namespace VK.Blocks.Storage;

/// <summary>
/// Validates that a string is a valid Storage file name.
/// Includes SafePath checks, reserved name checks, and optional extension checks.
/// </summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter)]
public sealed class StorageFileAttribute : ValidationAttribute
{
    private readonly string[]? _allowedExtensions;

    public StorageFileAttribute(params string[] allowedExtensions)
    {
        _allowedExtensions = allowedExtensions.Length > 0 ? allowedExtensions : null;
    }

    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value is null)
            return ValidationResult.Success;

        if (value is not string fileName)
        {
            return new ValidationResult("StorageFileAttribute can only be applied to string properties.");
        }

        // 1. Common Safe Path Rules
        if (!VKStorageGuard.IsValidSafePath(fileName, out var error))
        {
            return new ValidationResult(error);
        }

        // 2. Reserved Names
        if (fileName.EndsWith(VKStorageConstants.DirectoryMarker, StringComparison.OrdinalIgnoreCase))
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
