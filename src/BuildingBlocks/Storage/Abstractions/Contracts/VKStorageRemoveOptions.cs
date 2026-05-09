namespace VK.Blocks.Storage;

public sealed record VKStorageRemoveOptions(
    VKStorageDeleteMode Mode = VKStorageDeleteMode.SoftDelete,
    bool IncludeSnapshots = true);
