using System.Linq.Expressions;

namespace VK.Blocks.Persistence.Abstractions.Repositories;

/// <summary>
/// Provides a fluent interface for configuring property updates in bulk operations.
/// </summary>
/// <typeparam name="TEntity">The type of the entity being updated.</typeparam>
public interface IPropertySetter<TEntity> where TEntity : class
{
    #region Methods

    /// <summary>
    /// Sets a property to a constant value.
    /// </summary>
    /// <typeparam name="TProperty">The type of the property.</typeparam>
    /// <param name="propertyExpression">An expression selecting the property to update.</param>
    /// <param name="value">The constant value to set.</param>
    /// <returns>The current <see cref="IPropertySetter{TEntity}"/> instance for chaining.</returns>
    IPropertySetter<TEntity> SetProperty<TProperty>(
        Expression<Func<TEntity, TProperty>> propertyExpression,
        TProperty value);

    /// <summary>
    /// Sets a property using a value derived from another expression.
    /// </summary>
    /// <typeparam name="TProperty">The type of the property.</typeparam>
    /// <param name="propertyExpression">An expression selecting the property to update.</param>
    /// <param name="valueExpression">An expression defining the new value (e.g., computed from other properties).</param>
    /// <returns>The current <see cref="IPropertySetter{TEntity}"/> instance for chaining.</returns>
    IPropertySetter<TEntity> SetProperty<TProperty>(
        Expression<Func<TEntity, TProperty>> propertyExpression,
        Expression<Func<TEntity, TProperty>> valueExpression);

    #endregion
}
