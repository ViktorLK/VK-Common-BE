using System.Linq;
namespace VK.Blocks.Core;

/// <summary>
/// Defines a contract for evaluating a VKSpecification against an IQueryable.
/// </summary>
/// <typeparam name="T">The type of the entity.</typeparam>
public interface IVKSpecificationEvaluator<T> where T : class
{
    /// <summary>
    /// Applies the VKSpecification to the input query.
    /// </summary>
    /// <param name="inputQuery">The original IQueryable.</param>
    /// <param name="VKSpecification">The VKSpecification to apply.</param>
    /// <returns>The modified IQueryable with VKSpecification applied.</returns>
    IQueryable<T> GetQuery(IQueryable<T> inputQuery, IVKSpecification<T> VKSpecification);
}
