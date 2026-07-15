using VK.Blocks.Core;

namespace VK.Blocks.Persistence.EFCore.Sqlite;

/// <summary>
/// A marker type for the VK.Blocks.Persistence.Sqlite building block.
/// </summary>
[VKBlockMarker(Dependencies = [typeof(VKPersistenceEFCoreBlock)])]
public sealed partial class VKPersistenceEFCoreSqliteBlock;
