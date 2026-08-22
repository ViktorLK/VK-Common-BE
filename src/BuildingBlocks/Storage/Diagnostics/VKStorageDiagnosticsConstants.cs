namespace VK.Blocks.Storage;

/// <summary>
/// Diagnostic constants and semantic tokens for the Storage building block.
/// </summary>
public static class VKStorageDiagnosticsConstants
{
    /// <summary>
    /// The diagnostic source name for the Storage block.
    /// </summary>
    public const string SourceName = "VK.Blocks.Storage";

    /// <summary>
    /// Histogram for tracking storage upload operation duration.
    /// </summary>
    public const string UploadDurationName = "storage.upload.duration";

    /// <summary>
    /// Histogram for tracking storage download operation duration.
    /// </summary>
    public const string DownloadDurationName = "storage.download.duration";

    /// <summary>
    /// Counter for tracking storage operation failures.
    /// </summary>
    public const string OperationFailuresName = "storage.operation.failures";

    /// <summary>
    /// Tag key for storage name or blob identifier.
    /// </summary>
    public const string TagStorageName = "storage.name";

    /// <summary>
    /// Tag key for storage container or bucket name.
    /// </summary>
    public const string TagContainerName = "storage.container";

    /// <summary>
    /// Tag key for storage operation type (Upload, Download, Delete, etc.).
    /// </summary>
    public const string TagOperation = "storage.operation";

    /// <summary>
    /// Tag key for error code when an operation fails.
    /// </summary>
    public const string TagErrorCode = "storage.error_code";
}
