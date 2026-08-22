using VK.Blocks.Core;

namespace VK.Blocks.Storage;

public static class VKStorageErrors
{
    public static readonly VKError NotFound = new(
        "Storage.File.NotFound",
        "The specified Storage was not found.",
        VKErrorType.NotFound);

    public static readonly VKError UploadFailed = new(
        "Storage.Upload.Failed",
        "Failed to upload the Storage.",
        VKErrorType.Failure);

    public static readonly VKError FileSizeExceeded = new(
        "Storage.File.SizeExceeded",
        "The file size exceeds the allowed limit.",
        VKErrorType.Validation);

    public static readonly VKError InvalidFileType = new(
        "Storage.File.InvalidType",
        "The file type is not allowed.",
        VKErrorType.Validation);

    public static readonly VKError InvalidFileName = new(
        "Storage.File.InvalidName",
        "The file name is invalid.",
        VKErrorType.Validation);

    public static readonly VKError EmptyContent = new(
        "Storage.File.EmptyContent",
        "Storage content cannot be empty.",
        VKErrorType.Validation);

    public static readonly VKError GeneralFailure = new(
        "Storage.Engine.Failure",
        "An unexpected error occurred in Storage storage.",
        VKErrorType.Failure);

    public static readonly VKError ContainerNotFound = new(
        "Storage.Container.NotFound",
        "The specified container was not found.",
        VKErrorType.NotFound);

    public static readonly VKError LeaseAlreadyHeld = new(
        "Storage.Lease.AlreadyHeld",
        "There is already a lease on the Storage.",
        VKErrorType.Conflict);

    public static readonly VKError LeaseNotFound = new(
        "Storage.Lease.NotFound",
        "The specified lease ID was not found or has expired.",
        VKErrorType.NotFound);

    public static readonly VKError TagOperationFailed = new(
        "Storage.Tag.OperationFailed",
        "The operation on Storage tags failed.",
        VKErrorType.Failure);

    public static readonly VKError VersionNotFound = new(
        "Storage.Version.NotFound",
        "The specified Storage version was not found.",
        VKErrorType.NotFound);

    public static readonly VKError UndeleteFailed = new(
        "Storage.Version.UndeleteFailed",
        "Failed to restore the soft-deleted Storage.",
        VKErrorType.Failure);

    public static readonly VKError FeatureDisabled = new(
        "Storage.Feature.Disabled",
        "This feature is disabled in the configuration. Please check your VKStorageOptions and ensure the feature is enabled in your Storage account.",
        VKErrorType.Validation);

    public static readonly VKError NameConflict = new(
        "Storage.Path.Conflict",
        "A file or directory with the same name already exists (case-insensitive).",
        VKErrorType.Conflict);

    public static readonly VKError InvalidPath = new(
        "Storage.Path.Invalid",
        "The specified path contains illegal characters, path traversal segments, or is too long.",
        VKErrorType.Validation);
}
