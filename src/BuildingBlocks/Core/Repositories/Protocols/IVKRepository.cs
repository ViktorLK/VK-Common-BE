namespace VK.Blocks.Core;

/// <summary>
/// Combined read and write repository protocol for models and data access abstractions.
/// Combines <see cref="IVKReadRepository{T, TId}"/> and <see cref="IVKWriteRepository{T, TId}"/>.
/// </summary>
/// <typeparam name="T">The model or entity type.</typeparam>
/// <typeparam name="TId">The strongly-typed identifier type.</typeparam>
public interface IVKRepository<T, in TId> : IVKReadRepository<T, TId>, IVKWriteRepository<T, TId>
    where TId : notnull
{
}
