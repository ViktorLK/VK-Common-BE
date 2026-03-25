namespace VK.Blocks.Blob.Abstractions.Contracts;

public sealed record BlobEntry(
    string Name,
    string Path,
    bool IsDirectory,
    long? Size = null,
    string? ContentType = null,
    DateTimeOffset? CreatedOn = null,
    IDictionary<string, string>? Metadata = null);
