#if NET8_0
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.EntityFrameworkCore.Query;

namespace VK.Blocks.Persistence.EFCore.Caches.Internal;

/// <summary>
/// Cache for MethodInfo of property setter methods.
/// </summary>
/// <typeparam name="TEntity">The entity type.</typeparam>
internal static class EfCoreMethodInfoCache<TEntity>
{
    /// <summary>
    /// The MethodInfo for SetProperty(Expression, Value).
    /// </summary>
    public static readonly MethodInfo SetPropertyValueMethod = GetSetPropertyValueMethod();

    /// <summary>
    /// The MethodInfo for SetProperty(Expression, Expression).
    /// </summary>
    public static readonly MethodInfo SetPropertyExpressionMethod = GetSetPropertyExpressionMethod();

    private static MethodInfo GetSetPropertyValueMethod()
    {
        // SetProperty(Expression<Func>, TProperty)
        Expression<Func<SetPropertyCalls<TEntity>, SetPropertyCalls<TEntity>>> probe =
            Constants.SetPropertyProbe;

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
            Constants.SetPropertyExpressionProbe;

        if (probe.Body is MethodCallExpression methodCall)
        {
            return methodCall.Method.GetGenericMethodDefinition();
        }

        throw new InvalidOperationException("Could not detect SetProperty(Expression, Expression) method");
    }

    private static class Constants
    {
        public static readonly Expression<Func<SetPropertyCalls<TEntity>, SetPropertyCalls<TEntity>>> SetPropertyProbe =
            s => s.SetProperty(e => 0, 0);

        public static readonly Expression<Func<SetPropertyCalls<TEntity>, SetPropertyCalls<TEntity>>> SetPropertyExpressionProbe =
            s => s.SetProperty(e => 0, e => 0);
    }
}
#endif
