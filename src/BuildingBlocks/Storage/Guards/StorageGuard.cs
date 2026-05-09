using System;
using System.IO;
using System.Linq;

namespace VK.Blocks.Storage;

public static class VKStorageGuard
{
    private static readonly char[] InvalidPathChars = ['\\', ':', '*', '?', '"', '<', '>', '|'];

    public static void EnsureValidUpload(VKStorageUploadRequest request, VKStorageOptions options)
    {
        if (!IsValidSafePath(request.FileName, out var error))
        {
            throw new VKStorageValidationException(error, VKStorageErrors.InvalidPath.Code);
        }

        if (request.FileName.EndsWith(VKStorageConstants.DirectoryMarker, StringComparison.OrdinalIgnoreCase))
        {
            throw new VKStorageValidationException("The file name is reserved for directory markers.", VKStorageErrors.InvalidFileName.Code);
        }

        if (request.Content == null || request.Content.Length == 0)
        {
            throw new VKStorageValidationException(
                VKStorageErrors.EmptyContent.Description,
                VKStorageErrors.EmptyContent.Code);
        }

        if (request.Content.Length > options.MaxFileSizeBytes)
        {
            throw new VKStorageValidationException(
                $"File size exceeds the limit of {options.MaxFileSizeBytes} bytes.",
                VKStorageErrors.FileSizeExceeded.Code);
        }

        var extension = Path.GetExtension(request.FileName).ToLowerInvariant();
        if (!options.AllowedExtensions.Contains(extension, StringComparer.OrdinalIgnoreCase))
        {
            throw new VKStorageValidationException(
                $"File extension {extension} is not allowed.",
                VKStorageErrors.InvalidFileType.Code);
        }
    }

    public static void EnsureValidDirectory(string directoryPath)
    {
        if (!IsValidSafePath(directoryPath, out var error))
        {
            throw new VKStorageValidationException(error, VKStorageErrors.InvalidPath.Code);
        }
    }

    public static bool IsValidSafePath(string? path, out string errorMessage)
    {
        errorMessage = string.Empty;

        if (string.IsNullOrWhiteSpace(path))
        {
            errorMessage = "Path cannot be empty.";
            return false;
        }

        // 1. Check for Path Traversal
        if (path.Contains("..") || path.Contains("./") || path.Contains(".\\"))
        {
            errorMessage = "Path contains traversal segments (.. or .).";
            return false;
        }

        // 2. Check for Illegal Characters
        if (path.Any(c => InvalidPathChars.Contains(c) || char.IsControl(c)))
        {
            errorMessage = "Path contains illegal characters (control characters or \\ : * ? \" < > |).";
            return false;
        }

        // 4. Dot/Space check
        if (path.StartsWith('.') || path.EndsWith('.') || path.EndsWith(' '))
        {
            errorMessage = "Path cannot start or end with a dot, or end with a space.";
            return false;
        }

        // 3. Max Length (Azure Standard is 1024)
        if (path.Length > 1024)
        {
            errorMessage = "Path exceeds maximum length of 1024 characters.";
            return false;
        }

        return true;
    }
}
