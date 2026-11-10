using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.Core;

namespace VK.Blocks.VectorStore;

/// <summary>
/// Defines startup validation checks for vector tables and columns in relational databases.
/// Verifies extensions, column dimensions, and indexes without creating schema or modifying tables.
/// </summary>
public interface IVKVectorSchemaValidator
{
    /// <summary>
    /// Validates that the underlying storage entity or table has the expected vector dimension and appropriate indexes.
    /// </summary>
    /// <param name="collectionOrTableName">The name of the collection, table, or entity.</param>
    /// <param name="expectedDimensions">The expected dimensional size of the vector column.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A successful Result if the schema is valid; otherwise a Failure Result.</returns>
    Task<VKResult> ValidateSchemaAsync(
        string collectionOrTableName,
        int expectedDimensions,
        CancellationToken cancellationToken = default);
}

