namespace VK.Blocks.Blob.Abstractions.Contracts;

public sealed record BlobSasOptions(
    TimeSpan ExpiresIn,
    BlobPermissions Permissions);
