namespace VK.Blocks.Testing;

/// <summary>
/// Defines a declarative contract for test classes to provide entities via <c>yield return</c>.
/// Fixtures can automatically batch insert these entities on class initialization and delete them on class disposal.
/// </summary>
public interface IVKClassDeclaredEntities
{
    /// <summary>
    /// Yields entity instances to be seeded into the database before test execution.
    /// </summary>
    /// <returns>An enumerable of entity objects.</returns>
    static abstract IEnumerable<object> GetSeedEntities();
}
