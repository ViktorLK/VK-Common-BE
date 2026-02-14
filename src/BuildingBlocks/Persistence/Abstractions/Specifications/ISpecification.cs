
using System.Linq.Expressions;

namespace VK.Blocks.Persistence.Abstractions.Specifications;

/// <summary>
/// Defines a contract for specification pattern to encapsulate query logic.
/// </summary>
/// <typeparam name="T">The type of the entity.</typeparam>
public interface ISpecification<T>
{
    #region Methods

    /// <summary>
    /// Converts the specification to a LINQ expression.
    /// </summary>
    /// <returns>An expression that represents the specification.</returns>
    Expression<Func<T, bool>> ToExpression();

    #endregion
}
