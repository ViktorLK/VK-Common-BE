using System.Linq.Expressions;
using System.Reflection;
using Microsoft.EntityFrameworkCore.Query;

namespace VK.Blocks.Persistence.EFCore.Caches;

/// <summary>
/// Cache for MethodInfo of property setter methods.
/// </summary>
internal static class EfCoreMethodInfoCache<TEntity>
{
    #region Fields

    /// <summary>
    /// The MethodInfo for SetProperty(Expression, Value).
    /// </summary>
    public static readonly MethodInfo SetPropertyValueMethod = GetSetPropertyValueMethod();

    /// <summary>
    /// The MethodInfo for SetProperty(Expression, Expression).
    /// </summary>
    public static readonly MethodInfo SetPropertyExpressionMethod = GetSetPropertyExpressionMethod();

    #endregion

    #region Private Methods

    private static MethodInfo GetSetPropertyValueMethod()
    {
        // SetProperty(Expression<Func>, TProperty)
        Expression<Func<SetPropertyCalls<TEntity>, SetPropertyCalls<TEntity>>> probe =
            s => s.SetProperty(e => 0, 0);

        if (probe.Body is MethodCallExpression methodCall)
        {
            return methodCall.Method.GetGenericMethodDefinition();
        }
        throw new InvalidOperationException("Could not detect SetProperty(Expression, Value) method");
    }

    private static MethodInfo GetSetPropertyExpressionMethod()
    {
        // SetProperty(Expression<Func>, Expression<Func>)
        Expression<Func<SetPropertyCalls<TEntity>, SetPropertyCalls<TEntity>>> probe =
            s => s.SetProperty(e => 0, e => 0);

        if (probe.Body is MethodCallExpression methodCall)
        {
            return methodCall.Method.GetGenericMethodDefinition();
        }

        throw new InvalidOperationException("Could not detect SetProperty(Expression, Expression) method");
    }

    #endregion
}
