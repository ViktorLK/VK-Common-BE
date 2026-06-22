using System.Diagnostics.CodeAnalysis;
using VK.Blocks.Core;

namespace VK.Blocks.VectorStore.Sqlite.SqliteVec.Internal;

/// <summary>
/// Feature marker class for the SQLite vector store feature.
/// </summary>
[ExcludeFromCodeCoverage(Justification = "Feature marker class used for telemetry identification; contains no executable logic.")]
[VKFeatureMarker("SqliteVec", typeof(VKVectorStoreBlock))]
internal sealed partial class SqliteVecFeature;
