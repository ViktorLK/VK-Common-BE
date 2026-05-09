using VK.Blocks.Core;

namespace VK.Blocks.Storage;

public interface IVKStorageUriGenerator
{
    VKResult<string> GenerateSasUri(
        string storageName,
        VKStorageSasOptions options);

    VKResult<string> GetPublicUrl(string storageName);
}
