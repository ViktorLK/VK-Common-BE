namespace VK.Blocks.Core.Utilities.Mapping;

/// <summary>
/// Defines a contract for mapping one type to another.
/// Implement this interface to create a specific mapper for a source and destination type.
/// </summary>
/// <typeparam name="TSource">The source type to be mapped from.</typeparam>
/// <typeparam name="TDestination">The destination type to be mapped to.</typeparam>
public interface IMapper<in TSource, out TDestination>
{
    /// <summary>
    /// Maps the source object to a new object of type <typeparamref name="TDestination"/>.
    /// </summary>
    /// <param name="source">The source object to map.</param>
    /// <returns>A new instance of <typeparamref name="TDestination"/> representing the mapped source.</returns>
    TDestination Map(TSource source);
}

