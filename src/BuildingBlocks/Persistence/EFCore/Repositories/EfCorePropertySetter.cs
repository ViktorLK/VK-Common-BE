#if NET8_0
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Query;
using VK.Blocks.Persistence.Abstractions.Repositories;
using VK.Blocks.Persistence.EFCore.Caches;

namespace VK.Blocks.Persistence.EFCore.Repositories;

/// <summary>
/// Helper class to build property update expressions for EF Core bulk updates.
/// </summary>
/// <typeparam name="TEntity">The entity type.</typeparam>
public class EfCorePropertySetter<TEntity> : IPropertySetter<TEntity> where TEntity : class
{
    #region Fields

    private readonly ParameterExpression _parameter = Expression.Parameter(typeof(SetPropertyCalls<TEntity>));
    private Expression _currentExpressionChain;

    #endregion

    #region Constructors

    /// <summary>
    /// Initializes a new instance of the <see cref="EfCorePropertySetter{TEntity}"/> class.
    /// </summary>
    public EfCorePropertySetter()
    {
        _currentExpressionChain = _parameter;
    }

    #endregion

    #region Public Methods

    /// <inheritdoc />
    public IPropertySetter<TEntity> SetProperty<TProperty>(
        Expression<Func<TEntity, TProperty>> propertyExpression,
        TProperty value)
    {
        var method = EfCoreMethodInfoCache<TEntity>.SetPropertyValueMethod.MakeGenericMethod(typeof(TProperty));

        _currentExpressionChain = Expression.Call(
            _currentExpressionChain,
            method,
            propertyExpression,
            Expression.Constant(value, typeof(TProperty))
        );

        return this;
    }

    /// <inheritdoc />
    public IPropertySetter<TEntity> SetProperty<TProperty>(
        Expression<Func<TEntity, TProperty>> propertyExpression,
        Expression<Func<TEntity, TProperty>> valueExpression)
    {
        var method = EfCoreMethodInfoCache<TEntity>.SetPropertyExpressionMethod.MakeGenericMethod(typeof(TProperty));

        _currentExpressionChain = Expression.Call(
            _currentExpressionChain,
            method,
            propertyExpression,
            valueExpression
        );

        return this;
    }

    /// <summary>
    /// Builds the final LambdaExpression for ExecuteUpdate.
    /// </summary>
    /// <returns>The expression to be passed to ExecuteUpdate.</returns>
    internal Expression<Func<SetPropertyCalls<TEntity>, SetPropertyCalls<TEntity>>> BuildSetPropertyExpression()
    {
        // setPropertyCalls => setPropertyCalls.SetProperty(...).SetProperty(...)
        // Note: EF Core 7+ supports disparate parameters per SetProperty

        return Expression.Lambda<Func<SetPropertyCalls<TEntity>, SetPropertyCalls<TEntity>>>(_currentExpressionChain, _parameter);
    }

    #endregion
}
#endif
