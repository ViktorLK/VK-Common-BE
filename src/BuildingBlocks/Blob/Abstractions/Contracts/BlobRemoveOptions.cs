namespace VK.Blocks.Blob.Abstractions.Contracts;

public sealed record BlobRemoveOptions(
    BlobDeleteMode Mode = BlobDeleteMode.SoftDelete,
    bool IncludeSnapshots = true);
