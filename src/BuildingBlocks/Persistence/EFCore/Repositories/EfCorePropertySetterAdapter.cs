using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Query;
using VK.Blocks.Persistence.Abstractions.Repositories;

namespace VK.Blocks.Persistence.EFCore.Repositories;

/// <summary>
/// Helper class to build property update expressions for EF Core bulk updates.
/// </summary>
/// <typeparam name="TEntity">The entity type.</typeparam>
internal sealed class EfCorePropertySetterAdapter<TEntity>(UpdateSettersBuilder<TEntity> builder) : IPropertySetter<TEntity> where TEntity : class
{
#if NET8_0
#else
    #region Public Methods

    /// <inheritdoc />
    public IPropertySetter<TEntity> SetProperty<TProperty>(
        Expression<Func<TEntity, TProperty>> propertyExpression,
        TProperty value)
    {
        builder.SetProperty(propertyExpression, value);
        return this;
    }

    /// <inheritdoc />
    public IPropertySetter<TEntity> SetProperty<TProperty>(
        Expression<Func<TEntity, TProperty>> propertyExpression,
        Expression<Func<TEntity, TProperty>> valueExpression)
    {
        builder.SetProperty(propertyExpression, valueExpression);
        return this;
    }

    #endregion
#endif
}
