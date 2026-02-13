
using System.Linq.Expressions;

namespace VK.Blocks.Persistence.Abstractions.Specifications;

// TODO:
public interface ISpecification<T>
{
    Expression<Func<T, bool>> ToExpression();
}
