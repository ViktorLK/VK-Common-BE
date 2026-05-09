using System;
namespace VK.Blocks.Storage;

public sealed record VKStorageLeaseInfo(string LeaseId, DateTimeOffset? ExpiresOn);
