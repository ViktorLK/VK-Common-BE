using System;
using System.Linq.Expressions;

using VK.Blocks.Core.Specifications.Internal;

namespace VK.Blocks.Core;

/// <summary>
/// Represents a composite VKSpecification that performs a logical AND operation.
/// </summary>
/// <typeparam name="T">The type of the entity.</typeparam>
public sealed class VKAndSpecification<T> : VKSpecification<T>
{
    private readonly VKSpecification<T> _left;
    private readonly VKSpecification<T> _right;

    public VKAndSpecification(VKSpecification<T> left, VKSpecification<T> right)
    {
        _left = left;
        _right = right;
    }

    public override Expression<Func<T, bool>>? Criteria
    {
        get
        {
            if (_left.Criteria is null)
                return _right.Criteria;
            if (_right.Criteria is null)
                return _left.Criteria;

            var parameter = Expression.Parameter(typeof(T));
            var leftVisitor = new ReplaceExpressionVisitor(_left.Criteria.Parameters[0], parameter);
            var left = leftVisitor.Visit(_left.Criteria.Body);

            var rightVisitor = new ReplaceExpressionVisitor(_right.Criteria.Parameters[0], parameter);
            var right = rightVisitor.Visit(_right.Criteria.Body);

            return Expression.Lambda<Func<T, bool>>(Expression.AndAlso(left!, right!), parameter);
        }
    }
}
