using System.Collections.Generic;
using VK.Blocks.Core;

namespace VK.Blocks.Storage;

/// <summary>
/// A marker type for the VK.Blocks.Storage building block.
/// </summary>
[VKBlockMarker("Storage", Dependencies = [typeof(VKCoreBlock)])]
public sealed partial class VKStorageBlock
{
    static partial void RegisterBlockCustom(IVKStorageBuilder builder)
    {
    }

    static partial void ValidateBlockCustom(VKStorageOptions options, List<string> failures)
    {
        if (options.MaxFileSizeBytes <= 0)
        {
            failures.Add("MaxFileSizeBytes must be greater than 0.");
        }

        if (options.AllowedExtensions is null || options.AllowedExtensions.Length == 0)
        {
            failures.Add("AllowedExtensions must contain at least one extension.");
        }
    }
}
