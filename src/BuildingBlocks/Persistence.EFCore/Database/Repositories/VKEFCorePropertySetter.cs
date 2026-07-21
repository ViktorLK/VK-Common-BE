#if NET8_0
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Query;
using VK.Blocks.Persistence;

namespace VK.Blocks.Persistence.EFCore;

/// <summary>
/// Helper class to build property update expressions for EF Core bulk updates.
/// </summary>
/// <typeparam name="TEntity">The entity type.</typeparam>

public sealed class VKEFCorePropertySetter<TEntity> : IVKPropertySetter<TEntity> where TEntity : class
{
    private readonly ParameterExpression _parameter = Expression.Parameter(typeof(SetPropertyCalls<TEntity>));
    private Expression _currentExpressionChain;

    /// <summary>
    /// Initializes a new instance of the <see cref="VKEFCorePropertySetter{TEntity}"/> class.
    /// </summary>
    public VKEFCorePropertySetter()
    {
        _currentExpressionChain = _parameter;
    }

    /// <inheritdoc />
    public IVKPropertySetter<TEntity> SetProperty<TProperty>(
        Expression<Func<TEntity, TProperty>> propertyExpression,
        TProperty value)
    {
        var method = EFCoreMethodInfoCache<TEntity>.SetPropertyValueMethod.MakeGenericMethod(typeof(TProperty));

        _currentExpressionChain = Expression.Call(
            _currentExpressionChain,
            method,
            propertyExpression,
            Expression.Constant(value, typeof(TProperty))
        );

        return this;
    }

    /// <inheritdoc />
    public IVKPropertySetter<TEntity> SetProperty<TProperty>(
        Expression<Func<TEntity, TProperty>> propertyExpression,
        Expression<Func<TEntity, TProperty>> valueExpression)
    {
        var method = EFCoreMethodInfoCache<TEntity>.SetPropertyExpressionMethod.MakeGenericMethod(typeof(TProperty));

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


}
#endif
