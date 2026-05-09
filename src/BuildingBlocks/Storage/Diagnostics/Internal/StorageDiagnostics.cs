using VK.Blocks.Core;

namespace VK.Blocks.Storage.Diagnostics.Internal;

/// <summary>
/// Defines ActivitySource and Meter for the Storage module using the [VKBlockDiagnostics] source generator.
/// </summary>
[VKBlockDiagnostics<VKStorageBlock>]
internal static partial class StorageDiagnostics
{
    // The Source and Meter properties are automatically generated into a partial class.
}
