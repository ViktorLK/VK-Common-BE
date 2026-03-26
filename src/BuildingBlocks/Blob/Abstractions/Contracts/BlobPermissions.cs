namespace VK.Blocks.Blob.Abstractions.Contracts;

[Flags]
public enum BlobPermissions
{
    None = 0,
    Read = 1,
    Write = 2,
    Delete = 4
}
