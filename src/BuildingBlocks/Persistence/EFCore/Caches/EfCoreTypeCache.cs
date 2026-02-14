using VK.Blocks.Persistence.Abstractions.Entities;

namespace VK.Blocks.Persistence.EFCore.Caches;

internal static class EfCoreTypeCache<TEntity>
{
    #region Fields

    public static readonly bool IsAuditable = typeof(IAuditable).IsAssignableFrom(typeof(TEntity));
    public static readonly bool IsSoftDelete = typeof(ISoftDelete).IsAssignableFrom(typeof(TEntity));

    #endregion
}
