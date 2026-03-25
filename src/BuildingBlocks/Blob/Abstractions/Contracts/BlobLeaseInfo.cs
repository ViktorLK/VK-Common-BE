namespace VK.Blocks.Blob.Abstractions.Contracts;

public sealed record BlobLeaseInfo(string LeaseId, DateTimeOffset? ExpiresOn);
