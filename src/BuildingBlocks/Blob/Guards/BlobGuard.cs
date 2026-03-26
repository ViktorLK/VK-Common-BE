using VK.Blocks.Blob.Abstractions.Contracts;
using VK.Blocks.Blob.Constants;
using VK.Blocks.Blob.Exceptions;
using VK.Blocks.Blob.Options;

namespace VK.Blocks.Blob.Guards;

public static class BlobGuard
{
    private static readonly char[] InvalidPathChars = ['\\', ':', '*', '?', '"', '<', '>', '|'];

    public static void EnsureValidUpload(BlobUploadRequest request, BlobOptions options)
    {
        if (!IsValidSafePath(request.FileName, out var error))
        {
            throw new BlobValidationException(error, BlobErrors.InvalidPath.Code);
        }

        if (request.FileName.EndsWith(BlobConstants.DirectoryMarker, StringComparison.OrdinalIgnoreCase))
        {
            throw new BlobValidationException("The file name is reserved for directory markers.", BlobErrors.InvalidFileName.Code);
        }

        if (request.Content == null || request.Content.Length == 0)
        {
            throw new BlobValidationException(
                BlobErrors.EmptyContent.Code, 
                BlobErrors.EmptyContent.Description);
        }

        if (request.Content.Length > options.MaxFileSizeBytes)
        {
            throw new BlobValidationException(
                BlobErrors.FileSizeExceeded.Code, 
                $"File size exceeds the limit of {options.MaxFileSizeBytes} bytes.");
        }

        var extension = Path.GetExtension(request.FileName).ToLowerInvariant();
        if (!options.AllowedExtensions.Contains(extension, StringComparer.OrdinalIgnoreCase))
        {
            throw new BlobValidationException(
                BlobErrors.InvalidFileType.Code, 
                $"File extension {extension} is not allowed.");
        }
    }

    public static void EnsureValidDirectory(string directoryPath)
    {
        if (!IsValidSafePath(directoryPath, out var error))
        {
            throw new BlobValidationException(error, BlobErrors.InvalidPath.Code);
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
