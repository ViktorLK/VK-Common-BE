using System.Linq.Expressions;

namespace VK.Blocks.Specifications.Core;

/// <summary>
/// Represents a composite specification that performs a logical OR operation.
/// </summary>
/// <typeparam name="T">The type of the entity.</typeparam>
public sealed class OrSpecification<T> : Specification<T>
{
    private readonly Specification<T> _left;
    private readonly Specification<T> _right;

    public OrSpecification(Specification<T> left, Specification<T> right)
    {
        _left = left;
        _right = right;
    }

    public override Expression<Func<T, bool>>? Criteria
    {
        get
        {
            if (_left.Criteria == null || _right.Criteria == null) return null;

            var parameter = Expression.Parameter(typeof(T));
            var leftVisitor = new ReplaceExpressionVisitor(_left.Criteria.Parameters[0], parameter);
            var left = leftVisitor.Visit(_left.Criteria.Body);

            var rightVisitor = new ReplaceExpressionVisitor(_right.Criteria.Parameters[0], parameter);
            var right = rightVisitor.Visit(_right.Criteria.Body);

            return Expression.Lambda<Func<T, bool>>(Expression.OrElse(left!, right!), parameter);
        }
    }
}
