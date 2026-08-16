namespace VK.Blocks.Testing.Builders;

/// <summary>
/// Specialized builder for entities with standard identifiers.
/// </summary>
/// <typeparam name="TEntity">The entity type.</typeparam>
/// <typeparam name="TId">The identifier type.</typeparam>
public abstract class VKEntityBuilder<TEntity, TId> : VKTestDataBuilder<TEntity>
    where TEntity : class
{
    private Action<TEntity>? _idSetter;

    /// <summary>
    /// Configures an explicit entity identifier.
    /// </summary>
    /// <param name="id">The identifier value.</param>
    /// <param name="setId">Delegate to assign the identifier to the entity.</param>
    /// <returns>The builder instance for fluent chaining.</returns>
    public VKEntityBuilder<TEntity, TId> WithId(TId id, Action<TEntity, TId> setId)
    {
        _idSetter = entity => setId(entity, id);
        return this;
    }

    /// <summary>
    /// Builds the entity and applies the ID customization if specified.
    /// </summary>
    /// <returns>The constructed entity.</returns>
    public new TEntity Build()
    {
        var entity = base.Build();
        _idSetter?.Invoke(entity);
        return entity;
    }
}
