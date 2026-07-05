using System;
using System.Linq.Expressions;

namespace VK.Blocks.Core;

/// <summary>
/// Represents a composite VKSpecification that performs a logical NOT operation.
/// </summary>
/// <typeparam name="T">The type of the entity.</typeparam>
public sealed class VKNotSpecification<T> : VKSpecification<T>
{
    private readonly VKSpecification<T> _specification;

    public VKNotSpecification(VKSpecification<T> VKSpecification)
    {
        _specification = VKSpecification;
    }

    public override Expression<Func<T, bool>>? Criteria
    {
        get
        {
            if (_specification.Criteria is null)
                return null;

            return Expression.Lambda<Func<T, bool>>(
                Expression.Not(_specification.Criteria.Body),
                _specification.Criteria.Parameters);
        }
    }
}
