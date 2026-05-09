using VK.Blocks.Core;
using VK.Blocks.Infrastructure.Azure;

namespace VK.Blocks.Storage.Azure;

/// <summary>
/// A marker type for the VK.Blocks.Storage.Azure building block.
/// </summary>
[VKBlockMarker("Storage.Azure", Dependencies = [typeof(VKCoreBlock), typeof(VKStorageBlock), typeof(VKInfrastructureAzureBlock)])]
public sealed partial class VKStorageAzureBlock;
