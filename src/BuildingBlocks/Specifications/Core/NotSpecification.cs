using System.Linq.Expressions;

namespace VK.Blocks.Specifications.Core;

/// <summary>
/// Represents a composite specification that performs a logical NOT operation.
/// </summary>
/// <typeparam name="T">The type of the entity.</typeparam>
public sealed class NotSpecification<T> : Specification<T>
{
    private readonly Specification<T> _specification;

    public NotSpecification(Specification<T> specification)
    {
        _specification = specification;
    }

    public override Expression<Func<T, bool>>? Criteria
    {
        get
        {
            if (_specification.Criteria == null) return null;

            return Expression.Lambda<Func<T, bool>>(
                Expression.Not(_specification.Criteria.Body),
                _specification.Criteria.Parameters);
        }
    }
}
