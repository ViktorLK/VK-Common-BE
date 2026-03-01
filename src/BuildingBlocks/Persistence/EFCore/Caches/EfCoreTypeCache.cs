using VK.Blocks.Core.Primitives;

namespace VK.Blocks.Persistence.EFCore.Caches;

/// <summary>
/// Cache for entity type capabilities (e.g. IsAuditable, IsSoftDelete).
/// </summary>
internal static class EfCoreTypeCache<TEntity>
{
    #region Fields

    /// <summary>
    /// Gets a value indicating whether the entity implements IAuditable.
    /// </summary>
    public static readonly bool IsAuditable = typeof(IAuditable).IsAssignableFrom(typeof(TEntity));

    /// <summary>
    /// Gets a value indicating whether the entity implements ISoftDelete.
    /// </summary>
    public static readonly bool IsSoftDelete = typeof(ISoftDelete).IsAssignableFrom(typeof(TEntity));

    /// <summary>
    /// Gets a value indicating whether the entity implements IMultiTenant.
    /// </summary>
    public static readonly bool IsMultiTenant = typeof(IMultiTenant).IsAssignableFrom(typeof(TEntity));

    /// <summary>
    /// Gets the name of the entity type.
    /// </summary>
    public static readonly string EntityName = typeof(TEntity).Name;

    #endregion
}
