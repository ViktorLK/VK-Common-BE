using System;
namespace VK.Blocks.Storage;

public sealed record VKStorageSasOptions(
    TimeSpan ExpiresIn,
    VKStoragePermissions Permissions);
