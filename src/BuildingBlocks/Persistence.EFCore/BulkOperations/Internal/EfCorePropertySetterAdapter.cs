using System;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Query;
using VK.Blocks.Persistence;

namespace VK.Blocks.Persistence.EFCore.BulkOperations.Internal;

/// <summary>
/// Helper class to build property update expressions for EF Core bulk updates.
/// </summary>
/// <typeparam name="TEntity">The entity type.</typeparam>
internal sealed class EfCorePropertySetterAdapter<TEntity>(UpdateSettersBuilder<TEntity> builder) : IVKPropertySetter<TEntity> where TEntity : class
{
#if NET8_0
#else

    /// <inheritdoc />
    public IVKPropertySetter<TEntity> SetProperty<TProperty>(
        Expression<Func<TEntity, TProperty>> propertyExpression,
        TProperty value)
    {
        builder.SetProperty(propertyExpression, value);
        return this;
    }

    /// <inheritdoc />
    public IVKPropertySetter<TEntity> SetProperty<TProperty>(
        Expression<Func<TEntity, TProperty>> propertyExpression,
        Expression<Func<TEntity, TProperty>> valueExpression)
    {
        builder.SetProperty(propertyExpression, valueExpression);
        return this;
    }

#endif
}
