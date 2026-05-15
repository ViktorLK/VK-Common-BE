namespace VK.Blocks.AI.VectorStore;

/// <summary>
/// Defines the high-level contract for a named collection vector store.
/// Following the Industrial SDK pattern (v2).
/// </summary>
public interface IVKAIVectorStore
{
    /// <summary>
    /// Gets a specific collection by name.
    /// </summary>
    /// <typeparam name="T">The type of the document.</typeparam>
    /// <param name="name">The unique name of the collection (maps to a table/index).</param>
    /// <returns>A typed collection proxy.</returns>
    IVKAIVectorCollection<T> Collection<T>(string name) where T : class;
}
