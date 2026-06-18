using VK.Blocks.Core;
using VK.Blocks.Persistence.EFCore;

namespace VK.Blocks.Persistence.Sqlite;

/// <summary>
/// A marker type for the VK.Blocks.Persistence.Sqlite building block.
/// </summary>
[VKBlockMarker(Dependencies = [typeof(VKPersistenceEFCoreBlock)])]
public sealed partial class VKPersistenceSqliteBlock;
