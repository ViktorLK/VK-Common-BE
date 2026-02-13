using System.Linq.Expressions;

namespace VK.Blocks.Persistence.Abstractions.Repositories;

/// <summary>
/// Fluent interface for setting entity properties in bulk update operations.
/// </summary>
public interface IPropertySetter<TEntity> where TEntity : class
{
    /// <summary>
    /// Sets a property to a constant value.
    /// </summary>
    IPropertySetter<TEntity> SetProperty<TProperty>(
        Expression<Func<TEntity, TProperty>> propertyExpression,
        TProperty value);

    /// <summary>
    /// Sets a property using an expression (e.g., computed from other properties).
    /// </summary>
    IPropertySetter<TEntity> SetProperty<TProperty>(
        Expression<Func<TEntity, TProperty>> propertyExpression,
        Expression<Func<TEntity, TProperty>> valueExpression);
}
