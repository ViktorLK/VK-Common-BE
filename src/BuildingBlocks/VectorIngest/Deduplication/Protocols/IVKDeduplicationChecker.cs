using System.Threading;
using System.Threading.Tasks;
using VK.Blocks.Core;

namespace VK.Blocks.VectorIngest; // Flat root namespace for public APIs

/// <summary>
/// Defines the contract for checking if a chunk content hash already exists in the destination storage.
/// </summary>
public interface IVKDeduplicationChecker // [AP.03] public API surface
{
    /// <summary>
    /// Checks if a chunk with the specified content hash is a duplicate.
    /// </summary>
    Task<VKResult<bool>> IsDuplicateAsync(string contentHash, CancellationToken cancellationToken = default); // [CS.01] Result Pattern
}
