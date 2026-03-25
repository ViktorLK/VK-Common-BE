using VK.Blocks.Core.Results;

namespace VK.Blocks.Blob.Constants;

public static class BlobErrors
{
    public static readonly Error NotFound = new(
        "Blob.NotFound", 
        "The specified blob was not found.", 
        ErrorType.NotFound);

    public static readonly Error UploadFailed = new(
        "Blob.UploadFailed", 
        "Failed to upload the blob.", 
        ErrorType.Failure);

    public static readonly Error FileSizeExceeded = new(
        "Blob.FileSizeExceeded", 
        "The file size exceeds the allowed limit.", 
        ErrorType.Validation);

    public static readonly Error InvalidFileType = new(
        "Blob.InvalidFileType", 
        "The file type is not allowed.", 
        ErrorType.Validation);

    public static readonly Error InvalidFileName = new(
        "Blob.InvalidFileName", 
        "The file name is invalid.", 
        ErrorType.Validation);

    public static readonly Error EmptyContent = new(
        "Blob.EmptyContent", 
        "Blob content cannot be empty.", 
        ErrorType.Validation);

    public static readonly Error GeneralFailure = new(
        "Blob.Failure", 
        "An unexpected error occurred in blob storage.", 
        ErrorType.Failure);

    public static readonly Error ContainerNotFound = new(
        "Blob.ContainerNotFound",
        "The specified container was not found.",
        ErrorType.NotFound);

    public static readonly Error LeaseAlreadyHeld = new(
        "Blob.LeaseAlreadyHeld",
        "There is already a lease on the blob.",
        ErrorType.Conflict);

    public static readonly Error LeaseNotFound = new(
        "Blob.LeaseNotFound",
        "The specified lease ID was not found or has expired.",
        ErrorType.NotFound);

    public static readonly Error TagOperationFailed = new(
        "Blob.TagOperationFailed",
        "The operation on blob tags failed.",
        ErrorType.Failure);

    public static readonly Error VersionNotFound = new(
        "Blob.VersionNotFound",
        "The specified blob version was not found.",
        ErrorType.NotFound);

    public static readonly Error UndeleteFailed = new(
        "Blob.UndeleteFailed",
        "Failed to restore the soft-deleted blob.",
        ErrorType.Failure);

    public static readonly Error FeatureDisabled = new(
        "Blob.FeatureDisabled",
        "This feature is disabled in the configuration. Please check your BlobOptions and ensure the feature is enabled in your Azure Storage account.",
        ErrorType.Validation);

    public static readonly Error NameConflict = new(
        "Blob.NameConflict",
        "A file or directory with the same name already exists (case-insensitive).",
        ErrorType.Conflict);

    public static readonly Error InvalidPath = new(
        "Blob.InvalidPath",
        "The specified path contains illegal characters, path traversal segments, or is too long.",
        ErrorType.Validation);
}
