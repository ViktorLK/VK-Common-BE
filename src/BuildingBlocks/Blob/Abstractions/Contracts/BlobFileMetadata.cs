namespace VK.Blocks.Blob.Abstractions.Contracts;

public sealed record BlobFileMetadata(
    string Name,
    long Size,
    string ContentType,
    DateTimeOffset CreatedAt,
    IDictionary<string, string>? Metadata = null);
