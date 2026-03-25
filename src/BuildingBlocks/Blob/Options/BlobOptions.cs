namespace VK.Blocks.Blob.Options;

public sealed record BlobOptions
{
    public const string SectionName = "BlobStorage";

    public string ConnectionString { get; init; } = string.Empty;

    /// <summary>
    /// The Service URI for Azure Blob Storage (e.g. https://account.blob.core.windows.net).
    /// Used if ConnectionString is empty for Managed Identity (DefaultAzureCredential).
    /// </summary>
    public string? ServiceUri { get; init; }

    public string ContainerName { get; init; } = "default";

    public long MaxFileSizeBytes { get; init; } = 10 * 1024 * 1024; // 10MB default

    public string[] AllowedExtensions { get; init; } = [".jpg", ".jpeg", ".png", ".pdf"];

    /// <summary>
    /// Enables or disables soft-delete related operations.
    /// Requires Soft-Delete to be enabled on the Azure Storage account.
    /// </summary>
    public bool EnableSoftDelete { get; init; } = false;

    /// <summary>
    /// Enables or disables versioning related operations.
    /// Requires Versioning to be enabled on the Azure Storage account.
    /// </summary>
    public bool EnableVersioning { get; init; } = false;
}
