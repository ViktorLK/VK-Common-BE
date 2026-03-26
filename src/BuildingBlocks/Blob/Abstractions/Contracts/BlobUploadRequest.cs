using VK.Blocks.Blob.Attributes;

namespace VK.Blocks.Blob.Abstractions.Contracts;

public sealed record BlobUploadRequest(
    [BlobFile] string FileName,
    string ContentType,
    Stream Content,
    IDictionary<string, string>? Metadata = null,
    IDictionary<string, string>? IndexTags = null);
